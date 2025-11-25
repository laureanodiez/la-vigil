using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public enum InteractionType
{
    Automatic,      // Al entrar al trigger (sin botón)
    OnInteract,     // Presionar botón de interacción
    OnCollect,      // Al tocar (para ítems coleccionables)
    Mixed           // Automatic para completar, OnInteract para iniciar
}

[System.Serializable]
public class QuestAction
{
    public enum ActionType { Start, Complete, Toggle }
    public ActionType type = ActionType.Complete;
    public string questID;
    
    public void Execute()
    {
        if (string.IsNullOrEmpty(questID)) return;
        
        switch (type)
        {
            case ActionType.Start:
                QuestManager.Instance.StartQuest(questID);
                break;
            case ActionType.Complete:
                QuestManager.Instance.CompleteQuest(questID);
                break;
            case ActionType.Toggle:
                if (!QuestManager.Instance.IsQuestCompleted(questID))
                {
                    if (!QuestManager.Instance.IsQuestActive(questID))
                        QuestManager.Instance.StartQuest(questID);
                    else
                        QuestManager.Instance.CompleteQuest(questID);
                }
                break;
        }
    }
}

public class QuestInteractor : MonoBehaviour
{
    [Header("Tipo de Interacción")]
    [SerializeField] private InteractionType interactionType = InteractionType.OnInteract;
    
    [Header("Acciones de Misiones")]
    [SerializeField] private List<QuestAction> questActions = new List<QuestAction>();
    
    [Header("Condiciones para Activar")]
    [SerializeField] private List<string> requiredCompletedQuests = new List<string>();
    [SerializeField] private List<string> requiredActiveQuests = new List<string>();
    [SerializeField] private List<string> forbiddenQuests = new List<string>();
    
    [Header("UI de Interacción")]
    [SerializeField] private GameObject questInteractPrompt;
    [SerializeField] private TMP_Text questPromptText;
    [SerializeField] private string customPromptText = "Inspeccionar";
    [SerializeField] private bool changePromptByState = true;
    
    [Header("Configuración")]
    [SerializeField] private bool singleUse = true;
    [SerializeField] private float interactCooldown = 0.5f;
    [SerializeField] private bool requirePlayerFacing = false; // Para interacciones más realistas
    
    [Header("Efectos")]
    [SerializeField] private bool destroyOnComplete = false;
    [SerializeField] private bool disableOnComplete = false;
    [SerializeField] private GameObject activateOnComplete; // Activar otro objeto al completar
    [SerializeField] private GameObject effectPrefab; // Efecto visual al interactuar
    [SerializeField] private AudioClip interactSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private float soundVolume = 1f;
    
    [Header("Eventos")]
    [SerializeField] private UnityEvent onInteractionSuccess;
    [SerializeField] private UnityEvent onInteractionFailed;
    [SerializeField] private UnityEvent onConditionsNotMet;
    [SerializeField] private UnityEvent onPlayerEnter;
    [SerializeField] private UnityEvent onPlayerExit;
    
    // Estado interno
    private bool hasBeenUsed = false;
    private bool playerInRange = false;
    private bool canInteract = true;
    private GameObject playerObject;
    private float lastInteractTime;
    
    private void Start()
    {
        // Configurar el prompt inicial
        if (questInteractPrompt != null)
        {
            questInteractPrompt.SetActive(false);
        }
        
        if (questPromptText != null && !string.IsNullOrEmpty(customPromptText))
        {
            UpdatePromptText();
        }
        
        // Si hay objetos para activar, asegurarse de que estén desactivados
        if (activateOnComplete != null)
        {
            activateOnComplete.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (!playerInRange) return;
        
        // Actualizar texto del prompt según el estado
        if (changePromptByState && questPromptText != null)
        {
            UpdatePromptText();
        }
        
        // Verificar input para interacción
        if (interactionType == InteractionType.OnInteract || 
            interactionType == InteractionType.Mixed)
        {
            if (Input.GetButtonDown("Interact") && canInteract && Time.time - lastInteractTime > interactCooldown)
            {
                AttemptInteraction();
                lastInteractTime = Time.time;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        
        playerObject = collision.gameObject;
        playerInRange = true;
        onPlayerEnter?.Invoke();
        
        if (ShouldShowPrompt())
        {
            ShowInteractPrompt();
        }
        
        if (interactionType == InteractionType.Automatic || 
            interactionType == InteractionType.OnCollect)
        {
            AttemptInteraction();
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        
        playerInRange = false;
        playerObject = null;
        HideInteractPrompt();
        onPlayerExit?.Invoke();
    }
    
    private void AttemptInteraction()
    {
        // Verificar si ya se usó y es de un solo uso
        if (singleUse && hasBeenUsed)
        {
            onInteractionFailed?.Invoke();
            return;
        }
        
        // Verificar condiciones
        if (!CheckConditions())
        {
            onConditionsNotMet?.Invoke();
            PlaySound(interactSound); // Sonido de "no se puede"
            StartCoroutine(FlashPrompt(Color.red));
            return;
        }
        
        // Verificar si el jugador está mirando hacia el objeto (opcional)
        if (requirePlayerFacing && !IsPlayerFacingObject())
        {
            return;
        }
        
        // Ejecutar acciones de misiones
        ExecuteQuestActions();
        
        // Efectos
        PlayEffects();
        
        // Eventos
        onInteractionSuccess?.Invoke();
        
        // Marcar como usado
        hasBeenUsed = true;
        
        // Post-procesamiento
        HandlePostInteraction();
    }
    
    private bool CheckConditions()
    {
        // Verificar misiones completadas requeridas
        foreach (string questID in requiredCompletedQuests)
        {
            if (!QuestManager.Instance.IsQuestCompleted(questID))
                return false;
        }
        
        // Verificar misiones activas requeridas
        foreach (string questID in requiredActiveQuests)
        {
            if (!QuestManager.Instance.IsQuestActive(questID))
                return false;
        }
        
        // Verificar misiones prohibidas
        foreach (string questID in forbiddenQuests)
        {
            if (QuestManager.Instance.IsQuestCompleted(questID) || 
                QuestManager.Instance.IsQuestActive(questID))
                return false;
        }
        
        return true;
    }
    
    private void ExecuteQuestActions()
    {
        foreach (var action in questActions)
        {
            action.Execute();
        }
    }
    
    private void PlayEffects()
    {
        // Efecto visual
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 5f); // Destruir después de 5 segundos
        }
        
        // Sonido
        PlaySound(completeSound ?? interactSound);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, soundVolume);
        }
    }
    
    private void HandlePostInteraction()
    {
        // Ocultar prompt
        if (singleUse)
        {
            HideInteractPrompt();
        }
        
        // Activar otro objeto
        if (activateOnComplete != null)
        {
            activateOnComplete.SetActive(true);
        }
        
        // Destruir o desactivar
        if (destroyOnComplete)
        {
            StartCoroutine(DestroyAfterDelay(0.1f));
        }
        else if (disableOnComplete)
        {
            gameObject.SetActive(false);
        }
    }
    
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    
    private bool ShouldShowPrompt()
    {
        // No mostrar si es automático
        if (interactionType == InteractionType.Automatic)
            return false;
        
        // No mostrar si es de colección directa
        if (interactionType == InteractionType.OnCollect)
            return false;
        
        // No mostrar si ya se usó y es de un solo uso
        if (singleUse && hasBeenUsed)
            return false;
        
        // Verificar condiciones básicas
        return CheckConditions();
    }
    
    private void ShowInteractPrompt()
    {
        if (questInteractPrompt != null)
        {
            questInteractPrompt.SetActive(true);
            UpdatePromptText();
        }
    }
    
    private void HideInteractPrompt()
    {
        if (questInteractPrompt != null)
        {
            questInteractPrompt.SetActive(false);
        }
    }
    
        private void UpdatePromptText()
    {
        if (questPromptText == null) return;
        
        string text = customPromptText;
        
        if (changePromptByState && questActions.Count > 0)
        {
            var firstAction = questActions[0];
            
            if (!string.IsNullOrEmpty(firstAction.questID))
            {
                if (QuestManager.Instance.IsQuestCompleted(firstAction.questID))
                {
                    text = "Completado";
                    questPromptText.color = Color.green;
                }
                else if (QuestManager.Instance.IsQuestActive(firstAction.questID))
                {
                    text = customPromptText + " (En progreso)";
                    questPromptText.color = Color.yellow;
                }
                else
                {
                    questPromptText.color = Color.white;
                }
            }
        }
        
        questPromptText.text = text;
    }
    
    private bool IsPlayerFacingObject()
    {
        if (playerObject == null) return false;
        
        // Obtener la dirección del jugador (asumiendo que usa escala o rotation)
        Vector2 playerDirection = Vector2.right * playerObject.transform.localScale.x;
        Vector2 toObject = (transform.position - playerObject.transform.position).normalized;
        
        // Verificar si está mirando hacia el objeto (dot product)
        return Vector2.Dot(playerDirection.normalized, toObject) > 0.5f;
    }
    
    private IEnumerator FlashPrompt(Color flashColor)
    {
        if (questPromptText == null) yield break;
        
        Color originalColor = questPromptText.color;
        questPromptText.color = flashColor;
        yield return new WaitForSeconds(0.2f);
        questPromptText.color = originalColor;
    }
    
    // Métodos públicos para control externo
    public void ForceInteraction()
    {
        AttemptInteraction();
    }
    
    public void ResetInteractor()
    {
        hasBeenUsed = false;
        canInteract = true;
    }
    
    public void SetCanInteract(bool value)
    {
        canInteract = value;
        if (!value)
        {
            HideInteractPrompt();
        }
        else if (playerInRange && ShouldShowPrompt())
        {
            ShowInteractPrompt();
        }
    }
    
    // Método para debugging
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Mostrar el área de interacción
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            
            if (col is BoxCollider2D box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                Gizmos.DrawCube(box.offset, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius * transform.localScale.x);
            }
        }
    }
    #endif
}