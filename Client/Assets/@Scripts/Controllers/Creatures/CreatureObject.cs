using Protocol;
using UnityEngine;

public class CreatureObject : BaseObject
{
    MoveComponent _move;
    StateComponent _state;
    AnimComponent _anim;

    protected MoveComponent Move => _move;
    protected StateComponent State => _state;
    protected AnimComponent Anim => _anim;

    protected override void Awake()
    {
        base.Awake();
        _move = Utils.GetOrAddComponent<MoveComponent>(gameObject);
        _state = Utils.GetOrAddComponent<StateComponent>(gameObject);
        _state.OnStateFlagsChanged -= OnStateFlagsChanged;
        _state.OnStateFlagsChanged += OnStateFlagsChanged;
        _anim = Utils.GetOrAddComponent<AnimComponent>(gameObject);
    }

    private void OnStateFlagsChanged(int state)
    {
        _anim?.PlayAnim(state);
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

    public virtual void ApplyMoveDirection(float dirX, float dirY)
    {
        ApplyFacing(dirX);
    }

    protected void ApplyFacing(float dirX)
    {
        if (Mathf.Abs(dirX) <= 0.0001f)
            return;

        Anim?.SetFlip(dirX > 0f);
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
