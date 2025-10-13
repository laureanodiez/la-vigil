using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;
using UnityEngine.Audio;
using Unity.Cinemachine;

[System.Serializable]
public class CinematicEffect
{
    public enum EffectType
    {
        // Efectos de luz
        FadeLight,
        FlickerLight,
        LightningFlash,
        ColorShift,
        
        // Efectos de cámara
        CameraShake,
        CameraZoom,
        CameraMove,
        CameraRotate,
        
        // Efectos de pantalla
        ScreenFade,
        ScreenFlash,
        Vignette,
        ChromaticAberration,
        
        // Efectos de audio
        AudioFade,
        PlaySound,
        StopSound,
        MusicTransition,
        
        // Control de tiempo
        TimeScale,
        Pause,
        
        // Utilidades
        Wait,
        TriggerEvent,
        ActivateObject,
        DeactivateObject,
        ForceDialogue
    }
    
    [Header("Configuración Base")]
    public string effectName = "Nuevo Efecto";
    public EffectType type;
    public float duration = 1f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Valores de Luz (FadeLight, FlickerLight, ColorShift)")]
    public float targetIntensity = 0f;
    public Color targetColor = Color.white;
    public float flickerSpeed = 10f;
    
    [Header("Valores de Cámara (Shake, Zoom, Move)")]
    public float shakeIntensity = 1f;
    public float shakeFrequency = 1f;
    public float targetZoom = 60f;
    public Vector3 targetPosition;
    public Vector3 targetRotation;
    
    [Header("Valores de Pantalla (Fade, Flash, Vignette)")]
    public Color screenColor = Color.black;
    public float targetVignetteIntensity = 0.5f;
    public float targetChromaticIntensity = 1f;
    
    [Header("Valores de Audio")]
    public AudioClip audioClip;
    public float targetVolume = 0f;
    public AudioSource audioSource;
    public AudioMixerGroup mixerGroup;
    
    [Header("Valores de Control")]
    public float waitTime = 1f;
    public float targetTimeScale = 0.5f;
    
    [Header("Referencias")]
    public GameObject targetObject;
    public UnityEvent onEffectStart;
    public UnityEvent onEffectComplete;
    public string dialogueSetName;
}

[System.Serializable]
public class CinematicSequence
{
    public string sequenceName = "Nueva Secuencia";
    public bool playOnStart = false;
    public bool loop = false;
    public List<CinematicEffect> effects = new List<CinematicEffect>();
    
    [Header("Condiciones")]
    public List<string> requiredCompletedQuests = new List<string>();
    public List<string> requiredActiveQuests = new List<string>();
    
    [Header("Eventos de Secuencia")]
    public UnityEvent onSequenceStart;
    public UnityEvent onSequenceComplete;
    
    public bool CanPlay()
    {
        if (requiredCompletedQuests.Count > 0 || requiredActiveQuests.Count > 0)
        {
            foreach (string quest in requiredCompletedQuests)
            {
                if (!QuestManager.Instance.IsQuestCompleted(quest))
                    return false;
            }
            
            foreach (string quest in requiredActiveQuests)
            {
                if (!QuestManager.Instance.IsQuestActive(quest))
                    return false;
            }
        }
        return true;
    }
}

public class CinematicEffects : MonoBehaviour
{
    [Header("Referencias Principales")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject screenOverlay; // Canvas para efectos de pantalla
    [SerializeField] private UnityEngine.UI.Image fadeImage; // Imagen para fades
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private Dialogue dialogueSystem;
    
    [Header("Secuencias Cinematográficas")]
    [SerializeField] private List<CinematicSequence> sequences = new List<CinematicSequence>();
    
    [Header("Configuración")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool skipWithInput = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    
    // Estado interno
    private Coroutine currentSequenceCoroutine;
    private bool isPlayingSequence = false;
    private string currentSequenceName = "";
    private Dictionary<string, CinematicSequence> sequenceDict;
    
    // Valores originales para restaurar
    private float originalLightIntensity;
    private Color originalLightColor;
    private float originalCameraSize;
    private Vector3 originalCameraPosition;
    private Vector3 originalCameraRotation;
    private float originalTimeScale;
    private float originalMusicVolume;
    
    // Para el shake de cámara
    private CinemachineBasicMultiChannelPerlin cameraNoise;
    
    void Start()
    {
        // Guardar valores originales
        if (globalLight != null)
        {
            originalLightIntensity = globalLight.intensity;
            originalLightColor = globalLight.color;
        }
        
        if (virtualCamera != null)
        {
            originalCameraSize = virtualCamera.Lens.OrthographicSize;
            originalCameraPosition = virtualCamera.transform.position;
            originalCameraRotation = virtualCamera.transform.eulerAngles;

            // Obtener componente Noise usando la API no genérica y castear
            var compBase = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise);
            cameraNoise = compBase as CinemachineBasicMultiChannelPerlin;

            // Buscar en children (incluye inactivos) por si está en la pipeline oculta
            if (cameraNoise == null)
                cameraNoise = virtualCamera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>(true);

            // Si sigue sin existir, añadirlo preferiblemente al child "Pipeline" si existe, si no al mismo GameObject
            if (cameraNoise == null)
            {
                Transform pipeline = virtualCamera.transform.Find("Pipeline");
                if (pipeline != null)
                    cameraNoise = pipeline.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
                else
                    cameraNoise = virtualCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();

                Debug.Log($"Se añadió CinemachineBasicMultiChannelPerlin a '{cameraNoise.gameObject.name}'");
            }
        }

        
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        originalTimeScale = Time.timeScale;
        
        if (musicSource != null)
            originalMusicVolume = musicSource.volume;
        
        // Crear diccionario de secuencias
        sequenceDict = new Dictionary<string, CinematicSequence>();
        foreach (var seq in sequences)
        {
            if (!sequenceDict.ContainsKey(seq.sequenceName))
                sequenceDict.Add(seq.sequenceName, seq);
                
            // Auto-play si está configurado
            if (seq.playOnStart && seq.CanPlay())
            {
                PlaySequence(seq.sequenceName);
            }
        }
        
        // Asegurar que el overlay esté configurado
        if (screenOverlay == null)
        {
            CreateScreenOverlay();
        }
    }
    
    void Update()
    {
        // Skip de secuencias
        if (skipWithInput && isPlayingSequence && Input.GetKeyDown(skipKey))
        {
            if (!debugMode) // En debug mode no permitir skip
            {
                SkipCurrentSequence();
            }
        }
    }
    
    void CreateScreenOverlay()
    {
        // Crear un canvas para efectos de pantalla si no existe
        GameObject overlayGO = new GameObject("CinematicOverlay");
        overlayGO.transform.SetParent(transform);
        
        Canvas canvas = overlayGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        GameObject fadeGO = new GameObject("FadeImage");
        fadeGO.transform.SetParent(overlayGO.transform);
        fadeImage = fadeGO.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;
        
        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        screenOverlay = overlayGO;
    }
    
    // === MÉTODOS PÚBLICOS ===
    
    public void PlaySequence(string sequenceName)
    {
        if (sequenceDict.TryGetValue(sequenceName, out CinematicSequence sequence))
        {
            if (sequence.CanPlay())
            {
                if (currentSequenceCoroutine != null)
                {
                    StopCoroutine(currentSequenceCoroutine);
                }
                currentSequenceCoroutine = StartCoroutine(PlaySequenceCoroutine(sequence));
            }
            else if (debugMode)
            {
                Debug.LogWarning($"Secuencia '{sequenceName}' no cumple las condiciones para reproducirse");
            }
        }
        else
        {
            Debug.LogError($"Secuencia '{sequenceName}' no encontrada");
        }
    }
    
    public void StopCurrentSequence()
    {
        if (currentSequenceCoroutine != null)
        {
            StopCoroutine(currentSequenceCoroutine);
            isPlayingSequence = false;
            RestoreDefaults();
        }
    }
    
    public void SkipCurrentSequence()
    {
        StopCurrentSequence();
        
        // Ejecutar eventos de complete
        if (sequenceDict.TryGetValue(currentSequenceName, out CinematicSequence sequence))
        {
            sequence.onSequenceComplete?.Invoke();
        }
    }
    
    // === COROUTINES DE SECUENCIA ===
    
    IEnumerator PlaySequenceCoroutine(CinematicSequence sequence)
    {
        isPlayingSequence = true;
        currentSequenceName = sequence.sequenceName;
        
        if (debugMode)
            Debug.Log($"🎬 Iniciando secuencia: {sequence.sequenceName}");
        
        sequence.onSequenceStart?.Invoke();
        
        do
        {
            foreach (var effect in sequence.effects)
            {
                if (debugMode)
                    Debug.Log($"  ▶ Efecto: {effect.effectName} ({effect.type})");
                
                effect.onEffectStart?.Invoke();
                
                yield return StartCoroutine(PlayEffectCoroutine(effect));
                
                effect.onEffectComplete?.Invoke();
            }
        } while (sequence.loop && isPlayingSequence);
        
        sequence.onSequenceComplete?.Invoke();
        
        isPlayingSequence = false;
        currentSequenceName = "";
        
        if (debugMode)
            Debug.Log($"🎬 Secuencia completada: {sequence.sequenceName}");
    }
    
    IEnumerator PlayEffectCoroutine(CinematicEffect effect)
    {
        float elapsedTime = 0f;
        
        // Valores iniciales según el tipo de efecto
        float startIntensity = 0f;
        Color startColor = Color.white;
        float startZoom = 0f;
        Vector3 startPosition = Vector3.zero;
        Vector3 startRotation = Vector3.zero;
        float startVolume = 0f;
        
        // Capturar valores iniciales
        switch (effect.type)
        {
            case CinematicEffect.EffectType.FadeLight:
            case CinematicEffect.EffectType.FlickerLight:
                if (globalLight != null)
                {
                    startIntensity = globalLight.intensity;
                    startColor = globalLight.color;
                }
                break;
                
            case CinematicEffect.EffectType.CameraZoom:
                if (virtualCamera != null)
                    startZoom = virtualCamera.Lens.OrthographicSize;
                break;
                
            case CinematicEffect.EffectType.CameraMove:
                if (virtualCamera != null)
                    startPosition = virtualCamera.transform.position;
                break;
                
            case CinematicEffect.EffectType.CameraRotate:
                if (virtualCamera != null)
                    startRotation = virtualCamera.transform.eulerAngles;
                break;
                
            case CinematicEffect.EffectType.AudioFade:
                if (effect.audioSource != null)
                    startVolume = effect.audioSource.volume;
                else if (musicSource != null)
                    startVolume = musicSource.volume;
                break;
                
            case CinematicEffect.EffectType.ScreenFade:
            case CinematicEffect.EffectType.ScreenFlash:
                if (fadeImage != null)
                    startColor = fadeImage.color;
                break;
        }
        
        // Ejecutar el efecto
        switch (effect.type)
        {
            case CinematicEffect.EffectType.Wait:
                yield return new WaitForSeconds(effect.waitTime);
                break;
                
            case CinematicEffect.EffectType.TriggerEvent:
                // El evento ya se dispara con onEffectStart
                break;
                
            case CinematicEffect.EffectType.PlaySound:
                if (sfxSource != null && effect.audioClip != null)
                {
                    sfxSource.PlayOneShot(effect.audioClip, effect.targetVolume);
                }
                break;
                
            case CinematicEffect.EffectType.ActivateObject:
                if (effect.targetObject != null)
                    effect.targetObject.SetActive(true);
                break;
                
            case CinematicEffect.EffectType.DeactivateObject:
                if (effect.targetObject != null)
                    effect.targetObject.SetActive(false);
                break;
                
            case CinematicEffect.EffectType.ForceDialogue:
                if (dialogueSystem != null && !string.IsNullOrEmpty(effect.dialogueSetName))
                {
                    dialogueSystem.ForceDialogueSet(effect.dialogueSetName);
                    // Esperar a que termine el diálogo
                    //yield return new WaitUntil(() => !dialogueSystem.IsDialogueActive());
                }
                break;
                
            case CinematicEffect.EffectType.TimeScale:
                Time.timeScale = effect.targetTimeScale;
                break;
                
            case CinematicEffect.EffectType.CameraShake:
                if (cameraNoise != null)
                {
                    while (elapsedTime < effect.duration)
                    {
                        float t = effect.curve.Evaluate(elapsedTime / effect.duration);
                        cameraNoise.AmplitudeGain = effect.shakeIntensity * t;
                        cameraNoise.FrequencyGain = effect.shakeFrequency;
                        
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                    cameraNoise.AmplitudeGain = 0;
                }
                break;
                
            case CinematicEffect.EffectType.LightningFlash:
                if (globalLight != null)
                {
                    // Flash rápido de luz
                    float originalInt = globalLight.intensity;
                    globalLight.intensity = effect.targetIntensity;
                    globalLight.color = effect.targetColor;
                    yield return new WaitForSeconds(0.1f);
                    globalLight.intensity = originalInt * 0.5f;
                    yield return new WaitForSeconds(0.05f);
                    globalLight.intensity = effect.targetIntensity;
                    yield return new WaitForSeconds(0.1f);
                    globalLight.intensity = originalInt;
                    globalLight.color = originalLightColor;
                }
                break;
                
            default:
                // Efectos con interpolación
                while (elapsedTime < effect.duration)
                {
                    float t = effect.curve.Evaluate(elapsedTime / effect.duration);
                    
                    ApplyEffectAtTime(effect, t, startIntensity, startColor, startZoom, 
                                     startPosition, startRotation, startVolume);
                    
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                // Asegurar valor final
                ApplyEffectAtTime(effect, 1f, startIntensity, startColor, startZoom, 
                                 startPosition, startRotation, startVolume);
                break;
        }
    }
    
    void ApplyEffectAtTime(CinematicEffect effect, float t, float startIntensity, 
                           Color startColor, float startZoom, Vector3 startPosition, 
                           Vector3 startRotation, float startVolume)
    {
        switch (effect.type)
        {
            case CinematicEffect.EffectType.FadeLight:
                if (globalLight != null)
                {
                    globalLight.intensity = Mathf.Lerp(startIntensity, effect.targetIntensity, t);
                }
                break;
                
            case CinematicEffect.EffectType.ColorShift:
                if (globalLight != null)
                {
                    globalLight.color = Color.Lerp(startColor, effect.targetColor, t);
                }
                break;
                
            case CinematicEffect.EffectType.FlickerLight:
                if (globalLight != null)
                {
                    float flicker = Mathf.Sin(Time.time * effect.flickerSpeed) * 0.5f + 0.5f;
                    globalLight.intensity = Mathf.Lerp(startIntensity, effect.targetIntensity, flicker * t);
                }
                break;
                
            case CinematicEffect.EffectType.CameraZoom:
                if (virtualCamera != null)
                {
                    virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startZoom, effect.targetZoom, t);
                }
                break;
                
            case CinematicEffect.EffectType.CameraMove:
                if (virtualCamera != null)
                {
                    virtualCamera.transform.position = Vector3.Lerp(startPosition, effect.targetPosition, t);
                }
                break;
                
            case CinematicEffect.EffectType.CameraRotate:
                if (virtualCamera != null)
                {
                    virtualCamera.transform.eulerAngles = Vector3.Lerp(startRotation, effect.targetRotation, t);
                }
                break;
                
            case CinematicEffect.EffectType.ScreenFade:
                if (fadeImage != null)
                {
                    Color targetColor = effect.screenColor;
                    targetColor.a = t;
                    fadeImage.color = targetColor;
                }
                break;
                
            case CinematicEffect.EffectType.ScreenFlash:
                if (fadeImage != null)
                {
                    Color targetColor = effect.screenColor;
                    targetColor.a = 1f - t;
                    fadeImage.color = targetColor;
                }
                break;
                
            case CinematicEffect.EffectType.AudioFade:
                AudioSource source = effect.audioSource != null ? effect.audioSource : musicSource;
                if (source != null)
                {
                    source.volume = Mathf.Lerp(startVolume, effect.targetVolume, t);
                }
                break;
                
            case CinematicEffect.EffectType.MusicTransition:
                if (musicSource != null && effect.audioClip != null)
                {
                    if (t < 0.5f)
                    {
                        // Fade out primera mitad
                        musicSource.volume = Mathf.Lerp(startVolume, 0, t * 2);
                    }
                    else
                    {
                        // Cambiar clip y fade in
                        if (musicSource.clip != effect.audioClip)
                        {
                            musicSource.clip = effect.audioClip;
                            musicSource.Play();
                        }
                        musicSource.volume = Mathf.Lerp(0, effect.targetVolume, (t - 0.5f) * 2);
                    }
                }
                break;
        }
    }
    
    void RestoreDefaults()
    {
        if (globalLight != null)
        {
            globalLight.intensity = originalLightIntensity;
            globalLight.color = originalLightColor;
        }
        
        if (virtualCamera != null)
        {
            virtualCamera.Lens.OrthographicSize = originalCameraSize;
            virtualCamera.transform.position = originalCameraPosition;
            virtualCamera.transform.eulerAngles = originalCameraRotation;
            
            if (cameraNoise != null)
                cameraNoise.AmplitudeGain = 0;
        }
        
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
        
        Time.timeScale = originalTimeScale;
        
        if (musicSource != null)
            musicSource.volume = originalMusicVolume;
    }
    
    // === MÉTODOS DE UTILIDAD PÚBLICA ===
    
    public void QuickFadeOut(float duration = 1f)
    {
        CinematicEffect fadeEffect = new CinematicEffect
        {
            type = CinematicEffect.EffectType.ScreenFade,
            duration = duration,
            screenColor = Color.black
        };
        StartCoroutine(PlayEffectCoroutine(fadeEffect));
    }
    
    public void QuickFadeIn(float duration = 1f)
    {
        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
        }
        
        CinematicEffect fadeEffect = new CinematicEffect
        {
            type = CinematicEffect.EffectType.ScreenFlash,
            duration = duration,
            screenColor = Color.black
        };
        StartCoroutine(PlayEffectCoroutine(fadeEffect));
    }
    
    public void QuickShake(float intensity = 1f, float duration = 0.5f)
    {
        CinematicEffect shakeEffect = new CinematicEffect
        {
            type = CinematicEffect.EffectType.CameraShake,
            duration = duration,
            shakeIntensity = intensity,
            shakeFrequency = 10f
        };
        StartCoroutine(PlayEffectCoroutine(shakeEffect));
    }
    
    public void QuickBlackout(float duration = 0.1f)
    {
        CinematicEffect blackoutEffect = new CinematicEffect
        {
            type = CinematicEffect.EffectType.FadeLight,
            duration = duration,
            targetIntensity = 0f
        };
        StartCoroutine(PlayEffectCoroutine(blackoutEffect));
    }
    
    public bool IsPlayingSequence()
    {
        return isPlayingSequence;
    }
    
    // Para el Dialogue.cs
    public bool IsDialogueActive()
    {
        // Este método debería estar en tu Dialogue.cs
        // Lo pongo aquí como placeholder
        return false;
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Test: Fade Out")]
    void TestFadeOut() => QuickFadeOut();
    
    [ContextMenu("Test: Fade In")]
    void TestFadeIn() => QuickFadeIn();
    
    [ContextMenu("Test: Camera Shake")]
    void TestShake() => QuickShake();
    
    [ContextMenu("Test: Blackout")]
    void TestBlackout() => QuickBlackout();
    #endif
}