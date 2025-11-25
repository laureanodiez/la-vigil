using UnityEngine;
using Unity.Cinemachine;

public class AreaCameraSwitcher : MonoBehaviour
{
    [Header("Asignar")]
    public CinemachineCamera targetCamera;
    public string playerTag = "Player";
    
    [Header("Opciones")]
    public bool disablePreviousCamera = true;
    [SerializeField] private bool debugMode = false;
    
    // Estado estático compartido
    static AreaCameraSwitcher currentArea = null;  // Área donde está completamente el jugador
    static AreaCameraSwitcher nextArea = null;     // Área donde se asoma el jugador
    static CinemachineCamera activeCamera = null;
    
    // Estado local
    private Collider2D areaCollider;
    private bool playerInside = false;

    void Awake()
    {
        areaCollider = GetComponent<Collider2D>();
        if (areaCollider == null)
        {
            Debug.LogError($"[{name}] AreaCameraSwitcher requiere un Collider2D.");
            enabled = false;
            return;
        }
        
        if (!areaCollider.isTrigger)
        {
            areaCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        
        playerInside = true;
        
        if (debugMode)
            Debug.Log($"[{name}] Jugador ENTRÓ al trigger");
        
        // Caso 1: No hay área actual (inicio del juego o primer área)
        if (currentArea == null)
        {
            currentArea = this;
            ActivateCamera();
            if (debugMode)
                Debug.Log($"[{name}] Primera área, activando como CURRENT");
            return;
        }
        
        // Caso 2: El jugador está en currentArea y se asoma a una nueva área
        if (currentArea != this)
        {
            // Esta es una nueva área (nextArea)
            nextArea = this;
            ActivateCamera();
            if (debugMode)
                Debug.Log($"[{name}] Jugador se ASOMÓ, activando como NEXT (desde {currentArea.name})");
        }
        // Caso 3: El jugador vuelve al área que era nextArea (re-entra)
        else if (currentArea == this)
        {
            // El jugador volvió al área actual, mantener
            if (debugMode)
                Debug.Log($"[{name}] Jugador volvió al área CURRENT");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        
        playerInside = false;
        
        if (debugMode)
            Debug.Log($"[{name}] Jugador SALIÓ del trigger");
        
        // Caso 1: El jugador sale de nextArea (deja de asomarse)
        if (nextArea == this)
        {
            if (debugMode)
                Debug.Log($"[{name}] Jugador dejó NEXT, volviendo a cámara de CURRENT: {currentArea.name}");
            
            // Volver a la cámara del área current
            nextArea = null;
            if (currentArea != null)
            {
                currentArea.ActivateCamera();
            }
        }
        // Caso 2: El jugador sale completamente de currentArea
        else if (currentArea == this)
        {
            if (debugMode)
                Debug.Log($"[{name}] Jugador salió COMPLETAMENTE de CURRENT");
            
            // Si hay un nextArea, promoverlo a currentArea
            if (nextArea != null)
            {
                if (debugMode)
                    Debug.Log($"[{name}] Promoviendo NEXT ({nextArea.name}) a CURRENT");
                
                currentArea = nextArea;
                nextArea = null;
                // La cámara ya está activa, solo actualizamos referencias
            }
            else
            {
                // No hay nextArea, el jugador salió a ninguna parte
                currentArea = null;
                if (debugMode)
                    Debug.LogWarning($"[{name}] Jugador salió sin estar en otra área");
            }
        }
    }

    void ActivateCamera()
    {
        if (targetCamera == null)
        {
            Debug.LogWarning($"[{name}] targetCamera no asignada.");
            return;
        }

        // Si ya es la cámara activa, no hacer nada
        if (activeCamera == targetCamera)
        {
            if (debugMode)
                Debug.Log($"[{name}] Cámara ya está activa, no cambiando");
            return;
        }

        if (debugMode)
            Debug.Log($"[{name}] ACTIVANDO cámara: {targetCamera.name}");

        // Desactivar la cámara anterior
        if (disablePreviousCamera && activeCamera != null && activeCamera != targetCamera)
        {
            activeCamera.gameObject.SetActive(false);
            if (debugMode)
                Debug.Log($"[{name}] Desactivando cámara anterior: {activeCamera.name}");
        }

        // Activar la nueva cámara
        targetCamera.gameObject.SetActive(true);
        targetCamera.Prioritize();

        activeCamera = targetCamera;
    }

    // Métodos públicos para debugging
    public static AreaCameraSwitcher GetCurrentArea() => currentArea;
    public static AreaCameraSwitcher GetNextArea() => nextArea;
    public static CinemachineCamera GetActiveCamera() => activeCamera;

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        // Color según estado
        Color gizmoColor;
        if (currentArea == this)
            gizmoColor = new Color(0, 1, 0, 0.4f); // Verde brillante = CURRENT
        else if (nextArea == this)
            gizmoColor = new Color(1, 1, 0, 0.4f); // Amarillo = NEXT (asomándose)
        else if (playerInside)
            gizmoColor = new Color(1, 0.5f, 0, 0.3f); // Naranja = jugador dentro pero sin estado
        else
            gizmoColor = new Color(0, 0.5f, 1, 0.2f); // Azul claro = inactiva

        Gizmos.color = gizmoColor;

        if (col is BoxCollider2D box)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.DrawCube(box.offset, box.size);
            
            // Dibujar borde
            Gizmos.color = currentArea == this ? Color.green : 
                          nextArea == this ? Color.yellow : 
                          new Color(0, 0.5f, 1, 0.8f);
            Gizmos.DrawWireCube(box.offset, box.size);
            
            Gizmos.matrix = oldMatrix;
        }
        else if (col is PolygonCollider2D poly)
        {
            for (int i = 0; i < poly.points.Length; i++)
            {
                Vector2 start = transform.TransformPoint(poly.points[i]);
                Vector2 end = transform.TransformPoint(poly.points[(i + 1) % poly.points.Length]);
                Gizmos.DrawLine(start, end);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (targetCamera != null)
        {
            // Etiqueta con información
            string status = currentArea == this ? "CURRENT" : 
                           nextArea == this ? "NEXT" : 
                           playerInside ? "INSIDE" : "IDLE";
            
            UnityEditor.Handles.color = currentArea == this ? Color.green : 
                                       nextArea == this ? Color.yellow : Color.white;
            
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2, 
                $"{name}\n[{status}]\nCámara: {targetCamera.name}"
            );
        }
    }
    #endif
}