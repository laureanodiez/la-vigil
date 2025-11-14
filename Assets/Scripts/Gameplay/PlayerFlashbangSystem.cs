using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

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
    [SerializeField] private GameObject pared_sombra;
    [SerializeField] private GameObject pared_sombra1;
    [SerializeField] private QuimiSpriteAnimator quimiController;
    [SerializeField] private AudioSource cancionPasado;
    [SerializeField] private AudioSource breakbeat;
    [SerializeField] private AudioSource respiracion;
    [SerializeField] private AudioSource explosion;
    [SerializeField] private AudioSource apagon;


    [Header("Configuración")]
    [SerializeField] private float totalDuration = 30f;
    [SerializeField] private float minOuterRadius = 0.5f;

    [Header("Configuración Flash")]
    [SerializeField] private float flashIntensity = 300f;
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
        quimiController.enabled = true;



        //Reducir progresivamente el radio de la luz
        float progress = currentTime / totalDuration;
        spotLight.pointLightOuterRadius = Mathf.Lerp(
            minOuterRadius,
            initialOuterRadius,
            progress
        );
        respiracion.volume = Mathf.Lerp(1f, 0f, progress);


        //Finalizar la secuencia cuando el tiempo se acaba
        if (currentTime <= 0f)
        {
            ResetSystem();//EndFlashSequence();
        }
    }
    
    private IEnumerator FlashSequence()
    {
        quimiController.enabled = false;
        // 1. Flash inicial (aumento brusco de intensidad)
        yield return StartCoroutine(FlashEffect(true));
        cancionPasado.Stop();
        explosion.Play();
        breakbeat.Play();
        respiracion.Play();
        
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
        StartCoroutine(GoodEndFlashSequenceCoroutine());
    }

    private IEnumerator GoodEndFlashSequenceCoroutine()
    {
        // 1) detener el timer/gameplay
        isActive = false;

        if (quimiController != null) quimiController.enabled = false;

        if (globalLight != null) globalLight.enabled = true;

        // (opcional) guardar color original por si lo modificásemos (normalmente es blanco)
        Color originalColor = globalLight != null ? globalLight.color : Color.white;
        if (globalLight != null) globalLight.color = Color.white;

        // 4) FADE-IN (flash a blanco / aumento de intensidad)
        yield return StartCoroutine(FlashEffect(true));

        // 5) Teleport mientras la pantalla/escena sigue 'blanca'
        if (finalSpawnPoint != null)
        {
            gameplayStartPosition = finalSpawnPoint.position;
        }
        TeleportPlayer(gameplayStartPosition);

        // 6) aplicar cambios de mapa/estado mientras el jugador no ve (pantalla blanca)
        if (spotLight != null)
        {
            spotLight.gameObject.SetActive(false);
            spotLight.pointLightOuterRadius = initialOuterRadius;
        }
        if (globalLight != null)
        {
            globalLight.enabled = true;
            globalLight.intensity = initialGlobalIntensity; // lo normalizamos para el fade-out
        }
        escombros1.SetActive(true);
        quimiVision.enabled = true;
        mapa1.SetActive(false);
        mapa1_final.SetActive(true);
        pared_sombra.SetActive(false);
        pared_sombra1.SetActive(false);
        breakbeat.Stop();
        respiracion.Stop();
        explosion.Play();
        cancionPasado.Play();

        // Pequeña espera para asegurar que Unity haya aplicado transform/objetos (un frame)
        yield return null;

        // 7) FADE-OUT (volver a la normalidad)
        yield return StartCoroutine(FlashEffect(false));

        // 8) restaurar color si lo tocamos
        if (globalLight != null) globalLight.color = originalColor;

        // 9) devolver control al jugador
        if (quimiController != null) quimiController.enabled = true;
    }

    private void ResetSystem()
    {
        breakbeat.Stop();
        respiracion.Stop();
        apagon.Play();
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