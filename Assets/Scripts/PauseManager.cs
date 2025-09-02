using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Prefab del menú de pausa (Canvas)")]
    [SerializeField] private GameObject pauseMenuPrefab; // assign in inspector (prefab)
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // nombre exacto

    private GameObject pauseMenuInstance;
    private bool isPaused = false;
    public bool IsPaused => isPaused;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si la escena es MainMenu, nos aseguramos de ocultar/desactivar el menú
        if (scene.name == mainMenuSceneName)
        {
            if (pauseMenuInstance != null) pauseMenuInstance.SetActive(false);
            if (isPaused) Resume(); // dejar sin pausar
        }
    }

    void Update()
    {
        // Escape toggles pause, pero no en MainMenu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var active = SceneManager.GetActiveScene();
            if (active.name == mainMenuSceneName) return;

            if (!isPaused) Pause();
            else Resume();
        }
    }

    private void EnsureMenuInstantiated()
    {
        if (pauseMenuInstance != null) return;
        if (pauseMenuPrefab == null)
        {
            Debug.LogError("PauseManager: falta asignar pauseMenuPrefab en el inspector.");
            return;
        }
        pauseMenuInstance = Instantiate(pauseMenuPrefab);
        pauseMenuInstance.SetActive(false);
        DontDestroyOnLoad(pauseMenuInstance); // mantener la UI entre escenas
    }

    public void Pause()
    {
        EnsureMenuInstantiated();
        if (pauseMenuInstance == null) return;

        // mostrar UI
        pauseMenuInstance.SetActive(true);

        // detener el tiempo del juego (cuidado con coroutines que dependan de timeScale)
        Time.timeScale = 0f;

        // pausar audio opcional
        AudioListener.pause = true;

        isPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuInstance != null)
            pauseMenuInstance.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
    }

    // llamado desde botones del UI
    public void OnResumeButton() => Resume();

    public void OnQuitToMainMenuButton()
    {
        // Aseguramos salir del pause antes de cargar
        Resume();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnOptionsButton()
    {
        // Placeholder: abrir panel opciones dentro del prefab o llamar sistema de options
        Debug.Log("Options - abrir panel (implementar)");
    }
}
