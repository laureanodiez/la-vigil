using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float speed = 3f;
    public bool canMove = true;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 move;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update() {
        if (!canMove) return;

        // Captura input
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");

        // Normaliza diagonal para velocidad uniforme
        Vector2 clampedMove = move.normalized;

        // Animator parameters
        bool isMoving = clampedMove.sqrMagnitude > 0;
        anim.SetBool("isMoving", isMoving);

        if (isMoving) {
            anim.SetFloat("moveX", clampedMove.x);
            anim.SetFloat("moveY", clampedMove.y);
        }
    }

    void FixedUpdate() {
        if (!canMove) return;
        rb.MovePosition(rb.position + move.normalized * speed * Time.fixedDeltaTime);
    }
}
