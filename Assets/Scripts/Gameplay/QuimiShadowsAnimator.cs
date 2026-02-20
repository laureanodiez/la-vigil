using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class QuimiShadowAnimator : MonoBehaviour
{
    [Header("Control")]
    public bool canMove = true; // <-- Nuestro interruptor

    [Header("Sprites Sombra Perspectiva")]
    public Sprite frontIdle, frontMove1, frontMove2;
    public Sprite backIdle,  backMove1,  backMove2;
    public Sprite leftIdle,  leftMove1,  leftMove2;
    public Sprite rightIdle, rightMove1, rightMove2;

    [Header("Animation")]
    public float stepInterval = 0.25f;

    private SpriteRenderer sr;
    
    // Estado interno
    private Vector2 moveInput;
    private float animationTimer;
    private int stepIndex = 0; 
    private enum Direction { Front, Back, Left, Right }
    private Direction lastDirection = Direction.Front;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Método para apagar/prender desde tus diálogos o cinemáticas
    public void SetInputActive(bool active)
    {
        canMove = active;
    }

    void Update() 
    {
        // 1. Controlamos si leemos el teclado o forzamos a cero
        if (canMove)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            moveInput = Vector2.zero;
        }

        // 2. Lógica de animación
        if (moveInput.sqrMagnitude > 0) 
        {
            Sprite idle, mov1, mov2;

            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)) {
                if (moveInput.x > 0) { 
                    idle = rightIdle; mov1 = rightMove1; mov2 = rightMove2; 
                    lastDirection = Direction.Right; 
                } else { 
                    idle = leftIdle;  mov1 = leftMove1;  mov2 = leftMove2; 
                    lastDirection = Direction.Left;  
                }
            } else {
                if (moveInput.y > 0) { 
                    idle = backIdle;  mov1 = backMove1;  mov2 = backMove2; 
                    lastDirection = Direction.Back; 
                } else { 
                    idle = frontIdle; mov1 = frontMove1; mov2 = frontMove2; 
                    lastDirection = Direction.Front; 
                }
            }

            animationTimer += Time.deltaTime;
            
            if (animationTimer >= stepInterval) {
                animationTimer = 0f;
                stepIndex++; 
                if (stepIndex > 3) stepIndex = 0;
            }

            if (stepIndex == 0 || stepIndex == 2) sr.sprite = idle;
            else if (stepIndex == 1) sr.sprite = mov1;
            else if (stepIndex == 3) sr.sprite = mov2;
        }
        else 
        {
            // 3. Al cortar el input, la sombra se queda quieta respetando la última dirección
            animationTimer = 0f;
            stepIndex = 0;

            switch (lastDirection)
            {
                case Direction.Right: sr.sprite = rightIdle; break;
                case Direction.Left:  sr.sprite = leftIdle; break;
                case Direction.Back:  sr.sprite = backIdle; break;
                default:              sr.sprite = frontIdle; break;
            }
        }
    }
}