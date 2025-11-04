using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel;
    public GameObject mainButtonsPanel; // opcional, para ocultar botones cuando se abre options

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip clickSfx;

    [Header("Audio Settings")]
    [Range(0f,1f)] public float masterVolume = 1f;
    public Slider volumeSlider;

    [Header("Resolution")]
    public Dropdown resolutionDropdown;

    private Resolution[] resolutions;


    [Header("Navigation")]
    [SerializeField] private Button firstSelectedButton; // El botón "Play"
    [SerializeField] private Button firstSelectedInOptions; // El primer botón del panel de opciones
    [SerializeField] private bool useKeyboardNavigation = true;

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
            int currentIndex = options.IndexOf(Screen.width + " x " + Screen.height);
            resolutionDropdown.value = currentIndex >= 0 ? currentIndex : 0;
            resolutionDropdown.onValueChanged.AddListener(SetResolutionByIndex);
        }

        // Ensure options panel hidden
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        // 🎮 NUEVO: Seleccionar el primer botón automáticamente
        if (useKeyboardNavigation && firstSelectedButton != null)
        {
            SelectButton(firstSelectedButton);
        }
    }

    // Called by Play Button (assign in OnClick)
    public void PlayGame(string sceneName)
    {
        if (uiAudioSource != null && clickSfx != null) uiAudioSource.PlayOneShot(clickSfx);
        // carga por nombre (asegúrate de agregar la escena a Build Settings)
        SceneManager.LoadScene(sceneName);
    }

    // Called by Options Button
    public void OpenOptions()
    {
        if (uiAudioSource != null && clickSfx != null) uiAudioSource.PlayOneShot(clickSfx);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        
        // 🎮 NUEVO: Seleccionar el primer botón de opciones
        if (useKeyboardNavigation && firstSelectedInOptions != null)
        {
            SelectButton(firstSelectedInOptions);
        }
    }


    // Called by Back Button in options
    // Seleccionar un botón específico
    void SelectButton(Button button)
    {
        if (button == null) return;
        
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);
        
    }

    // Método público para seleccionar desde otros lugares
    public void SelectFirstButton()
    {
        SelectButton(firstSelectedButton);
    }
    public void CloseOptions()
    {
        if (uiAudioSource != null && clickSfx != null) uiAudioSource.PlayOneShot(clickSfx);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        
        // 🎮 NUEVO: Volver a seleccionar el botón principal
        if (useKeyboardNavigation && firstSelectedButton != null)
        {
            SelectButton(firstSelectedButton);
        }
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
    void Update()
    {
        // Cerrar opciones con ESC
        if (optionsPanel != null && optionsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseOptions();
        }
        
        // 🎮 DETECCIÓN DE TECLADO: Reseleccionar botón si usas flechas/WASD
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            // Si no hay nada seleccionado, seleccionar el primer botón
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
            {
                if (optionsPanel != null && optionsPanel.activeSelf)
                    SelectButton(firstSelectedInOptions);
                else
                    SelectButton(firstSelectedButton);
            }
        }
        
        // 🖱️ DETECCIÓN DE MOUSE: Deseleccionar cuando el mouse se mueve
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            // Deseleccionar cualquier botón activo
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

}

