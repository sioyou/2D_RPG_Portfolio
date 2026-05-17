using UnityEngine;

/// <summary>
/// Keeps on-screen sprite size stable when the window is resized by scaling orthographic size with screen height.
/// Reference is 1080p; min/max clamps avoid overly blurry (tiny window) or tiny (huge monitor) characters.
/// </summary>
[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(-100)]
public class OrthographicCameraScaler : MonoBehaviour
{
    [Header("Reference (1080p baseline)")]
    [SerializeField] float _referenceOrthoSize = 7.5f;
    [SerializeField] float _referenceHeight = 1080f;

    [Header("Quality clamps")]
    [Tooltip("Do not zoom out past 720p equivalent — avoids tiny sprites on small windows.")]
    [SerializeField] float _minOrthoSize = 5f;
    [Tooltip("Do not zoom in past this on very tall displays — keeps characters readable on 4K.")]
    [SerializeField] float _maxOrthoSize = 9f;

    Camera _camera;
    int _lastWidth;
    int _lastHeight;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographic = true;
        Apply();
    }

    void Update()
    {
        if (_lastWidth == Screen.width && _lastHeight == Screen.height)
            return;

        Apply();
    }

    void Apply()
    {
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        if (_referenceHeight <= 0f)
            return;

        float scale = Screen.height / _referenceHeight;
        float orthoSize = _referenceOrthoSize * scale;
        orthoSize = Mathf.Clamp(orthoSize, _minOrthoSize, _maxOrthoSize);

        _camera.orthographicSize = orthoSize;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (_referenceHeight <= 0f)
            _referenceHeight = 1080f;
        if (_minOrthoSize > _maxOrthoSize)
            _minOrthoSize = _maxOrthoSize;
    }
#endif
}
