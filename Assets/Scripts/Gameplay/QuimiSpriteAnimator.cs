using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class QuimiSpriteAnimator : MonoBehaviour
{
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
    public float baseStepInterval = 0.25f; // Un poco más rápido para que el ciclo de 4 no se sienta lento
    
    [Header("Audio")]
    [SerializeField] public AudioSource steps;

    // Componentes
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    // Estado interno
    private Vector2 moveInput;
    private float currentSpeed;
    private float animationTimer;
    
    // CAMBIO: En vez de bool, usamos un int para contar pasos (0, 1, 2, 3)
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

    void Update() {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 0) {
            
            // 1. Definir qué set de sprites usar según dirección
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

            // 2. Calcular Frecuencia (igual que antes)
            float speedMultiplier = currentSpeed / walkSpeed;
            // Evitamos dividir por 0 si la velocidad es muy baja
            if(speedMultiplier < 0.1f) speedMultiplier = 1f; 

            float dynamicInterval = baseStepInterval / speedMultiplier;

            // 3. Ciclo de Animación de 4 Pasos
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= dynamicInterval) {
                animationTimer = 0f;
                stepIndex++; // Pasamos al siguiente frame
                
                // Si llegamos a 4, volvemos a 0 (El ciclo es 0, 1, 2, 3)
                if (stepIndex > 3) stepIndex = 0;

                // Sonido: Solo suena en los frames de movimiento (1 y 3), no en los idle
                if((stepIndex == 1 || stepIndex == 3) && steps != null) {
                    steps.pitch = Random.Range(0.9f, 1.1f); //* speedMultiplier;
                    steps.Play();
                }
            }

            // 4. Asignar el Sprite según el paso actual
            if (stepIndex == 0 || stepIndex == 2) {
                sr.sprite = idle;
            }
            else if (stepIndex == 1) {
                sr.sprite = mov1;
            }
            else if (stepIndex == 3) {
                sr.sprite = mov2;
            }
        }
        else {
            // Quieto
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

        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float rate = (targetSpeed > currentSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
    }
}