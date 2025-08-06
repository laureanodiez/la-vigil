using UnityEngine;

public class GlobalVisionController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float baseVisionRadius = 5f;
    [SerializeField] private float visionTransitionSpeed = 3f;

    [Header("Referencias")]
    [SerializeField] private Material globalVisionMaterial;
    [SerializeField] private Transform player;

    private SpriteRenderer visionRenderer;
    private float currentVisionRadius;
    private static readonly int PlayerPosition = Shader.PropertyToID("_PlayerPos");
    private static readonly int VisionRadius = Shader.PropertyToID("_VisionRadius");

    private void Start()
    {
        visionRenderer = GetComponent<SpriteRenderer>();

        // Escalar para cubrir toda la pantalla
        ResizeToScreen();

        currentVisionRadius = baseVisionRadius;
    }

    private void Update()
    {
        UpdateShaderParameters();
        UpdateVisionRadius();
    }

    private void ResizeToScreen()
    {
        // Calcular tamaño para cubrir toda la cámara
        float height = Camera.main.orthographicSize * 2;
        float width = height * Camera.main.aspect;

        transform.localScale = new Vector3(width, height, 1);
    }

    private void UpdateShaderParameters()
    {
        if (player == null || globalVisionMaterial == null) return;

        // Actualizar posición del jugador en el shader
        globalVisionMaterial.SetVector(PlayerPosition, player.position);
        globalVisionMaterial.SetFloat(VisionRadius, currentVisionRadius);
    }

    private void UpdateVisionRadius()
    {
        // Suavizar transiciones de radio
        currentVisionRadius = Mathf.Lerp(
            currentVisionRadius,
            baseVisionRadius,
            visionTransitionSpeed * Time.deltaTime
        );
    }

    // Llamar esta función para cambiar dinámicamente el radio
    public void SetVisionRadius(float radius, bool instant = false)
    {
        baseVisionRadius = radius;
        if (instant) currentVisionRadius = radius;
    }

    // Para cuando agregues nuevos niveles
    public void UpdatePlayerReference(Transform newPlayer)
    {
        player = newPlayer;
    }
    
    private void OnEnable()
    {
        // Forzar actualización al activarse
        if(Camera.main != null)
        {
            Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
            Camera.main.transparencySortAxis = Vector3.forward;
        }
    }
}