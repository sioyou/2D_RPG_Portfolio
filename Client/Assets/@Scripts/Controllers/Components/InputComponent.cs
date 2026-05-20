using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerInput Send Messages. InputSystem_Actions는 Addressables Preload로 로드됨.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class InputComponent : MonoBehaviour
{
    const string InputActionsKey = "InputSystem_Actions";
    const float MoveDeadzone = 0.15f;

    [SerializeField] InputActionAsset _inputActionsAsset;

    PlayerInput _playerInput;

    public Vector2 MoveDirection { get; private set; }

    public event Action<Vector2> OnMoveAction;
    public event Action OnAttackAction;
    public event Action OnInteractAction;

    void Awake()
    {
        InputActionAsset asset = ResolveInputAsset();
        if (asset == null)
        {
            Debug.LogError($"[InputComponent] InputActionAsset not found. Add '{InputActionsKey}' to Addressables with Preload label.");
            enabled = false;
            return;
        }

        _playerInput = gameObject.GetOrAddComponent<PlayerInput>();
        _playerInput.actions = Instantiate(asset);
        _playerInput.defaultActionMap = "Player";
        _playerInput.notificationBehavior = PlayerNotifications.SendMessages;
    }

    InputActionAsset ResolveInputAsset()
    {
        if (_inputActionsAsset != null)
            return _inputActionsAsset;

        if (Managers.Initialized)
        {
            InputActionAsset cached = Managers.Resource.Load<InputActionAsset>(InputActionsKey);
            if (cached != null)
                return cached;
        }

        return null;
    }

    void OnEnable()
    {
        if (_playerInput == null)
            return;

        _playerInput.ActivateInput();
    }

    void OnDisable()
    {
        SetMove(Vector2.zero);

        if (_playerInput != null)
            _playerInput.DeactivateInput();
    }

    #region PlayerInput Send Messages

    public void OnMove(InputValue value)
    {
        SetMove(value.Get<Vector2>());
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed == false)
            return;

        OnAttackAction?.Invoke();
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed == false)
            return;

        OnInteractAction?.Invoke();
    }

    #endregion

    void SetMove(Vector2 direction)
    {
        if (direction.sqrMagnitude > 1f)
            direction = direction.normalized;

        if (direction.sqrMagnitude < MoveDeadzone * MoveDeadzone)
            direction = Vector2.zero;

        if (MoveDirection == direction)
            return;

        MoveDirection = direction;
        OnMoveAction?.Invoke(direction);
    }
}
