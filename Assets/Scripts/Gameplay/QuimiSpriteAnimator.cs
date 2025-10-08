using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class QuimiSpriteAnimator : MonoBehaviour
{
    [Header("Sprites Idle / Moving")]
    public Sprite frontIdle, frontMove;
    public Sprite backIdle,  backMove;
    public Sprite leftIdle,  leftMove;
    public Sprite rightIdle, rightMove;

    [Header("Settings")]
    public float switchInterval = 0.3f;  // Tiempo entre frames MOV
    public float speed = 3f;

    [Header("Step Sound")]
    [SerializeField] public AudioSource steps;


    // Componentes
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    // Estado
    private Vector2 move;
    private float timer;
    private bool toggleFrame;

    // Última orientación conocida para mostrar el idle correcto cuando está quieto
    private enum Direction { Front, Back, Left, Right }
    private Direction lastDirection = Direction.Front;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        steps.loop = true;
        
    }

    void Update() {
        // 1) Movimiento
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");

        // 2) Control de Sprite
        if (move.sqrMagnitude > 0) {
            if (steps != null && !steps.isPlaying) {
                steps.Play();
            }

            // A) Elegir orientación principal
            Sprite idle, mov;
            if (Mathf.Abs(move.x) > Mathf.Abs(move.y)) {
                // horizontal
                if (move.x > 0) { idle = rightIdle; mov = rightMove; lastDirection = Direction.Right; }
                else          { idle = leftIdle;  mov = leftMove;  lastDirection = Direction.Left;  }
            } else {
                // vertical
                if (move.y > 0) { idle = backIdle;  mov = backMove;  lastDirection = Direction.Back; }
                else            { idle = frontIdle; mov = frontMove; lastDirection = Direction.Front; }
            }

            // B) Alternar frames MOV
            timer += Time.deltaTime;
            if (timer >= switchInterval) {
                timer = 0f;
                toggleFrame = !toggleFrame;
            }
            sr.sprite = toggleFrame ? idle : mov;
        }
        else {
            // Quieto → siempre idle de la última orientación:
            timer = 0f;
            toggleFrame = false;
            if (steps != null && steps.isPlaying) {
                steps.Stop();
            }

            // Mostrar el idle correspondiente a la última orientación guardada
            switch (lastDirection)
            {
                case Direction.Right:
                    sr.sprite = rightIdle;
                    break;
                case Direction.Left:
                    sr.sprite = leftIdle;
                    break;
                case Direction.Back:
                    sr.sprite = backIdle;
                    break;
                case Direction.Front:
                default:
                    sr.sprite = frontIdle;
                    break;
            }
        }
    }

    void FixedUpdate() {
        rb.MovePosition(rb.position + move.normalized * speed * Time.fixedDeltaTime);
    }
}
