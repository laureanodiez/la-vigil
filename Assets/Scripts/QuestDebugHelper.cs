using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuestDebugHelper : MonoBehaviour
{
    [Header("Debug Options")]
    [SerializeField] private bool resetQuestsOnStart = true; // ⬅️ ACTIVAR ESTO MIENTRAS DESARROLLÁS
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private KeyCode debugMenuKey = KeyCode.F1;
    
    [Header("UI References (Optional)")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TMP_Text questStatusText;
    
    [Header("Quick Actions")]
    [SerializeField] private List<string> questsToMonitor = new List<string>();
    
    private bool debugMenuOpen = false;
    
    void Start()
    {
        // RESETEAR TODAS LAS QUESTS AL INICIAR (solo en desarrollo)
        if (resetQuestsOnStart && Application.isEditor)
        {
            Debug.Log("🔄 <color=yellow>RESETEANDO TODAS LAS QUESTS (Debug Mode)</color>");
            QuestManager.Instance.ResetAllQuests();
        }
        
        if (debugPanel != null)
            debugPanel.SetActive(false);
    }
    
    void Update()
    {
        // Toggle menú de debug
        if (Input.GetKeyDown(debugMenuKey))
        {
            ToggleDebugMenu();
        }
        
        // Shortcuts de debug
        if (Application.isEditor)
        {
            // R = Reset todas las quests
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
            {
                ResetAllQuests();
            }
            
            // C = Completar quests activas
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
                CompleteActiveQuests();
            }
            
            // L = Log estado de quests
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
            {
                LogQuestStatus();
            }
        }
        
        // Actualizar UI de debug si está visible
        if (showDebugUI && debugPanel != null && debugPanel.activeSelf)
        {
            UpdateDebugUI();
        }
    }
    
    void ToggleDebugMenu()
    {
        if (debugPanel == null) return;
        
        debugMenuOpen = !debugMenuOpen;
        debugPanel.SetActive(debugMenuOpen);
        
        // Pausar el juego cuando el menú está abierto (opcional)
        // Time.timeScale = debugMenuOpen ? 0 : 1;
    }
    
    void UpdateDebugUI()
    {
        if (questStatusText == null || QuestManager.Instance == null) return;
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>== QUEST STATUS ==</b>\n");
        
        // Mostrar quests monitoreadas
        if (questsToMonitor.Count > 0)
        {
            sb.AppendLine("<color=cyan>Monitored Quests:</color>");
            foreach (string questID in questsToMonitor)
            {
                var quest = QuestManager.Instance.GetQuestData(questID);
                if (quest != null)
                {
                    string status = quest.isCompleted ? "<color=green>✓ COMPLETE</color>" : 
                                  quest.isActive ? "<color=yellow>◉ ACTIVE</color>" : 
                                                  "<color=gray>○ INACTIVE</color>";
                    sb.AppendLine($"  {questID}: {status}");
                }
            }
            sb.AppendLine();
        }
        
        // Mostrar quests activas
        var activeQuests = QuestManager.Instance.GetActiveQuests();
        if (activeQuests.Count > 0)
        {
            sb.AppendLine("<color=yellow>Active Quests:</color>");
            foreach (var quest in activeQuests)
            {
                sb.AppendLine($"  • {quest.questID}");
            }
            sb.AppendLine();
        }
        
        // Mostrar quests completadas recientemente
        var completedQuests = QuestManager.Instance.GetCompletedQuests();
        if (completedQuests.Count > 0 && completedQuests.Count <= 5)
        {
            sb.AppendLine("<color=green>Recently Completed:</color>");
            foreach (var quest in completedQuests)
            {
                sb.AppendLine($"  ✓ {quest.questID}");
            }
        }
        else if (completedQuests.Count > 5)
        {
            sb.AppendLine($"<color=green>Completed: {completedQuests.Count} quests</color>");
        }
        
        sb.AppendLine("\n<size=10><color=gray>Press Shift+R to reset all</color></size>");
        
        questStatusText.text = sb.ToString();
    }
    
    // === MÉTODOS PÚBLICOS PARA BOTONES UI ===
    
    public void ResetAllQuests()
    {
        QuestManager.Instance.ResetAllQuests();
        Debug.Log("✅ <color=green>Todas las quests reseteadas</color>");
        
        // Opcional: Recargar la escena para resetear objetos
        if (Application.isEditor)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }
    
    public void CompleteActiveQuests()
    {
        var activeQuests = QuestManager.Instance.GetActiveQuests();
        foreach (var quest in activeQuests)
        {
            QuestManager.Instance.CompleteQuest(quest.questID);
            Debug.Log($"✅ Completada: {quest.questID}");
        }
    }
    
    public void StartQuest(string questID)
    {
        QuestManager.Instance.StartQuest(questID);
        Debug.Log($"▶️ Iniciada: {questID}");
    }
    
    public void CompleteQuest(string questID)
    {
        QuestManager.Instance.CompleteQuest(questID);
        Debug.Log($"✅ Completada: {questID}");
    }
    
    public void LogQuestStatus()
    {
        Debug.Log("=== <color=cyan>QUEST STATUS LOG</color> ===");
        
        var activeQuests = QuestManager.Instance.GetActiveQuests();
        var completedQuests = QuestManager.Instance.GetCompletedQuests();
        
        Debug.Log($"<color=yellow>ACTIVE ({activeQuests.Count}):</color>");
        foreach (var quest in activeQuests)
        {
            Debug.Log($"  ◉ {quest.questID}: {quest.questName}");
        }
        
        Debug.Log($"<color=green>COMPLETED ({completedQuests.Count}):</color>");
        foreach (var quest in completedQuests)
        {
            Debug.Log($"  ✓ {quest.questID}: {quest.questName}");
        }
        
        // Verificar quests específicas monitoreadas
        if (questsToMonitor.Count > 0)
        {
            Debug.Log("<color=magenta>MONITORED QUESTS:</color>");
            foreach (string id in questsToMonitor)
            {
                bool active = QuestManager.Instance.IsQuestActive(id);
                bool completed = QuestManager.Instance.IsQuestCompleted(id);
                Debug.Log($"  {id}: Active={active}, Completed={completed}");
            }
        }
    }
    
    // === GIZMOS PARA VISUALIZAR EN EDITOR ===
    
    void OnGUI()
    {
        if (!Application.isEditor || !showDebugUI) return;
        
        // Mini panel en la esquina si no hay UI configurada
        if (debugPanel == null)
        {
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(new Rect(10, 10, 250, 100), "");
            GUI.color = Color.white;
            
            GUI.Label(new Rect(15, 15, 240, 20), "QUEST DEBUG (Shift + ...)");
            GUI.Label(new Rect(15, 35, 240, 20), "R = Reset All Quests");
            GUI.Label(new Rect(15, 55, 240, 20), "C = Complete Active Quests");
            GUI.Label(new Rect(15, 75, 240, 20), "L = Log Quest Status");
        }
    }
}

// === CUSTOM EDITOR PARA FACILITAR EL DEBUG ===
#if UNITY_EDITOR
[CustomEditor(typeof(QuestDebugHelper))]
public class QuestDebugHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        QuestDebugHelper helper = (QuestDebugHelper)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("🔄 Reset All", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                helper.ResetAllQuests();
            }
            else
            {
                // Limpiar PlayerPrefs incluso sin estar en Play Mode
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("PlayerPrefs limpiados. Las quests se resetearán al iniciar.");
            }
        }
        
        if (GUILayout.Button("✅ Complete Active", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                helper.CompleteActiveQuests();
            }
            else
            {
                Debug.LogWarning("Debes estar en Play Mode para completar quests activas.");
            }
        }
        
        if (GUILayout.Button("📋 Log Status", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                helper.LogQuestStatus();
            }
            else
            {
                Debug.LogWarning("Debes estar en Play Mode para ver el estado de las quests.");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Tip: Activa 'Reset Quests On Start' para limpiar las quests automáticamente al iniciar el juego en el editor.", MessageType.Info);
        }
    }
}

// === MENU ITEM PARA LIMPIAR PLAYERPREFS ===
public static class QuestDebugMenu
{
    [MenuItem("Tools/Quest System/Clear All Quest Data")]
    public static void ClearAllQuestData()
    {
        if (EditorUtility.DisplayDialog("Clear Quest Data", 
            "Esto borrará TODOS los datos guardados de las quests.\n\n¿Estás seguro?", 
            "Sí, borrar todo", "Cancelar"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("✅ <color=green>Todos los datos de quests han sido borrados.</color>");
        }
    }
    
    [MenuItem("Tools/Quest System/Open PlayerPrefs Location")]
    public static void OpenPlayerPrefsLocation()
    {
        string path = "";
        
        #if UNITY_EDITOR_WIN
            path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + 
                   "/../LocalLow/" + Application.companyName + "/" + Application.productName;
        #elif UNITY_EDITOR_OSX
            path = "~/Library/Preferences";
        #endif
        
        if (!string.IsNullOrEmpty(path))
        {
            EditorUtility.RevealInFinder(path);
        }
    }
}
#endif