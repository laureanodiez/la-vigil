using UnityEngine;
using System.Collections;

public class ControlPantallaInicio : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Arrastra aquí la IMAGEN del botón continuar")]
    [SerializeField] private GameObject mensajeContinuar;

    [Header("Configuración")]
    [SerializeField] private float tiempoDeEspera = 3.0f;

    private bool permiteContinuar = false;

    // Usamos OnEnable para que pause cada vez que actives este Canvas
    void OnEnable()
    {
        // 1. Ocultar el botón al inicio
        if (mensajeContinuar != null)
        {
            mensajeContinuar.SetActive(false);
        }

        permiteContinuar = false;

        // 2. PAUSAR EL JUEGO (Movimiento y Física)
        Time.timeScale = 0f;

        // 3. PAUSAR EL SONIDO GLOBALMENTE
        AudioListener.pause = true;

        // Iniciamos la cuenta regresiva
        StartCoroutine(MostrarMensajeRutina());
    }

    void Update()
    {
        if (permiteContinuar)
        {
            // Detectar Espacio (o clic si prefieres)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReanudarJuego();
            }
        }
    }

    IEnumerator MostrarMensajeRutina()
    {
        // IMPORTANTE: Usamos Realtime porque el Time.timeScale está en 0.
        // Si usáramos WaitForSeconds normal, esto se congelaría eternamente.
        yield return new WaitForSecondsRealtime(tiempoDeEspera);

        if (mensajeContinuar != null)
        {
            mensajeContinuar.SetActive(true);
        }

        permiteContinuar = true;
    }

    void ReanudarJuego()
    {
        // 1. Restaurar el tiempo normal
        Time.timeScale = 1f;

        // 2. Restaurar el sonido
        AudioListener.pause = false;

        // 3. Ocultar el Canvas
        gameObject.SetActive(false);
    }
}