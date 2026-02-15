using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [Header("Configuración de Animación")]
    [Tooltip("Arrastra aquí tus sprites en orden.")]
    [SerializeField] private Sprite[] frames;

    [Tooltip("Tiempo en segundos que dura cada frame.")]
    [SerializeField] private float velocidad = 0.1f;

    [Tooltip("Si es TRUE, la animación se repite infinitamente. Si es FALSE, se detiene en el último frame.")]
    [SerializeField] private bool repetir = true;

    [Tooltip("Si está activo, elige un frame al azar (ignora la opción de repetir).")]
    [SerializeField] private bool randomizar = false;

    [Header("Estado (Solo lectura)")]
    [SerializeField] private bool estaReproduciendo = true;

    private SpriteRenderer _spriteRenderer;
    private float _timer;
    private int _currentIndex;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"Faltan frames en: {gameObject.name}");
            enabled = false;
        }
    }

    void Update()
    {
        // Si no debe reproducirse, no hacemos nada
        if (!estaReproduciendo) return;

        _timer += Time.deltaTime;

        if (_timer >= velocidad)
        {
            _timer = 0f;
            CambiarFrame();
        }
    }

    void CambiarFrame()
    {
        if (randomizar)
        {
            _currentIndex = Random.Range(0, frames.Length);
            _spriteRenderer.sprite = frames[_currentIndex];
        }
        else
        {
            _currentIndex++;

            // Lógica de fin de animación
            if (_currentIndex >= frames.Length)
            {
                if (repetir)
                {
                    _currentIndex = 0; // Volver al inicio
                }
                else
                {
                    _currentIndex = frames.Length - 1; // Se queda en el último frame
                    estaReproduciendo = false; // Detenemos la lógica
                    return; 
                }
            }

            _spriteRenderer.sprite = frames[_currentIndex];
        }
    }

    // Método público útil para activar la animación desde otro script o evento
    public void Reproducir()
    {
        _currentIndex = 0;
        _timer = 0f;
        estaReproduciendo = true;
        enabled = true;
        
        // Asignamos el primer frame inmediatamente
        if (frames.Length > 0) 
            _spriteRenderer.sprite = frames[0];
    }
}