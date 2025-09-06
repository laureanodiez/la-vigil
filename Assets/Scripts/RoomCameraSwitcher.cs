using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class SimpleTriggerCameraSwitcher : MonoBehaviour
{
    [Header("Asignar en el Trigger GameObject")]
    public CinemachineCamera targetCamera;   // la cámara que queremos activar al entrar
    public string playerTag = "Player";

    [Header("Opciones")]
    public bool disablePreviousCamera = true; // si true, desactiva la última cámara activa
    public float switchDebounce = 0.15f;     // tiempo mínimo entre switches (segundos)

    // estado estático (compartido entre todos los triggers)
    static CinemachineCamera currentActiveCamera = null;
    static float lastSwitchTime = -10f;

    void Reset()
    {
        // para que en inspector aparezca el collider como trigger por defecto
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;      // solo el player
        if (Time.time - lastSwitchTime < switchDebounce) return;
        lastSwitchTime = Time.time;

        if (targetCamera == null)
        {
            Debug.LogWarning($"[{name}] SimpleTriggerCameraSwitcher: 'targetCamera' no está asignada.");
            return;
        }

        // Si ya es la misma cámara, no hacemos nada
        if (currentActiveCamera == targetCamera) return;

        // Desactivar la anterior (si corresponde)
        if (disablePreviousCamera && currentActiveCamera != null)
        {
            try { currentActiveCamera.gameObject.SetActive(false); }
            catch { /* tolerante a errores runtime */ }
        }

        // Activar la nueva
        try
        {
            targetCamera.gameObject.SetActive(true);
            // Prioritizar para que Cinemachine la elija si la API lo permite
            // (Prioritize() es la forma que evita tocar Priority internals)
            targetCamera.Prioritize();
        }
        catch { /* tolerante a errores runtime */ }

        currentActiveCamera = targetCamera;
    }
}
