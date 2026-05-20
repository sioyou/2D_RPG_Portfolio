using Protocol;
using UnityEngine;

public class CreatureObject : BaseObject
{
    MoveComponent _move;

    protected MoveComponent Move => _move;

    protected override void Awake()
    {
        base.Awake();
        _move = Utils.GetOrAddComponent<MoveComponent>(gameObject);
    }

    public bool LerpCompleted => _move != null && _move.LerpCompleted;

    public bool Interpolate
    {
        get => _move != null && _move.Interpolate;
        set
        {
            if (_move != null)
                _move.Interpolate = value;
        }
    }

    public float SqrDistanceToDestination =>
        _move != null ? _move.SqrDistanceToDestination : 0f;

    protected override void OnInfoSet(ObjectInfo info)
    {
        if (_move != null)
            _move.Interpolate = !IsMyPlayer;

        SyncDestinationToCurrent();
    }

    public virtual void ApplyDestPosition(Vector2 pos)
    {
        SetDestination(pos.x, pos.y);
    }

    protected void SetPosition(float posX, float posY)
    {
        _move?.SnapTo(posX, posY);
    }

    protected void SetPosition(Vector3 pos)
    {
        _move?.SnapTo(pos);
    }

    protected void SetDestination(float posX, float posY)
    {
        _move?.SetDestination(posX, posY);
    }

    protected void SetDestination(Vector3 dest)
    {
        _move?.SetDestination(dest);
    }

    protected void SyncDestinationToCurrent()
    {
        _move?.SyncDestinationFromTransform();
    }

    protected void AddWorldDelta(Vector2 worldDelta)
    {
        if (_move == null)
            return;

        Vector3 pos = transform.position;
        _move.SnapTo(pos.x + worldDelta.x, pos.y + worldDelta.y);
    }
}
