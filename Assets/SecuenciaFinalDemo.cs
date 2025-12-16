using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SecuenciaFinalDemo : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [SerializeField] private string nombreEscenaMenu = "Frente";

    [Header("Referencias de UI")]
    [SerializeField] private GameObject panelFondoNegro;
    [SerializeField] private GameObject objetoTitulo;
    [SerializeField] private GameObject objetoFecha;

    [Header("Audio Secuencial")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Suena EN EL INSTANTE que la pantalla se pone negra")]
    [SerializeField] private AudioClip sonidoPantallaNegra;
    
    [Tooltip("Suena cuando aparece el Título")]
    [SerializeField] private AudioClip sonidoTitulo;
    
    [Tooltip("Suena cuando aparece el 2026")]
    [SerializeField] private AudioClip sonidoFecha;

    [Header("Tiempos (Segundos)")]
    [SerializeField] private float esperaInicialNegro = 1.0f; 
    [SerializeField] private float esperaHastaFecha = 2.0f;   
    [SerializeField] private float esperaFinal = 4.0f;        

    void OnEnable()
    {
        // 1. Estado inicial
        if (panelFondoNegro != null) panelFondoNegro.SetActive(true);
        if (objetoTitulo != null) objetoTitulo.SetActive(false);
        if (objetoFecha != null) objetoFecha.SetActive(false);

        // 2. Reproducir sonido inicial (Pantalla Negra)
        ReproducirSonido(sonidoPantallaNegra);

        // 3. Iniciar secuencia
        StartCoroutine(RutinaFinal());
    }

    IEnumerator RutinaFinal()
    {
        // FASE 1: Solo pantalla negra y su sonido (ya reproducido en OnEnable)
        yield return new WaitForSecondsRealtime(esperaInicialNegro);

        // FASE 2: Aparece Título + Sonido Fuerte
        if (objetoTitulo != null) objetoTitulo.SetActive(true);
        ReproducirSonido(sonidoTitulo);
        
        yield return new WaitForSecondsRealtime(esperaHastaFecha);

        // FASE 3: Aparece Fecha + Sonido Fecha
        if (objetoFecha != null) objetoFecha.SetActive(true);
        ReproducirSonido(sonidoFecha);

        // FASE 4: Lectura final
        yield return new WaitForSecondsRealtime(esperaFinal);

        // Cargar Menú
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // Stop() opcional: Si quieres que el sonido anterior se corte drásticamente
            // audioSource.Stop(); 
            audioSource.PlayOneShot(clip);
        }
    }
}