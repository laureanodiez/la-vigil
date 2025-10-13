using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class QuestData
{
    public string questID;
    public string questName;
    [TextArea(2, 3)]
    public string description;
    public bool isCompleted = false;
    public bool isActive = false;
    
    [Header("Eventos")]
    public UnityEvent onQuestStart;
    public UnityEvent onQuestComplete;
    
    public QuestData(string id, string name, string desc = "")
    {
        questID = id;
        questName = name;
        description = desc;
        isCompleted = false;
        isActive = false;
    }
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    [Header("Lista de Tareas")]
    [SerializeField] private List<QuestData> allQuests = new List<QuestData>();
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Diccionario para acceso rápido
    private Dictionary<string, QuestData> questDictionary;
    
    void Awake()
    {
        // Si este QuestManager es para una escena específica
        if (gameObject.name == "LocalQuestManager")
        {
            // NO hacer singleton, permitir múltiples
            Instance = this;
            InitializeQuests();
        }
        else
        {
            // Comportamiento normal (global)
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeQuests();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    void InitializeQuests()
    {
        questDictionary = new Dictionary<string, QuestData>();
        
        foreach (var quest in allQuests)
        {
            if (!questDictionary.ContainsKey(quest.questID))
            {
                questDictionary.Add(quest.questID, quest);
            }
            else
            {
                Debug.LogWarning($"Quest ID duplicado: {quest.questID}");
            }
        }
        
        // Cargar estado guardado si existe
        LoadQuestProgress();
    }
    
    // === MÉTODOS PRINCIPALES ===
    
    public bool IsQuestCompleted(string questID)
    {
        if (string.IsNullOrEmpty(questID))
            return false;
            
        if (questDictionary.TryGetValue(questID, out QuestData quest))
        {
            return quest.isCompleted;
        }
        
        if (showDebugLogs)
            Debug.LogWarning($"Quest no encontrada: {questID}");
        
        return false;
    }
    
    public bool IsQuestActive(string questID)
    {
        if (questDictionary.TryGetValue(questID, out QuestData quest))
        {
            return quest.isActive && !quest.isCompleted;
        }
        return false;
    }
    
    public void StartQuest(string questID)
    {
        if (questDictionary.TryGetValue(questID, out QuestData quest))
        {
            if (!quest.isActive && !quest.isCompleted)
            {
                quest.isActive = true;
                quest.onQuestStart?.Invoke();
                
                if (showDebugLogs)
                    Debug.Log($"Tarea iniciada: {quest.questName}");
                
                SaveQuestProgress();
            }
        }
        else
        {
            Debug.LogWarning($"No se puede iniciar la tarea: {questID} no existe");
        }
    }
    
    public void CompleteQuest(string questID)
    {
        if (questDictionary.TryGetValue(questID, out QuestData quest))
        {
            if (!quest.isCompleted)
            {
                quest.isCompleted = true;
                quest.isActive = false;
                quest.onQuestComplete?.Invoke();
                
                if (showDebugLogs)
                    Debug.Log($"Tarea completada: {quest.questName}");
                
                SaveQuestProgress();
            }
        }
        else
        {
            Debug.LogWarning($"No se puede completar la tarea: {questID} no existe");
        }
    }
    
    public void ResetQuest(string questID)
    {
        if (questDictionary.TryGetValue(questID, out QuestData quest))
        {
            quest.isCompleted = false;
            quest.isActive = false;
            
            if (showDebugLogs)
                Debug.Log($"Tarea reiniciada: {quest.questName}");
            
            SaveQuestProgress();
        }
    }
    
    // === MÉTODOS DE UTILIDAD ===
    
    public List<QuestData> GetActiveQuests()
    {
        List<QuestData> activeQuests = new List<QuestData>();
        foreach (var quest in questDictionary.Values)
        {
            if (quest.isActive && !quest.isCompleted)
            {
                activeQuests.Add(quest);
            }
        }
        return activeQuests;
    }
    
    public List<QuestData> GetCompletedQuests()
    {
        List<QuestData> completedQuests = new List<QuestData>();
        foreach (var quest in questDictionary.Values)
        {
            if (quest.isCompleted)
            {
                completedQuests.Add(quest);
            }
        }
        return completedQuests;
    }
    
    public QuestData GetQuestData(string questID)
    {
        questDictionary.TryGetValue(questID, out QuestData quest);
        return quest;
    }
    
    // === PERSISTENCIA ===
    
    void SaveQuestProgress()
    {
        foreach (var quest in questDictionary.Values)
        {
            PlayerPrefs.SetInt($"Quest_{quest.questID}_Completed", quest.isCompleted ? 1 : 0);
            PlayerPrefs.SetInt($"Quest_{quest.questID}_Active", quest.isActive ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
    
    void LoadQuestProgress()
    {
        foreach (var quest in questDictionary.Values)
        {
            quest.isCompleted = PlayerPrefs.GetInt($"Quest_{quest.questID}_Completed", 0) == 1;
            quest.isActive = PlayerPrefs.GetInt($"Quest_{quest.questID}_Active", 0) == 1;
        }
    }
    
    public void ResetAllQuests()
    {
        foreach (var quest in questDictionary.Values)
        {
            quest.isCompleted = false;
            quest.isActive = false;
        }
        
        // Limpiar PlayerPrefs
        foreach (var quest in questDictionary.Values)
        {
            PlayerPrefs.DeleteKey($"Quest_{quest.questID}_Completed");
            PlayerPrefs.DeleteKey($"Quest_{quest.questID}_Active");
        }
        PlayerPrefs.Save();
        
        if (showDebugLogs)
            Debug.Log("Todas las tareas han sido reiniciadas");
    }
    
    // === MÉTODOS DE CONVENIENCIA PARA CONDICIONES COMPLEJAS ===
    
    public bool AreAllQuestsCompleted(List<string> questIDs)
    {
        foreach (string id in questIDs)
        {
            if (!IsQuestCompleted(id))
                return false;
        }
        return true;
    }
    
    public bool IsAnyQuestCompleted(List<string> questIDs)
    {
        foreach (string id in questIDs)
        {
            if (IsQuestCompleted(id))
                return true;
        }
        return false;
    }
    
    public int GetCompletedQuestCount(List<string> questIDs)
    {
        int count = 0;
        foreach (string id in questIDs)
        {
            if (IsQuestCompleted(id))
                count++;
        }
        return count;
    }
    
    // === MÉTODOS PARA TESTING EN EL EDITOR ===
    
    #if UNITY_EDITOR
    [ContextMenu("Completar Todas las Tareas Activas")]
    void CompleteAllActiveQuests()
    {
        foreach (var quest in GetActiveQuests())
        {
            CompleteQuest(quest.questID);
        }
    }

//   [ContextMenu("Reinicia todas las tareas")]
// void ResetAllQuests()
  //  {
  //      foreach (var quest in questDictionary.Values)
      //  {
          //  quest.isCompleted = false;
 //           quest.isActive = false;
   //     }
   // }

    [ContextMenu("Activar Todas las Tareas")]
    void ActivateAllQuests()
    {
        foreach (var quest in questDictionary.Values)
        {
            if (!quest.isCompleted)
                StartQuest(quest.questID);
        }
    }
    #endif
}