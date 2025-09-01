using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel;
    public GameObject mainButtonsPanel; // opcional, para ocultar botones cuando se abre options

    [Header("Camera")]
    public Camera camera;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip clickSfx;

    [Header("Audio Settings")]
    [Range(0f,1f)] public float masterVolume = 1f;
    public Slider volumeSlider;

    [Header("Resolution")]
    public Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    void Start()
    {
        // Volume init
        if (volumeSlider != null) {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Resolutions
        resolutions = Screen.resolutions;
        if (resolutionDropdown != null) {
            resolutionDropdown.ClearOptions();
            var options = resolutions.Select(r => r.width + " x " + r.height).Distinct().ToList();
            resolutionDropdown.AddOptions(options);
            // seleccionar la resolución actual
            int currentIndex = options.IndexOf(Screen.width + " x " + Screen.height);
            resolutionDropdown.value = currentIndex >= 0 ? currentIndex : 0;
            resolutionDropdown.onValueChanged.AddListener(SetResolutionByIndex);
        }

        // Ensure options panel hidden
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    // Called by Play Button (assign in OnClick)
    public void PlayGame(string sceneName)
    {
        if (uiAudioSource != null && clickSfx != null) uiAudioSource.PlayOneShot(clickSfx);
        camera.enabled = false;
        optionsPanel.SetActive(false);
        mainButtonsPanel.SetActive(false);
        SceneManager.LoadScene(sceneName);
    }

    // Called by Options Button
    public void OpenOptions()
    {
        if (uiAudioSource != null && clickSfx != null) uiAudioSource.PlayOneShot(clickSfx);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
    }

    // Called by Back Button in options
    public void CloseOptions()
    {
        if (uiAudioSource != null && clickSfx != null) uiAudioSource.PlayOneShot(clickSfx);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value; // simple global volume
        if (uiAudioSource != null && clickSfx != null) uiAudioSource.PlayOneShot(clickSfx);
    }

    public void SetResolutionByIndex(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        if (uiAudioSource != null && clickSfx != null) uiAudioSource.PlayOneShot(clickSfx);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
