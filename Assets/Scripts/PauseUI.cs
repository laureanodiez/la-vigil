using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptions);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    private void OnResume()
    {
        PauseManager.Instance?.OnResumeButton();
    }

    private void OnOptions()
    {
        PauseManager.Instance?.OnOptionsButton();
    }

    private void OnQuit()
    {
        PauseManager.Instance?.OnQuitToMainMenuButton();
    }
}
