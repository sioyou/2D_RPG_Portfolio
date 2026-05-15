// Wrapper for Assets/InputSystem_Actions.inputactions (Unity "Generate C# Class" equivalent).
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem_Actions : IDisposable
{
    public InputActionAsset asset { get; }

    public InputSystem_Actions()
    {
        asset = LoadAsset();
    }

    public InputSystem_Actions(InputActionAsset source)
    {
        asset = source != null ? UnityEngine.Object.Instantiate(source) : LoadAsset();
    }

    static InputActionAsset LoadAsset()
    {
#if UNITY_EDITOR
        var editorAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
            "Assets/InputSystem_Actions.inputactions");
        if (editorAsset != null)
            return UnityEngine.Object.Instantiate(editorAsset);
#endif
        var resourceAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        if (resourceAsset != null)
            return UnityEngine.Object.Instantiate(resourceAsset);

        throw new InvalidOperationException(
            "InputSystem_Actions asset not found. Ensure Assets/InputSystem_Actions.inputactions exists " +
            "and a copy is in Assets/Resources/ for builds.");
    }

    public void Enable() => asset.Enable();
    public void Disable() => asset.Disable();

    public void Dispose()
    {
        if (asset != null)
            UnityEngine.Object.Destroy(asset);
    }

    public struct PlayerActions
    {
        readonly InputSystem_Actions _wrapper;
        public PlayerActions(InputSystem_Actions wrapper) => _wrapper = wrapper;

        public InputAction Move =>
            _wrapper.asset.FindActionMap("Player", throwIfNotFound: true).FindAction("Move", throwIfNotFound: true);
        public InputAction Attack =>
            _wrapper.asset.FindActionMap("Player", throwIfNotFound: true).FindAction("Attack", throwIfNotFound: true);
        public InputAction Interact =>
            _wrapper.asset.FindActionMap("Player", throwIfNotFound: true).FindAction("Interact", throwIfNotFound: true);
    }

    public struct UIActions
    {
        readonly InputSystem_Actions _wrapper;
        public UIActions(InputSystem_Actions wrapper) => _wrapper = wrapper;

        public InputAction Navigate =>
            _wrapper.asset.FindActionMap("UI", throwIfNotFound: true).FindAction("Navigate", throwIfNotFound: true);
        public InputAction Submit =>
            _wrapper.asset.FindActionMap("UI", throwIfNotFound: true).FindAction("Submit", throwIfNotFound: true);
        public InputAction Cancel =>
            _wrapper.asset.FindActionMap("UI", throwIfNotFound: true).FindAction("Cancel", throwIfNotFound: true);
    }

    public PlayerActions Player => new PlayerActions(this);
    public UIActions UI => new UIActions(this);
}
