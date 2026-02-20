using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class DialogueSet
{
    public string setName = "Default";
    public string[] speaker;
    [TextArea(2, 4)]
    public string[] dialogueWords;
    public Sprite[] portrait;
    
    [Header("Audio del Diálogo (opcional)")]
    public AudioClip[] blipSounds; // Sonidos de "blip" para cada línea de diálogo (puede ser el mismo)
    [Range(0.5f, 2f)]
    public float[] blipPitch = new float[] { 1f }; // Pitch para cada línea (opcional)
    
    [Header("Condiciones para activar este set")]
    public List<string> requiredCompletedQuests = new List<string>(); // Tareas que deben estar COMPLETADAS
    public List<string> requiredActiveQuests = new List<string>(); // Tareas que deben estar ACTIVAS (en progreso)
    public List<string> forbiddenQuests = new List<string>(); // Tareas que NO deben estar completadas NI activas
    public int priority = 0; // Para casos donde múltiples sets cumplan condiciones
    
    [Header("Eventos del Set")]
    public UnityEvent onDialogueStart; // Se ejecuta al iniciar este set de diálogo
    public UnityEvent onDialogueEnd; // Se ejecuta al terminar este set de diálogo
    
    public bool CanActivate()
    {
        // Verificar tareas completadas requeridas
        foreach (string quest in requiredCompletedQuests)
        {
            if (!QuestManager.Instance.IsQuestCompleted(quest))
                return false;
        }
        
        // Verificar tareas activas requeridas (para diálogos "en progreso")
        foreach (string quest in requiredActiveQuests)
        {
            if (!QuestManager.Instance.IsQuestActive(quest))
                return false;
        }
        
        // Verificar tareas prohibidas
        foreach (string quest in forbiddenQuests)
        {
            if (QuestManager.Instance.IsQuestCompleted(quest) || 
                QuestManager.Instance.IsQuestActive(quest))
                return false;
        }
        
        return true;
    }
    
    // Obtener el clip de audio para un paso específico
    public AudioClip GetBlipForStep(int step)
    {
        if (blipSounds == null || blipSounds.Length == 0)
            return null;
        
        // Si hay un clip por cada línea, usar el correspondiente
        if (step < blipSounds.Length)
            return blipSounds[step];
        
        // Si hay menos clips que líneas, usar el último disponible
        return blipSounds[blipSounds.Length - 1];
    }
    
    // Obtener el pitch para un paso específico
    public float GetPitchForStep(int step)
    {
        if (blipPitch == null || blipPitch.Length == 0)
            return 1f;
        
        if (step < blipPitch.Length)
            return blipPitch[step];
        
        return blipPitch[blipPitch.Length - 1];
    }
}

public class Dialogue : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private GameObject interactCanvas;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    
    [Header("Configuración de Diálogos")]
    [SerializeField] private List<DialogueSet> dialogueSets = new List<DialogueSet>();
    [SerializeField] private bool rememberLastDialogue = true; // Si debe recordar qué diálogo mostró
    
    [Header("Configuración")]
    public Behaviour[] disableDuringTransition;
    [SerializeField] private float typewriterSpeed = 0.03f; // Velocidad del efecto máquina de escribir
    [SerializeField] private bool useTypewriter = false;

    [Header("Player Control")]
// Referencia directa al componente, no al GameObject
    [SerializeField] private QuimiSpriteAnimator playerMovement;
    
    [Header("Configuración de Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool useDialogueAudio = true;
    [SerializeField] private AudioClip defaultBlipSound; // Sonido por defecto si no se especifica
    [Range(0f, 1f)]
    [SerializeField] private float blipVolume = 0.5f;
    [SerializeField] private int charactersPerBlip = 2; // Cada cuántos caracteres suena (1-3 recomendado)
    [SerializeField] private bool skipSpaces = true; // No sonar en espacios
    [SerializeField] private bool skipPunctuation = true; // No sonar en puntuación al final
    [Range(0f, 0.3f)]
    [SerializeField] private float pitchVariation = 0.05f; // Variación aleatoria del pitch
    
    private DialogueSet currentSet;
    private bool dialogueActivated;
    private int step;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    private string lastUsedSetName = "";
    
    void Start()
    {
        // Asegurarse de que el canvas esté desactivado al inicio
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
        if (interactCanvas != null) interactCanvas.SetActive(false);
        
        // Crear AudioSource si no existe
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configurar AudioSource
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
    
    void Update()
    {
        if (Input.GetButtonDown("Interact") && dialogueActivated)
        {
            HandleDialogueInteraction();
        }
    }
    
    void HandleDialogueInteraction()
    {
        // Si está escribiendo, completar el texto actual
        if (isTyping && typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            dialogueText.text = currentSet.dialogueWords[step - 1];
            isTyping = false;
            return;
        }
        
        // Si no hay diálogo activo, activar el apropiado
        if (currentSet == null)
        {
            SelectAndStartDialogue();
            return;
        }
        
        // Avanzar al siguiente paso del diálogo
        if (step >= currentSet.speaker.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowDialogueStep();
        }
    }
    
    void SelectAndStartDialogue()
    {
        currentSet = GetAppropiateDialogueSet();
        
        if (currentSet == null)
        {
            Debug.LogWarning($"No hay diálogo disponible para mostrar en {gameObject.name}");
            return;
        }
        
        // Ejecutar evento de inicio del set
        currentSet.onDialogueStart?.Invoke();
        
        step = 0;
        ShowDialogueStep();
    }
    
    DialogueSet GetAppropiateDialogueSet()
    {
        // Filtrar sets que cumplen las condiciones y ordenar por prioridad
        var availableSets = dialogueSets
            .Where(set => set.CanActivate())
            .OrderByDescending(set => set.priority)
            .ToList();
        
        if (availableSets.Count == 0)
            return null;
        
        // Si recordamos el último diálogo y sigue siendo válido, usarlo
        if (rememberLastDialogue && !string.IsNullOrEmpty(lastUsedSetName))
        {
            var lastSet = availableSets.FirstOrDefault(s => s.setName == lastUsedSetName);
            if (lastSet != null)
                return lastSet;
        }
        
        // Retornar el de mayor prioridad
        var selectedSet = availableSets.First();
        lastUsedSetName = selectedSet.setName;
        return selectedSet;
    }
    
    void ShowDialogueStep()
    {
        if (playerMovement != null) 
        {
            playerMovement.SetInputActive(false); 
        }
        foreach (var b in disableDuringTransition)
            if (b != null) b.enabled = false;
        
        interactCanvas.SetActive(false);
        dialogueCanvas.SetActive(true);
        
        // Actualizar UI
        speakerText.text = currentSet.speaker[step];
        portraitImage.sprite = currentSet.portrait[step];
        
        // Mostrar texto con o sin efecto
        if (useTypewriter)
        {
            typewriterCoroutine = StartCoroutine(TypewriterEffect(currentSet.dialogueWords[step], step));
        }
        else
        {
            dialogueText.text = currentSet.dialogueWords[step];
        }
        
        step++;
    }
    
    IEnumerator TypewriterEffect(string text, int currentStep)
    {
        isTyping = true;
        dialogueText.text = "";
        
        // Obtener el audio y pitch para este paso
        AudioClip blipClip = currentSet.GetBlipForStep(currentStep);
        if (blipClip == null) blipClip = defaultBlipSound;
        
        float basePitch = currentSet.GetPitchForStep(currentStep);
        
        int charCount = 0;
        
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            
            // Reproducir sonido si corresponde
            if (useDialogueAudio && blipClip != null && ShouldPlayBlip(letter, charCount))
            {
                PlayBlip(blipClip, basePitch);
            }
            
            charCount++;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }
    
    bool ShouldPlayBlip(char character, int charCount)
    {
        // Solo sonar cada X caracteres
        if (charCount % charactersPerBlip != 0)
            return false;
        
        // Saltar espacios si está configurado
        if (skipSpaces && char.IsWhiteSpace(character))
            return false;
        
        // Saltar puntuación si está configurado
        if (skipPunctuation && char.IsPunctuation(character))
            return false;
        
        return true;
    }
    
    void PlayBlip(AudioClip clip, float basePitch)
    {
        if (audioSource == null || clip == null) return;
        
        // Aplicar pitch base + variación aleatoria
        audioSource.pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip, blipVolume);
    }
    
    void EndDialogue()
    {
        // Ejecutar evento de fin del set antes de limpiar
        if (currentSet != null)
        {
            currentSet.onDialogueEnd?.Invoke();
        }
        
        dialogueCanvas.SetActive(false);
        currentSet = null;
        step = 0;

        if (playerMovement != null) 
        {
            playerMovement.SetInputActive(true); 
        }        
        foreach (var b in disableDuringTransition)
            if (b != null) b.enabled = true;
        
        // Si el jugador sigue en el trigger, mostrar el icono de interacción
        if (dialogueActivated)
            interactCanvas.SetActive(true);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Verificar si hay algún diálogo disponible antes de mostrar el icono
            var availableSet = GetAppropiateDialogueSet();
            if (availableSet != null)
            {
                interactCanvas.SetActive(true);
                dialogueActivated = true;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactCanvas.SetActive(false);
            dialogueActivated = false;
            
            // Si hay diálogo activo, cerrarlo
            if (currentSet != null)
            {
                EndDialogue();
            }
        }
    }
    
    // Método público para forzar un diálogo específico
    public void ForceDialogueSet(string setName)
    {
        var forcedSet = dialogueSets.FirstOrDefault(s => s.setName == setName);
        if (forcedSet != null)
        {
            currentSet = forcedSet;
            step = 0;
            ShowDialogueStep();
        }
    }
    
    // Método para añadir sets de diálogo dinámicamente
    public void AddDialogueSet(DialogueSet newSet)
    {
        if (!dialogueSets.Any(s => s.setName == newSet.setName))
        {
            dialogueSets.Add(newSet);
        }
    }
    
    // Métodos públicos para controlar el audio
    public void SetDialogueAudioEnabled(bool enabled)
    {
        useDialogueAudio = enabled;
    }
    
    public void SetBlipVolume(float volume)
    {
        blipVolume = Mathf.Clamp01(volume);
    }
}