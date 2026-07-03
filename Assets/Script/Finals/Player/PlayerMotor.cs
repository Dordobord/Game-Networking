using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    private CharacterController cc;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    private float currentSpeed;

    [Header("Jump / Gravity")]
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float jumpHeight = 3f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint; // empty GameObject at the character's feet
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Crouch")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSpeed = 8f;

    private Vector3 velocity;
    private bool isGrounded;

    private bool isCrouching;
    private bool isSprinting;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        // Crouch is purely visual/height-based, so it's fine here regardless of script order.
        HandleCrouch();
    }

    // Ground check + gravity now run at the START of ProcessMove(), not in a separate
    // Update(). This guarantees they're always resolved BEFORE the move is applied on
    // the same frame, regardless of script execution order between InputManager and
    // PlayerMotor. That removes the per-frame inconsistency that was causing micro-stutter.
    public void ProcessMove(Vector2 input)
    {
        GroundCheck();
        HandleGravity();

        Vector3 moveInput = new Vector3(input.x, 0f, input.y);

        if (moveInput.magnitude > 1f)
            moveInput.Normalize();

        Vector3 horizontalVelocity =
            transform.TransformDirection(moveInput) * currentSpeed;

        Vector3 finalVelocity = horizontalVelocity + velocity;

        cc.Move(finalVelocity * Time.deltaTime);
    }

    void GroundCheck()
    {
        // CharacterController.isGrounded flickers true/false between frames even on flat
        // ground, because it only reflects the result of the LAST cc.Move() collision.
        // A small physics sphere check at the feet is stable and doesn't cause the
        // velocity.y sawtooth that was making the camera bob.
        if (groundCheckPoint != null)
        {
            isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundMask);
        }
        else
        {
            // Fallback if you haven't set up a ground check point yet.
            isGrounded = cc.isGrounded;
        }

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
    }

    void HandleGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }

    public void Jump()
    {
        if (!isGrounded) return;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void Sprint(bool sprinting)
    {
        isSprinting = sprinting;
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
    }

    public void Crouch()
    {
        isCrouching = !isCrouching;
    }

    void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;

        cc.height = Mathf.Lerp(cc.height,targetHeight,crouchSpeed * Time.deltaTime);

        cc.center = new Vector3(0f, cc.height / 2f, 0f);
    }
}