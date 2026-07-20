using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;

    private PlayerMovement motor;
    private PlayerLook look;
    private bool wasChatOpen;

    public PlayerInput.OnFootActions OnFoot => onFoot;

    private void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        motor = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();

        onFoot.Jump.performed += _ => motor.Jump();
        onFoot.Crouch.performed += _ => motor.Crouch();

        onFoot.Sprint.performed += _ => motor.Sprint(true);
        onFoot.Sprint.canceled += _ => motor.Sprint(false);

        onFoot.Disable();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        onFoot.Enable();
        LockCursor();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        onFoot.Disable();
        UnlockCursor();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        bool chatOpen = LobbyManager.IsChatOpen || PlayerChat.IsChatOpen;

        if (chatOpen && !wasChatOpen)
        {
            onFoot.Disable();
        }
        else if (!chatOpen && wasChatOpen)
        {
            onFoot.Enable();
        }

        wasChatOpen = chatOpen;

        if (chatOpen)
        {
            motor.ProcessMove(Vector2.zero);
            return;
        }

        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());

        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    public void SetGameplayInputEnabled(bool enabled)
    {
        if (!IsSpawned || !IsOwner)
            return;

        if (enabled)
        {
            onFoot.Enable();
            return;
        }

        onFoot.Disable();
        motor.Sprint(false);
    }

    private void OnEnable()
    {
        if (IsSpawned && IsOwner)
            onFoot.Enable();
    }

    private void OnDisable()
    {
        if (playerInput != null)
            onFoot.Disable();
    }

    private void OnDestroy()
    {
        playerInput?.Dispose();
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}