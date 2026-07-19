using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private PlayerInput _playerInput;
    private PlayerInput.OnFootActions onFoot;

    private PlayerMovement motor;
    private PlayerLook look;
    private bool wasChatOpen;

    public PlayerInput.OnFootActions OnFoot => onFoot;

    private void Awake()
    {
        _playerInput = new PlayerInput();
        onFoot = _playerInput.OnFoot;

        motor = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();

        onFoot.Jump.performed += _ => motor.Jump();
        onFoot.Crouch.performed += _ => motor.Crouch();

        onFoot.Sprint.performed += _ => motor.Sprint(true);
        onFoot.Sprint.canceled += _ => motor.Sprint(false);
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        bool chatOpen = LobbyManager.IsChatOpen;

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
            return;

        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());

        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        if (_playerInput != null)
            onFoot.Enable();
    }

    private void OnDisable()
    {
        if (_playerInput != null)
            onFoot.Disable();
    }
}