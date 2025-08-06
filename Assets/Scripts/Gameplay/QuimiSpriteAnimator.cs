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

    // Componentes
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    // Estado
    private Vector2 move;
    private float timer;
    private bool toggleFrame;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void Update() {
        // 1) Movimiento
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");

        // 2) Control de Sprite
        if (move.sqrMagnitude > 0) {
            // A) Elegir orientación principal
            Sprite idle, mov;
            if (Mathf.Abs(move.x) > Mathf.Abs(move.y)) {
                // horizontal
                if (move.x > 0) { idle = rightIdle; mov = rightMove; }
                else          { idle = leftIdle;  mov = leftMove;  }
            } else {
                // vertical
                if (move.y > 0) { idle = backIdle;  mov = backMove;  }
                else            { idle = frontIdle; mov = frontMove; }
            }

            // B) Alternar frames MOV
            timer += Time.deltaTime;
            if (timer >= switchInterval) {
                timer = 0f;
                toggleFrame = !toggleFrame;
            }
            sr.sprite = toggleFrame ? mov : idle;
        }
        else {
            // Quieto → siempre idle de la última orientación:
            timer = 0f;
            toggleFrame = false;
            // Detectar última orientación:
            // Si mov.x o mov.y vienen de antes, podrías guardarlas; pero
            // para empezar usaremos frontIdle si no te importa.
            sr.sprite = frontIdle;
        }
    }

    void FixedUpdate() {
        rb.MovePosition(rb.position + move.normalized * speed * Time.fixedDeltaTime);
    }
}
