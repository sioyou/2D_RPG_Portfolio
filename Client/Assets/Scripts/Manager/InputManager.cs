using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions _actions;

    // Phase 1에서 구독
    public event Action<Vector2> OnMove;
    public event Action OnAttack;
    public event Action OnInteract;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _actions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _actions.Enable();

        _actions.Player.Move.performed += HandleMove;
        _actions.Player.Move.canceled  += HandleMove;
        _actions.Player.Attack.performed  += HandleAttack;
        _actions.Player.Interact.performed += HandleInteract;
    }

    private void OnDisable()
    {
        _actions.Player.Move.performed -= HandleMove;
        _actions.Player.Move.canceled  -= HandleMove;
        _actions.Player.Attack.performed  -= HandleAttack;
        _actions.Player.Interact.performed -= HandleInteract;

        _actions.Disable();
    }

    private void HandleMove(InputAction.CallbackContext ctx)
        => OnMove?.Invoke(ctx.ReadValue<Vector2>());

    private void HandleAttack(InputAction.CallbackContext ctx)
        => OnAttack?.Invoke();

    private void HandleInteract(InputAction.CallbackContext ctx)
        => OnInteract?.Invoke();
}
