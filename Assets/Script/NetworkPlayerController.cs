using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;
    [SerializeField] private float jumpHeight = 5f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector2 movementInput = new Vector2(horizontalInput, verticalInput);

        bool jumpInput = Input.GetKeyDown(KeyCode.Space);

        if (IsServer)
        {
            MovePlayer(movementInput, jumpInput);
        }
        else
        {
            MovePlayerRpc(movementInput, jumpInput);
        }
    }

    [Rpc(SendTo.Server)]
    private void MovePlayerRpc(Vector2 movementInput, bool jumpPressed)
    {
        MovePlayer(movementInput, jumpPressed);
    }

    private void MovePlayer(Vector2 movementInput, bool jumpPressed)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // ADDED
        if (characterController.isGrounded && jumpPressed)
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        Vector3 moveDirection = new Vector3(movementInput.x, 0f, movementInput.y).normalized;

        Vector3 horizontalMovement = moveDirection * moveSpeed;

        Vector3 verticalMovement = Vector3.up * verticalVelocity;

        Vector3 finalMovement = horizontalMovement + verticalMovement;

        characterController.Move(finalMovement * Time.deltaTime);
    }
}