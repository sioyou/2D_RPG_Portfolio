using Protocol;
using UnityEngine;

public class CreatureObject : BaseObject
{
    MoveComponent _move;
    StateComponent _state;

    protected MoveComponent Move => _move;
    protected StateComponent State => _state;

    protected override void Awake()
    {
        base.Awake();
        _move = Utils.GetOrAddComponent<MoveComponent>(gameObject);
        _state = Utils.GetOrAddComponent<StateComponent>(gameObject);
    }

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
        ApplyStateFlags(info.StateFlags);
    }

    public virtual void ApplyDestPosition(Vector2 pos)
    {
        SetDestination(pos.x, pos.y);
    }

    public virtual void ApplyStateFlags(int stateFlags)
    {
        _state?.ApplyNetworkFlags(stateFlags);
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
