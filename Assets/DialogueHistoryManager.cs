using UnityEngine;
using TMPro;
using UnityEngine.UI; // Necesario para el ScrollRect
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class DialogueHistoryManager : MonoBehaviour
{
    public static DialogueHistoryManager Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private GameObject historyPanel;
    [SerializeField] private TMP_Text historyText;
    [SerializeField] private ScrollRect scrollRect; // Para hacer auto-scroll hacia abajo
    [SerializeField] private GameObject hudButton;

    [Header("Referencias del Jugador")]
    [SerializeField] private QuimiSpriteAnimator playerMovement; // Para frenar a Quime

    [Header("Controles")]
    [SerializeField] private KeyCode toggleKey = KeyCode.H;
    private bool canToggleHistory = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        if (historyPanel != null) historyPanel.SetActive(false);
        if (historyText != null) historyText.text = ""; 
    }

    private void Update()
    {
        // Ahora solo funciona si canToggleHistory es true
        if (canToggleHistory && Input.GetKeyDown(toggleKey)) 
        {
            ToggleHistory();
        }
    }

    private HashSet<string> registeredLines = new HashSet<string>();
    public void AddToHistory(string uniqueID, string speaker, string dialogue)
    {
        if (historyText == null) return;
        
        if (!registeredLines.Add(uniqueID)) 
        {
            return; 
        }
        
        historyText.text += $"<b>{speaker}:</b> {dialogue}\n\n";
        
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null && scrollRect.content != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        }
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    // Este método lo podés llamar desde el botón de la UI
    public void ToggleHistory()
    {
        if (historyPanel == null) return;

        bool isOpening = !historyPanel.activeSelf;
        historyPanel.SetActive(isOpening);

        // Frenar o liberar a Quime según corresponda
        if (playerMovement != null)
        {
            playerMovement.SetInputActive(!isOpening);
        }

        // Si estamos abriendo el historial, forzamos la actualización matemática
        if (isOpening)
        {
            // 1. Obliga a TextMeshPro a procesar las letras y renglones nuevos
            Canvas.ForceUpdateCanvases(); 
            
            // 2. LA MAGIA: Obliga al Content a recalcular su alto basándose en el texto, AHORA MISMO
            if (scrollRect != null && scrollRect.content != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            }

            // 3. Ahora que el Content tiene su alto real, tiramos el scroll bien al fondo
            if (scrollRect != null) 
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }


    public void SetHUDVisible(bool isVisible)
    {
        canToggleHistory = isVisible; // Bloquea/Desbloquea la tecla H
        
        if (hudButton != null) hudButton.SetActive(isVisible);

        if (!isVisible && historyPanel != null && historyPanel.activeSelf)
        {
            ToggleHistory(); 
        }
    }
}

