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

        RotateTowardsMouse();
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

    [Rpc(SendTo.Server)]
    private void RotatePlayerServerRpc(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
    private void RotateTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);

            Vector3 lookDirection = point - transform.position;
            lookDirection.y = 0f;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDirection);

                RotatePlayerServerRpc(targetRot);
            }
        }
    }
    
    
}