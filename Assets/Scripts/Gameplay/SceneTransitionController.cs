using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance { get; private set; }

    [Header("Fade")]
    public Image fadeImage;                // UI Image (negra) que cubre toda la pantalla
    public float fadeDuration = 0.6f;

    [Header("Controlador de Quime")]
    [SerializeField] private QuimiSpriteAnimator quimiController;

    [Header("Opcional")]
    public AudioSource transitionSfx;      // sonido breve de transición opcional
    public Behaviour[] disableDuringTransition; // componentes a desactivar durante la transición (ej: PlayerController)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (fadeImage != null)
            {
                // asegurarnos que arranquemos transparentes
                var c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    IEnumerator TransitionCoroutine(string sceneName)
    {
        // 1. Desactivar input/movimiento de Quime
        foreach (var b in disableDuringTransition)
            if (b != null) b.enabled = false;

        if (transitionSfx != null)
            transitionSfx.Play();

        // 2. Fade Out a pantalla negra
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // 3. Iniciar carga asíncrona
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // 4. Activar la nueva escena
        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);

        // Esperar un frame extra para estabilizar el motor
        yield return null;

        // 5. Fade In (mostrar la nueva escena)
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        // 6. ¡AHORA SÍ! Reactivar input/movimiento de Quime
        foreach (var b in disableDuringTransition)
            if (b != null) b.enabled = true;
    }
    IEnumerator Fade(float from, float to, float dur)
    {
        if (fadeImage == null) yield break;
        float t = 0f;
        Color c = fadeImage.color;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / dur);
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }
        c.a = to;
        fadeImage.color = c;
    }
}
