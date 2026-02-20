using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class QuimiWallShadowAnimator : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    public Transform playerTransform;
    public SpriteRenderer playerSpriteRenderer;

    [Header("Configuración del Cuadrado")]
    public LayerMask wallLayer;
    public Vector2 shadowDirection = new Vector2(-1f, 1f);
    public float maxDistance = 3f;

    [Header("Ajustes de Precisión")]
    [Tooltip("Ajusta desde dónde sale el rayo para que no empiece ADENTRO de la pared.")]
    public Vector2 offsetOrigenRayo = Vector2.zero;
    public Vector2 offsetVisualSombra = Vector2.zero;

    private SpriteRenderer mySpriteRenderer;

    void Awake()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

void Update()
    {
        mySpriteRenderer.sprite = playerSpriteRenderer.sprite;
        if (mySpriteRenderer.sprite == null) return;

        Vector2 origenReal = (Vector2)playerTransform.position + offsetOrigenRayo;
        Vector2 direccion = shadowDirection.normalized;

        RaycastHit2D hit = Physics2D.Raycast(origenReal, direccion, maxDistance, wallLayer);

        Vector2 puntoDestinoLogico;

        if (hit.collider != null)
        {
            // Choca: el vértice se achica a la pared
            puntoDestinoLogico = hit.point;
        }
        else
        {
            // No choca: el vértice va a su tamaño máximo
            puntoDestinoLogico = origenReal + (direccion * maxDistance);
        }

        transform.position = puntoDestinoLogico + offsetVisualSombra;
    }

    // --- MAGIA DE GIZMOS ---
    // Esto solo se ve en la pestaña "Scene", no en el juego final.
    void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        Vector2 origenReal = (Vector2)playerTransform.position + offsetOrigenRayo;
        Vector2 direccion = shadowDirection.normalized;

        // 1. Dibujamos el ORIGEN (Rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(origenReal, 0.05f);

        RaycastHit2D hit = Physics2D.Raycast(origenReal, direccion, maxDistance, wallLayer);

        if (hit.collider != null)
        {
            // 2. Si choca, dibujamos la línea y el punto de impacto (Verde)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origenReal, hit.point);
            Gizmos.DrawSphere(hit.point, 0.05f);

            // Dibujamos tu "Cuadrado" que se achica
            DibujarCuadrado(origenReal, hit.point, Color.green);
        }
        else
        {
            // 3. Si no choca, dibujamos la línea hasta el máximo (Amarillo)
            Vector2 destinoMax = origenReal + (direccion * maxDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origenReal, destinoMax);
            Gizmos.DrawSphere(destinoMax, 0.05f);

            // Dibujamos tu "Cuadrado" al tamaño máximo
            DibujarCuadrado(origenReal, destinoMax, Color.yellow);
        }
    }

    // Función auxiliar para dibujar el contorno del cuadrado en la Scene
    private void DibujarCuadrado(Vector2 origen, Vector2 destino, Color color)
    {
        Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);
        Vector2 esquinaArribaDer = new Vector2(destino.x, origen.y);
        Vector2 esquinaAbajoIzq = new Vector2(origen.x, destino.y);
        
        Gizmos.DrawLine(origen, esquinaArribaDer);
        Gizmos.DrawLine(esquinaArribaDer, destino);
        Gizmos.DrawLine(destino, esquinaAbajoIzq);
        Gizmos.DrawLine(esquinaAbajoIzq, origen);
    }
}