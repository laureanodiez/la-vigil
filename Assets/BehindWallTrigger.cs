using UnityEngine;
using System.Collections;

public class BehindWallTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer wallSprite; 

    [Header("Configuración")]
    [Range(0f, 1f)]
    [SerializeField] private float transparentAlpha = 0.3f; 
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartFade(transparentAlpha);

            // --- NUEVO: Buscamos el Aura en Quime y la activamos ---
            // Importante: El objeto hijo en Quime DEBE llamarse exactamente "AuraTransparente"
            Transform aura = other.transform.Find("Mask");
            if (aura != null)
            {
                aura.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartFade(1.0f);

            // --- NUEVO: Buscamos el Aura en Quime y la desactivamos ---
            Transform aura = other.transform.Find("Mask");
            if (aura != null)
            {
                aura.gameObject.SetActive(false);
            }
        }
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        Color currentColor = wallSprite.color;
        float startAlpha = currentColor.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            wallSprite.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        wallSprite.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}