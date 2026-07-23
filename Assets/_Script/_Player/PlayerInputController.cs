using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool offlineMode;

    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;

    private PlayerMovement motor;
    private PlayerLook look;
    private bool gameplayInputAllowed = true;

    public PlayerInput.OnFootActions OnFoot => onFoot;
    public bool GameplayInputAllowed => gameplayInputAllowed;

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

    private void Start()
    {
        if (!offlineMode)
            return;

        gameplayInputAllowed = true;
        RefreshInputState();
        LockCursor();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        gameplayInputAllowed = true;
        RefreshInputState();
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
        if (!CanControlPlayer())
            return;

        bool chatOpen = LobbyManager.IsChatOpen || PlayerChat.IsChatOpen;
        bool shouldProcessInput = gameplayInputAllowed && !chatOpen;

        if (shouldProcessInput != onFoot.enabled)
            RefreshInputState();

        if (!shouldProcessInput)
        {
            motor.ProcessMove(Vector2.zero);
            return;
        }

        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    public void SetGameplayInputEnabled(bool enabled)
    {
        if (!CanControlPlayer())
            return;

        gameplayInputAllowed = enabled;
        RefreshInputState();

        if (!enabled)
        {
            motor.Sprint(false);
            motor.ProcessMove(Vector2.zero);
        }
    }

    private void RefreshInputState()
    {
        bool chatOpen = LobbyManager.IsChatOpen || PlayerChat.IsChatOpen;
        bool shouldEnable = CanControlPlayer() && gameplayInputAllowed && !chatOpen;

        if (shouldEnable)
            onFoot.Enable();
        else
            onFoot.Disable();
    }

    private void OnEnable()
    {
        if (offlineMode || (IsSpawned && IsOwner))
            RefreshInputState();
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

    private bool CanControlPlayer()
    {
        return offlineMode || (IsSpawned && IsOwner);
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