using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TelescopeController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas telescopeCanvas;
    [SerializeField] private Image scopeOverlay; // Tu imagen de PS con la mirilla
    
    [Header("Camera 2D")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomedOrthographicSize = 2f; // Para 2D usamos Orthographic Size
    private float originalOrthographicSize;
    
    [Header("Lighting 2D")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D[] lights2D; // Para URP 2D
    // O si usas lighting tradicional:
    [SerializeField] private Light[] sceneLights;
    [SerializeField] private float lightFadeSpeed = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // Para sonidos inquietantes
    [SerializeField] private AudioClip[] eerieNoises;
    [SerializeField] private AudioSource[] audioSourcesToDisable; // Pasos de Quime, música, etc.
    
    [Header("Environment Changes")]
    [SerializeField] private GameObject[] objectsToActivate;
    [SerializeField] private GameObject[] objectsToDeactivate;
    [SerializeField] private SpriteRenderer[] spritesToChange; // Sprites que cambiarán
    [SerializeField] private Sprite[] newSprites; // Nuevos sprites (mismo orden que spritesToChange)
    [SerializeField] private Transform[] objectsToMove;
    [SerializeField] private Vector3[] newPositions;
    
    [Header("Timing")]
    [SerializeField] private float viewDuration = 10f;
    [SerializeField] private float darknessDelay = 2f;
    
    [Header("Player Control")]
    [SerializeField] private MonoBehaviour playerMovementScript; // Tu script de movimiento del jugador
    
    [Header("Quest Integration")]
    [SerializeField] private GameObject questManagerObject; // GameObject con QuestManager
    [SerializeField] private string questToComplete = ""; // Nombre de la quest a completar
    
    private bool isViewing = false;
    private float[] originalLightIntensities;
    private Sprite[] originalSprites;
    private bool[] audioSourcesWerePlaying; // Para saber cuáles reactivar

    void Start()
    {
        // Guardar valores originales
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        // Guardar el tamaño ortográfico original (para 2D)
        if (mainCamera.orthographic)
        {
            originalOrthographicSize = mainCamera.orthographicSize;
        }
        
        // Guardar intensidades originales de luces 2D (URP)
        if (lights2D != null && lights2D.Length > 0)
        {
            originalLightIntensities = new float[lights2D.Length];
            for (int i = 0; i < lights2D.Length; i++)
            {
                if (lights2D[i] != null)
                    originalLightIntensities[i] = lights2D[i].intensity;
            }
        }
        // O luces tradicionales
        else if (sceneLights != null && sceneLights.Length > 0)
        {
            originalLightIntensities = new float[sceneLights.Length];
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i] != null)
                    originalLightIntensities[i] = sceneLights[i].intensity;
            }
        }
        
        // Guardar sprites originales
        if (spritesToChange != null && spritesToChange.Length > 0)
        {
            originalSprites = new Sprite[spritesToChange.Length];
            for (int i = 0; i < spritesToChange.Length; i++)
            {
                if (spritesToChange[i] != null)
                    originalSprites[i] = spritesToChange[i].sprite;
            }
        }
        
        // Inicializar array para controlar AudioSources
        if (audioSourcesToDisable != null && audioSourcesToDisable.Length > 0)
        {
            audioSourcesWerePlaying = new bool[audioSourcesToDisable.Length];
        }
        
        // Desactivar UI del telescopio al inicio
        if (telescopeCanvas != null)
            telescopeCanvas.gameObject.SetActive(false);
    }

    public void UseTelescope()
    {
        if (!isViewing)
        {
            StartCoroutine(TelescopeSequence());
        }
    }

    private IEnumerator TelescopeSequence()
    {
        isViewing = true;
        
        // 1. Activar vista del telescopio
        ActivateTelescopeView();
        
        // 2. Desactivar AudioSources (música, pasos, etc.)
        DisableAudioSources();
        
        // 3. Esperar el tiempo configurado
        yield return new WaitForSeconds(viewDuration);
        
        // 4. Salir de la vista del telescopio
        DeactivateTelescopeView();
        
        // 5. Apagar luces gradualmente
        yield return StartCoroutine(FadeLights(false));
        
        // 6. Reproducir sonidos inquietantes
        PlayEerieSounds();
        
        // 7. Esperar en la oscuridad
        yield return new WaitForSeconds(darknessDelay);
        
        // 8. Cambiar el ambiente
        ChangeEnvironment();
        
        // 9. Encender luces gradualmente
        yield return StartCoroutine(FadeLights(true));
        
        // 10. Reactivar AudioSources
        EnableAudioSources();
        
        // 11. Completar quest si está configurada
        CompleteQuest();
        
        isViewing = false;
    }

    private void ActivateTelescopeView()
    {
        // Activar Canvas con tu imagen del scope
        if (telescopeCanvas != null)
            telescopeCanvas.gameObject.SetActive(true);
        
        // Hacer zoom en 2D (cambiar orthographic size)
        if (mainCamera != null && mainCamera.orthographic)
        {
            mainCamera.orthographicSize = zoomedOrthographicSize;
        }
        
        // Desactivar movimiento del jugador
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
        
        Debug.Log("Telescopio activado - Mirando a través del lente");
    }

    private void DeactivateTelescopeView()
    {
        // Desactivar Canvas
        if (telescopeCanvas != null)
            telescopeCanvas.gameObject.SetActive(false);
        
        // Restaurar Orthographic Size
        if (mainCamera != null && mainCamera.orthographic)
        {
            mainCamera.orthographicSize = originalOrthographicSize;
        }
        
        // Reactivar movimiento del jugador
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
        
        Debug.Log("Telescopio desactivado");
    }

    private void DisableAudioSources()
    {
        if (audioSourcesToDisable == null || audioSourcesToDisable.Length == 0)
            return;
        
        for (int i = 0; i < audioSourcesToDisable.Length; i++)
        {
            if (audioSourcesToDisable[i] != null)
            {
                // Guardar si estaba reproduciendo
                audioSourcesWerePlaying[i] = audioSourcesToDisable[i].isPlaying;
                
                // Detener y desactivar el componente completo
                audioSourcesToDisable[i].Stop();
                audioSourcesToDisable[i].enabled = false; // Desactivar el componente
                Debug.Log($"AudioSource desactivado: {audioSourcesToDisable[i].gameObject.name}");
            }
        }
    }

    private void EnableAudioSources()
    {
        if (audioSourcesToDisable == null || audioSourcesToDisable.Length == 0)
            return;
        
        for (int i = 0; i < audioSourcesToDisable.Length; i++)
        {
            if (audioSourcesToDisable[i] != null)
            {
                // Reactivar el componente
                audioSourcesToDisable[i].enabled = true;
                
                // Solo reproducir si estaba sonando antes (para música de fondo)
                if (audioSourcesWerePlaying[i])
                {
                    audioSourcesToDisable[i].Play();
                    Debug.Log($"AudioSource reactivado: {audioSourcesToDisable[i].gameObject.name}");
                }
            }
        }
    }

    private IEnumerator FadeLights(bool fadeIn)
    {
        float elapsed = 0f;
        float duration = 1f / lightFadeSpeed;
        
        // Fade para luces 2D (URP)
        if (lights2D != null && lights2D.Length > 0)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                for (int i = 0; i < lights2D.Length; i++)
                {
                    if (lights2D[i] != null)
                    {
                        if (fadeIn)
                        {
                            lights2D[i].intensity = Mathf.Lerp(0f, originalLightIntensities[i], t);
                        }
                        else
                        {
                            lights2D[i].intensity = Mathf.Lerp(originalLightIntensities[i], 0f, t);
                        }
                    }
                }
                
                yield return null;
            }
            
            // Asegurar valores finales
            for (int i = 0; i < lights2D.Length; i++)
            {
                if (lights2D[i] != null)
                {
                    lights2D[i].intensity = fadeIn ? originalLightIntensities[i] : 0f;
                }
            }
        }
        // Fade para luces tradicionales
        else if (sceneLights != null && sceneLights.Length > 0)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                for (int i = 0; i < sceneLights.Length; i++)
                {
                    if (sceneLights[i] != null)
                    {
                        if (fadeIn)
                        {
                            sceneLights[i].intensity = Mathf.Lerp(0f, originalLightIntensities[i], t);
                        }
                        else
                        {
                            sceneLights[i].intensity = Mathf.Lerp(originalLightIntensities[i], 0f, t);
                        }
                    }
                }
                
                yield return null;
            }
            
            // Asegurar valores finales
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i] != null)
                {
                    sceneLights[i].intensity = fadeIn ? originalLightIntensities[i] : 0f;
                }
            }
        }
    }

    private void PlayEerieSounds()
    {
        if (audioSource != null && eerieNoises != null && eerieNoises.Length > 0)
        {
            // Elegir un sonido aleatorio
            AudioClip randomClip = eerieNoises[Random.Range(0, eerieNoises.Length)];
            audioSource.PlayOneShot(randomClip);
        }
    }

    private void ChangeEnvironment()
    {
        // Activar objetos
        if (objectsToActivate != null)
        {
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
        
        // Desactivar objetos
        if (objectsToDeactivate != null)
        {
            foreach (GameObject obj in objectsToDeactivate)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
        
        // Cambiar sprites (útil para cambiar decoración, paredes, etc.)
        if (spritesToChange != null && newSprites != null)
        {
            int count = Mathf.Min(spritesToChange.Length, newSprites.Length);
            for (int i = 0; i < count; i++)
            {
                if (spritesToChange[i] != null && newSprites[i] != null)
                {
                    spritesToChange[i].sprite = newSprites[i];
                }
            }
        }
        
        // Mover objetos
        if (objectsToMove != null && newPositions != null)
        {
            int count = Mathf.Min(objectsToMove.Length, newPositions.Length);
            for (int i = 0; i < count; i++)
            {
                if (objectsToMove[i] != null)
                {
                    objectsToMove[i].position = newPositions[i];
                }
            }
        }
        
        Debug.Log("Ambiente cambiado - ¡Algo extraño ha ocurrido!");
    }

    private void CompleteQuest()
    {
        // Solo intentar completar si hay un QuestManager asignado y un nombre de quest
        if (questManagerObject != null && !string.IsNullOrEmpty(questToComplete))
        {
            QuestManager questManager = questManagerObject.GetComponent<QuestManager>();
            
            if (questManager != null)
            {
                questManager.CompleteQuest(questToComplete);
                Debug.Log($"Quest completada: {questToComplete}");
            }
            else
            {
                Debug.LogWarning("No se encontró el componente QuestManager en el GameObject asignado.");
            }
        }
    }

    // Método opcional para salir antes con Escape
    void Update()
    {
        if (isViewing && Input.GetKeyDown(KeyCode.Escape))
        {
            StopAllCoroutines();
            DeactivateTelescopeView();
            EnableAudioSources();
            StartCoroutine(FadeLights(true));
            isViewing = false;
        }
    }

    // Si restauras el ambiente (para testing)
    public void RestoreOriginalEnvironment()
    {
        // Restaurar sprites
        if (spritesToChange != null && originalSprites != null)
        {
            for (int i = 0; i < Mathf.Min(spritesToChange.Length, originalSprites.Length); i++)
            {
                if (spritesToChange[i] != null && originalSprites[i] != null)
                {
                    spritesToChange[i].sprite = originalSprites[i];
                }
            }
        }
    }
}