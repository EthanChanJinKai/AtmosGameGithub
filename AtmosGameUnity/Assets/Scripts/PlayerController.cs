using UnityEngine;

/// <summary>
/// A robust third-person controller for Humanoid rigs. 
/// Uses CharacterController for stable, non-physics movement, 
/// and includes running, jumping, gravity, and camera-relative rotation.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    // --- MOVEMENT TUNING ---
    [Header("Movement Speeds")]
    public float walkSpeed = 3f;
    public float runSpeed = 7f;

    [Header("Rotation & Smoothing")]
    [Tooltip("How quickly the player turns to face the direction of movement.")]
    public float turnSmoothTime = 0.1f;
    [Tooltip("How quickly the movement speed changes (acceleration/deceleration).")]
    public float speedSmoothTime = 0.1f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -25f; // A bit stronger gravity feels snappier

    // --- PRIVATE STATE & REFERENCES ---
    private float turnSmoothVelocity;
    private float speedSmoothVelocity;
    private float currentSpeed;
    private float verticalVelocity; // Stores speed for falling and jumping

    private Animator animator;
    private CharacterController controller;
    private Transform mainCameraTransform;

    void Awake()
    {
        // Get the mandatory components
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Cache the main camera's transform for direction reference
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("FATAL ERROR: Scene requires a GameObject with the 'MainCamera' tag.");
        }
    }

    void Update()
    {
        // 1. --- Input and Direction Calculation ---

        // Get raw input values (-1 to 1) for X (Horizontal) and Z (Vertical/Forward)
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 inputDir = new Vector3(input.x, 0f, input.y).normalized;

        // 2. --- Rotation and Movement ---

        if (inputDir.magnitude >= 0.1f)
        {
            // Calculate the target angle based on the camera's Y rotation (where the camera is looking)
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;

            // Smoothly rotate the player's body to face the direction of movement
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calculate the movement direction vector (this is the direction the character is facing)
            Vector3 moveDir = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;

            // Apply horizontal movement
            MoveAndAnimate(inputDir.magnitude, moveDir);
        }
        else
        {
            // If no input, set speed to zero and only apply gravity
            MoveAndAnimate(0f, Vector3.zero);
        }

        // 3. --- Gravity and Jump ---
        HandleGravityAndJump();
    }

    private void MoveAndAnimate(float inputMagnitude, Vector3 moveDirection)
    {
        // Determine if the player is holding the Run key (Left Shift is default "Fire3" button)
        bool running = Input.GetKey(KeyCode.LeftShift);

        // Calculate the target speed based on input magnitude and run state
        float speedGoal = ((running) ? runSpeed : walkSpeed) * inputMagnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, speedGoal, ref speedSmoothVelocity, speedSmoothTime);

        // Combine horizontal movement with vertical velocity (gravity/jump)
        Vector3 finalMovement = moveDirection.normalized * currentSpeed;
        finalMovement.y = verticalVelocity;

        // Apply movement using the CharacterController
        controller.Move(finalMovement * Time.deltaTime);

        // --- Animation Control ---

        // Calculate a blend value (0 for idle, 0.5 for walk, 1.0 for run)
        float animationBlendValue = ((running) ? 1f : 0.5f) * inputMagnitude;

        // Send the value to the Animator
        animator.SetFloat("SpeedPercent", animationBlendValue, speedSmoothTime, Time.deltaTime);
    }

    private void HandleGravityAndJump()
    {
        // Check if the CharacterController is touching the ground
        if (controller.isGrounded)
        {
            // Reset vertical velocity when grounded
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            // Handle Jump Input (Default Unity "Jump" is Spacebar)
            if (Input.GetButtonDown("Jump"))
            {
                // Calculate the required initial velocity to reach jumpHeight
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                // Optionally add an animator trigger here for a jump animation:
                // animator.SetTrigger("Jump");
            }
        }

        // Apply constant gravity
        verticalVelocity += gravity * Time.deltaTime;
    }
}
