using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class QuimiSpriteAnimator : MonoBehaviour
{
    [Header("Control")]
    public bool canMove = true; 

    [Header("Cinemática")]
    public bool isCinematic = false; // <-- Nuevo: Indica si el Timeline tomó el control
    private Vector2 cinematicInput = Vector2.zero;

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

    public void SetInputActive(bool active)
    {
        canMove = active;
        
        if (!active) {
            rb.linearVelocity = Vector2.zero; 
            currentSpeed = 0;
        }
    }

    void Update() {
        
        // 1. CONTROL DE INPUT: ¿Quién maneja?
        if (isCinematic)
        {
            // El Timeline dicta hacia dónde ir
            moveInput = cinematicInput;
        }
        else if (canMove)
        {
            // El jugador tiene el control
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            // Nadie tiene el control (ej: en un diálogo quieto)
            moveInput = Vector2.zero;
        }

        // A partir de aquí, la lógica de animación original fluye natural...
        if (moveInput.sqrMagnitude > 0) {
            
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

            float speedMultiplier = currentSpeed / walkSpeed;
            if(speedMultiplier < 0.1f) speedMultiplier = 1f; 

            float dynamicInterval = baseStepInterval / speedMultiplier;

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

            if (stepIndex == 0 || stepIndex == 2) sr.sprite = idle;
            else if (stepIndex == 1) sr.sprite = mov1;
            else if (stepIndex == 3) sr.sprite = mov2;
        }
        else {
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
        rb.MovePosition(rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime);
        
        float targetSpeed = 0f;
        
        // 2. CONTROL DE VELOCIDAD FÍSICA
        if (isCinematic) 
        {
            // En cinemáticas, queremos que camine a velocidad normal, 
            // aunque el jugador no pueda tocar los controles.
            targetSpeed = walkSpeed; 
        }
        else if (canMove) 
        {
            targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        } 

        float rate = (targetSpeed > currentSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
    }

    // --- 3. MÉTODOS PÚBLICOS PARA TIMELINE SIGNALS ---
    public void CinematicaCaminarArriba()    { isCinematic = true; cinematicInput = Vector2.up; }
    public void CinematicaCaminarAbajo()     { isCinematic = true; cinematicInput = Vector2.down; }
    public void CinematicaCaminarIzquierda() { isCinematic = true; cinematicInput = Vector2.left; }
    public void CinematicaCaminarDerecha()   { isCinematic = true; cinematicInput = Vector2.right; }
    public void CinematicaMirarArriba()    { CinematicaFrenar(); lastDirection = Direction.Back;  sr.sprite = backIdle; }
    public void CinematicaMirarAbajo()     { CinematicaFrenar(); lastDirection = Direction.Front; sr.sprite = frontIdle; }
    public void CinematicaMirarIzquierda() { CinematicaFrenar(); lastDirection = Direction.Left;  sr.sprite = leftIdle; }
    public void CinematicaMirarDerecha()   { CinematicaFrenar(); lastDirection = Direction.Right; sr.sprite = rightIdle; }
    
    public void CinematicaFrenar() 
    { 
        isCinematic = false; 
        cinematicInput = Vector2.zero; 
        moveInput = Vector2.zero; 
    }
}