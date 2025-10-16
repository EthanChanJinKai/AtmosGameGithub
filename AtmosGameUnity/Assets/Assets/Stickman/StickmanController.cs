using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class StickmanController : MonoBehaviour
{
    
    public float walkSpeed = 3.0f;
    public float sprintSpeed = 6.0f;

    public float gravity = -9.81f;
    public float rotationSpeed = 2.0f;

    public float jumpHeight = 2.0f;

    private bool isJumping = false; 

    
    private CharacterController characterController;
    private Animator animator; 
    private Vector3 velocity; 
    private Vector3 currentMomentum;

    private float currentMoveSpeed;

    void Start()
    {
        
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentMoveSpeed = sprintSpeed;
        }
        else
        {
            currentMoveSpeed = walkSpeed;
        }

        float horizontalInput = Input.GetAxis("Horizontal"); // A/D
        float verticalInput = Input.GetAxis("Vertical");   // W/S

        Vector3 desiredMove = transform.forward * verticalInput + transform.right * horizontalInput;

        if (desiredMove.magnitude >= 0.1f)
        {
            currentMomentum = desiredMove.normalized;
        }
        else if (characterController.isGrounded)
        {
            
            currentMomentum = Vector3.zero;
        }

        velocity.x = currentMomentum.x * currentMoveSpeed;
        velocity.z = currentMomentum.z * currentMoveSpeed;

        

        if (characterController.isGrounded)
        {
            isJumping = false;
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isJumping = true;

                animator.SetTrigger("JumpTrigger");
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        characterController.Move(velocity * Time.deltaTime);

        float currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude;

        float speedPercent = Mathf.Min(1.0f, currentHorizontalSpeed / sprintSpeed);

        animator.SetFloat("SpeedPercent", speedPercent);

    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
