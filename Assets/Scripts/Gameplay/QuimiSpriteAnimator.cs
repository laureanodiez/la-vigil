using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class QuimiSpriteAnimator : MonoBehaviour
{
    [Header("Control")]
    public bool canMove = true; // Esta es la variable clave

    [Header("Sprites")]
    public Sprite frontIdle, frontMove1, frontMove2;
    public Sprite backIdle,  backMove1,  backMove2;
    public Sprite leftIdle,  leftMove1,  leftMove2;
    public Sprite rightIdle, rightMove1, rightMove2;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;      
    public float runSpeed = 5f;       
    
    public float acceleration = 20f;  
    public float deceleration = 25f;  

    [Header("Animation")]
    public float baseStepInterval = 0.25f;
    
    [Header("Audio")]
    [SerializeField] public AudioSource steps;

    // Componentes
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    // Estado interno
    private Vector2 moveInput;
    private float currentSpeed;
    private float animationTimer;
    private int stepIndex = 0; 

    private enum Direction { Front, Back, Left, Right }
    private Direction lastDirection = Direction.Front;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        currentSpeed = walkSpeed;
        if(steps != null) steps.loop = true;
    }

    // Método público para llamar desde tus otros scripts
    public void SetInputActive(bool active)
    {
        canMove = active;
        
        // Opcional: Si desactivamos input, forzamos frenado inmediato de la física también
        if (!active) {
            rb.linearVelocity = Vector2.zero; 
            currentSpeed = 0;
        }
    }

    void Update() {
        
        // 1. INPUT MODIFICADO
        // Si canMove es true, leemos teclado. Si es false, moveInput es cero.
        if (canMove)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            moveInput = Vector2.zero;
        }

        // A partir de aquí, tu lógica original funciona perfecto.
        // Al ser moveInput (0,0), el código saltará automáticamente al 'else' de abajo.

        if (moveInput.sqrMagnitude > 0) {
            
            // Lógica de dirección...
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

            // Frecuencia...
            float speedMultiplier = currentSpeed / walkSpeed;
            if(speedMultiplier < 0.1f) speedMultiplier = 1f; 

            float dynamicInterval = baseStepInterval / speedMultiplier;

            // Ciclo de Animación...
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= dynamicInterval) {
                animationTimer = 0f;
                stepIndex++; 
                if (stepIndex > 3) stepIndex = 0;

                if((stepIndex == 1 || stepIndex == 3) && steps != null) {
                    steps.pitch = Random.Range(0.9f, 1.1f); 
                    steps.Play();
                }
            }

            // Asignar Sprite...
            if (stepIndex == 0 || stepIndex == 2) sr.sprite = idle;
            else if (stepIndex == 1) sr.sprite = mov1;
            else if (stepIndex == 3) sr.sprite = mov2;
        }
        else {
            // ESTO ES LO QUE QUERÍAS:
            // Al cortar el input, el código cae aquí naturalmente.
            // Se resetea el timer, el index, se para el audio y se pone el IDLE.
            
            animationTimer = 0f;
            stepIndex = 0;
            if (steps != null && steps.isPlaying) steps.Stop();

            switch (lastDirection)
            {
                case Direction.Right: sr.sprite = rightIdle; break;
                case Direction.Left:  sr.sprite = leftIdle; break;
                case Direction.Back:  sr.sprite = backIdle; break;
                default:              sr.sprite = frontIdle; break;
            }
        }
    }

    void FixedUpdate() {
        // Si no hay input (porque canMove es false), moveInput es (0,0), 
        // así que el personaje se detendrá suavemente según tu lógica de desaceleración.
        rb.MovePosition(rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime);

        // Si canMove es false, moveInput es 0, por lo que querrás que currentSpeed baje a 0
        // Nota: Input.GetKey seguirá funcionando si no lo bloqueamos, así que mejor usamos canMove también aquí.
        
        float targetSpeed = 0f;
        
        if (canMove) {
            targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        } 
        // Si canMove es false, targetSpeed se queda en 0.

        float rate = (targetSpeed > currentSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
    }
}