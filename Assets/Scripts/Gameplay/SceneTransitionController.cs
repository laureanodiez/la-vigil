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
        // desactivar input / movement si se le pasaron referencias
        foreach (var b in disableDuringTransition)
            if (b != null) b.enabled = false;

        // sonido opcional
        if (transitionSfx != null)
            transitionSfx.Play();

        // Fade out
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // start async load
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // espera que se cargue hasta 90%
        while (op.progress < 0.9f)
            yield return null;

        // activa la escena (finaliza la carga)
        op.allowSceneActivation = true;

        // opcional wait one frame
        yield return null;

        // Fade in (del nuevo contenido)
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        // reenables (si deseás volver a habilitar)
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
