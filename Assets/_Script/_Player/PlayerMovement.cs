using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
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
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Crouch")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionTime = 1f;

    [Header("Crouch Camera")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float standCamY = 1f;
    [SerializeField] private float crouchCamY = 0.9f;

    private Vector3 velocity;
    private bool isGrounded;

    private bool isCrouching;
    private bool isSprinting;

    private bool lerpCrouch;
    private float crouchTimer;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / crouchTransitionTime;
            p *= p; // ease in

            float targetHeight = isCrouching ? crouchHeight : standHeight;
            cc.height = Mathf.Lerp(cc.height, targetHeight, p);
            cc.center = new Vector3(0f, cc.height / 2f - standHeight / 2f, 0f);

            if (cameraHolder != null)
            {
                float targetCamY = isCrouching ? crouchCamY : standCamY;
                Vector3 camPos = cameraHolder.localPosition;
                camPos.y = Mathf.Lerp(camPos.y, targetCamY, p);
                cameraHolder.localPosition = camPos;
            }

            if (p > 1f)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
    }

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
        if (groundCheckPoint != null)
        {
            isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundMask);
        }
        else
        {
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
        crouchTimer = 0f;
        lerpCrouch = true;
    }
}