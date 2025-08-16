using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic; 

public class PlayerFlashbangSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private GameObject escombros;
    [SerializeField] private GameObject escombros1;
    [SerializeField] private GameObject pared;

    [SerializeField] private Light2D spotLight;
    [SerializeField] private Light2D quimiVision;

    //[SerializeField] private CanvasGroup flashEffect;
    [SerializeField] private Transform resetPoint;
    [SerializeField] private GameObject mapa1;
    [SerializeField] private GameObject mapa1_final;

    [Header("Configuración")]
    [SerializeField] private float totalDuration = 30f;
    [SerializeField] private float minOuterRadius = 0.5f;

    [Header("Configuración Flash")]
    [SerializeField] private float flashIntensity = 30f;
    [SerializeField] private float flashDuration = 0.5f;
    
    [Header("Posiciones")]
    [SerializeField] private Transform initialSpawnPoint;
    [SerializeField] private Transform explosionResetPoint;
    [SerializeField] private Transform finalSpawnPoint;

    private float initialGlobalIntensity;
    private float initialOuterRadius;
    private float currentTime;
    private bool isActive;
    private Vector3 gameplayStartPosition;

    private void Start()
    {
        initialOuterRadius = spotLight.pointLightOuterRadius;
        
        gameplayStartPosition = initialSpawnPoint != null ? 
            initialSpawnPoint.position : 
            transform.position;
        TeleportPlayer(gameplayStartPosition);
        
        if(globalLight != null)
            initialGlobalIntensity = globalLight.intensity;
        ResetSystem();
    }

    public void StartFlashbangSequence()
    {
        StartCoroutine(FlashSequence());
    }

    private void Update()
    {
        if (!isActive) return;

        currentTime -= Time.deltaTime;
        quimiVision.enabled = false; 
        escombros.SetActive(true);



        //Reducir progresivamente el radio de la luz
        float progress = currentTime / totalDuration;
        spotLight.pointLightOuterRadius = Mathf.Lerp(
            minOuterRadius, 
            initialOuterRadius, 
            progress
        );

        //Finalizar la secuencia cuando el tiempo se acaba
        if (currentTime <= 0f)
        {
            ResetSystem();//EndFlashSequence();
        }
    }
    
    private IEnumerator FlashSequence()
    {
        // 1. Flash inicial (aumento brusco de intensidad)
        yield return StartCoroutine(FlashEffect(true));
        
        // 2. Cambio de luces
        if (globalLight != null) 
        {
            globalLight.enabled = false;
            globalLight.intensity = initialGlobalIntensity; // Restaurar intensidad
        }
        
        if (spotLight != null) spotLight.gameObject.SetActive(true);
        
        // 3. Flash final (disminución)
        yield return StartCoroutine(FlashEffect(false));
        
        // 4. Iniciar temporizador
        currentTime = totalDuration;
        isActive = true;
        
        // Guardar posición actual como punto de respawn
        if (explosionResetPoint != null)
        {
            gameplayStartPosition = explosionResetPoint.position;
        }
    }

    private IEnumerator FlashEffect(bool isEnteringFlash)
    {
        if (globalLight == null) yield break;
        
        float startIntensity = isEnteringFlash ? initialGlobalIntensity : flashIntensity;
        float endIntensity = isEnteringFlash ? flashIntensity : initialGlobalIntensity;
        
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            globalLight.intensity = Mathf.Lerp(
                startIntensity, 
                endIntensity, 
                elapsed / flashDuration
            );
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        globalLight.intensity = endIntensity;
    }

    private IEnumerator BadEndFlashSequence()
    {
        isActive = false;
        
        // 1. Flash final (aumento)
        yield return StartCoroutine(FlashEffect(true));
        
        // 2. Reset del sistema
        ResetSystem();
        
        // 3. Flash final (disminución)
        yield return StartCoroutine(FlashEffect(false));
    }

    public void GoodEndFlashSequence()
    {
        isActive = false;
        gameplayStartPosition = finalSpawnPoint.position;
        TeleportPlayer(gameplayStartPosition);
                if (globalLight != null)
        {
            globalLight.enabled = true;
            globalLight.intensity = initialGlobalIntensity;
        }

        if (spotLight != null)
        {
            spotLight.gameObject.SetActive(false);
            spotLight.pointLightOuterRadius = initialOuterRadius;
        }
        escombros1.SetActive(true);
        mapa1.SetActive(false);
        mapa1_final.SetActive(true);
    }

    private void ResetSystem()
    {
        // 1. Teletransportar al punto de respawn actual
        TeleportPlayer(gameplayStartPosition);

        // 2. Resetear luces
        if (globalLight != null)
        {
            globalLight.enabled = true;
            globalLight.intensity = initialGlobalIntensity;
        }

        if (spotLight != null)
        {
            spotLight.gameObject.SetActive(false);
            spotLight.pointLightOuterRadius = initialOuterRadius;
        }

        // 3. Reactivar trigger
        var trigger = FindObjectOfType<TriggerFlashbang>();
        if (trigger != null)
        {
            trigger.GetComponent<Collider2D>().enabled = true;
        }

        isActive = false;
    }

    private void TeleportPlayer(Vector3 position)
    {
        // Método robusto para teletransporte
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = position;
        }
        else
        {
            transform.position = position;
        }        
    }
}