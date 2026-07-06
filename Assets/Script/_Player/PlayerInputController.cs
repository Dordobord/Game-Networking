using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private PlayerInput _playerInput;
    private PlayerLook look;
    private PlayerInput.OnFootActions onFoot;
    public PlayerInput.OnFootActions OnFoot => onFoot;

    private PlayerMovement motor;

    void Awake()
    {
        _playerInput = new PlayerInput();
        onFoot = _playerInput.OnFoot;

        motor = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();

        onFoot.Jump.performed += ctx => motor.Jump();
        onFoot.Crouch.performed += ctx => motor.Crouch();

        onFoot.Sprint.performed += ctx => motor.Sprint(true);
        onFoot.Sprint.canceled += ctx => motor.Sprint(false);
    }

    void Update()
    {
        if (!IsOwner) return;
        
        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    void OnEnable()
    {
        onFoot.Enable();
    }

    void OnDisable()
    {
        onFoot.Disable();
    }
}