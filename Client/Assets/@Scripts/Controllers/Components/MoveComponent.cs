using System;
using UnityEngine;

/// <summary>
/// 월드 위치 보간만 담당
/// </summary>
public class MoveComponent : MonoBehaviour
{
    [SerializeField] float _lerpSpeed = 15f;
    [SerializeField] float _arriveSqrThreshold = 0.0004f;

    Vector3 _destination;
    bool _hasDestination;
    bool _interpolate = true;

    public bool Interpolate
    {
        get => _interpolate;
        set => _interpolate = value;
    }

    public float LerpSpeed
    {
        get => _lerpSpeed;
        set => _lerpSpeed = value;
    }

    public float ArriveSqrThreshold
    {
        get => _arriveSqrThreshold;
        set => _arriveSqrThreshold = value;
    }

    public bool HasDestination => _hasDestination;

    public Vector3 Destination => _destination;

    public float SqrDistanceToDestination =>
        _hasDestination ? XySqrMagnitude(transform.position, _destination) : 0f;

    public bool LerpCompleted { get; private set; } = true;

    public event Action ArrivedAtDestination;

    void Update()
    {
        if (!_interpolate || !_hasDestination)
            return;

        float z = transform.position.z;

        if (SqrDistanceToDestination < _arriveSqrThreshold)
        {
            transform.position = new Vector3(_destination.x, _destination.y, z);
            _destination = transform.position;
            _hasDestination = false;
            LerpCompleted = true;
            ArrivedAtDestination?.Invoke();
            return;
        }

        Vector3 cur = transform.position;
        Vector3 next = Vector3.Lerp(cur, _destination, _lerpSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, z);
        LerpCompleted = false;
    }

    /// <summary>즉시 이동. Z는 현재 transform 유지.</summary>
    public void SnapTo(float x, float y)
    {
        float z = transform.position.z;
        SnapTo(new Vector3(x, y, z));
    }

    public void SnapTo(Vector3 worldPosition)
    {
        float z = transform.position.z;
        transform.position = new Vector3(worldPosition.x, worldPosition.y, z);
        _destination = transform.position;
        _hasDestination = true;
        LerpCompleted = true;
    }

    /// <summary>목표만 설정. Z는 호출 시점 transform 기준.</summary>
    public void SetDestination(float x, float y)
    {
        float z = transform.position.z;
        SetDestination(new Vector3(x, y, z));
    }

    public void SetDestination(Vector3 worldDestination)
    {
        float z = transform.position.z;
        _destination = new Vector3(worldDestination.x, worldDestination.y, z);
        _hasDestination = true;
        LerpCompleted = false;
    }

    public void SyncDestinationFromTransform()
    {
        _destination = transform.position;
        _hasDestination = true;
        LerpCompleted = true;
    }

    public void ClearDestination()
    {
        _hasDestination = false;
        LerpCompleted = true;
    }

    static float XySqrMagnitude(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return dx * dx + dy * dy;
    }
}
