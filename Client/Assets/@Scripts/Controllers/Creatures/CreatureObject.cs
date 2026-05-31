using System.Collections;
using Protocol;
using UnityEngine;

public class CreatureObject : BaseObject
{
    const float DieFallbackDuration = 0.5f;

    private Coroutine _dieRoutine;

    private MoveComponent _move;
    private StateComponent _state;
    private AnimComponent _anim;
    private StatComponent _stat;

    private UI_WorldHpBar _worldHpBar;

    protected MoveComponent Move => _move;
    protected StateComponent State => _state;
    protected AnimComponent Anim => _anim;
    protected StatComponent Stat => _stat;

    protected override void Awake()
    {
        base.Awake();
        _move = Utils.GetOrAddComponent<MoveComponent>(gameObject);
        _state = Utils.GetOrAddComponent<StateComponent>(gameObject);
        _state.OnStateFlagsChanged -= OnStateFlagsChanged;
        _state.OnStateFlagsChanged += OnStateFlagsChanged;
        _anim = Utils.GetOrAddComponent<AnimComponent>(gameObject);
        _stat = Utils.GetOrAddComponent<StatComponent>(gameObject);
        _stat.OnStatChanged -= OnStatChanged;
        _stat.OnStatChanged += OnStatChanged;

        _worldHpBar = Managers.UI.MakeWorldSpaceUI<UI_WorldHpBar>(gameObject.transform, "UI_WorldHpBar");
    }

    private void OnStateFlagsChanged(int state)
    {
        _anim?.PlayAnim(state);
    }

    void OnStatChanged(StatChangeFlags flags)
    {
        if ((flags & StatChangeFlags.Hp) != 0)
            RefreshWorldHpBar();

        if ((flags & StatChangeFlags.MaxHp) != 0)
            RefreshWorldHpBar();
    }

    void RefreshWorldHpBar()
    {
        if (_worldHpBar == null || _stat == null)
            return;

        _worldHpBar.SetHpValue(_stat.HpRatio);
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
        _stat?.ApplyFrom(info);
        ApplyStateFlags(info.StateFlags);
        ApplySpawnFacing(info.DirX, info.DirY);
    }

    protected void ApplySpawnFacing(float dirX, float dirY)
    {
        if (Mathf.Abs(dirX) <= 0.0001f && Mathf.Abs(dirY) <= 0.0001f)
            dirX = 1f;

        ApplyFacing(dirX);
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

    public virtual void ApplyAttack(float dirX, float dirY)
    {
        ApplyFacing(dirX);
        _state?.AddState(CreatureState.Attack);
        Invoke(nameof(ClearAttackState), 0.5f);
    }

    public virtual void ApplyDamaged(float faceDirX, int targetHp = -1)
    {
        if (State != null && State.HasState(CreatureState.Dead))
            return;

        if (targetHp >= 0)
            _stat?.SetHpFromServer(targetHp);

        ApplyFacing(faceDirX);
        _state?.AddState(CreatureState.Damaged);
        Invoke(nameof(ClearDamagedState), 0.3f);
    }

    public virtual void ApplyDie()
    {
        CancelTransientStateInvokes();
        StopDieRoutine();

        _state?.ApplyNetworkFlags(CreatureStateUtil.ToBitMask(CreatureState.Dead));
        _dieRoutine = StartCoroutine(CoDieAndDespawn());
    }

    IEnumerator CoDieAndDespawn()
    {
        float duration = Anim != null ? Anim.GetClipLength(AnimName.DIE) : DieFallbackDuration;
        if (duration <= 0f)
            duration = DieFallbackDuration;

        yield return new WaitForSeconds(duration);

        _dieRoutine = null;
        DespawnAfterDie();
    }

    protected virtual void DespawnAfterDie()
    {
        if (Info != null)
            Managers.Object.Despawn(Info.ObjectId);
    }

    void StopDieRoutine()
    {
        if (_dieRoutine == null)
            return;

        StopCoroutine(_dieRoutine);
        _dieRoutine = null;
    }

    void OnDestroy()
    {
        if (_stat != null)
            _stat.OnStatChanged -= OnStatChanged;

        StopDieRoutine();
    }

    protected void CancelTransientStateInvokes()
    {
        CancelInvoke(nameof(ClearAttackState));
        CancelInvoke(nameof(ClearDamagedState));
    }

    void ClearAttackState()
    {
        _state?.RemoveState(CreatureState.Attack);
    }

    void ClearDamagedState()
    {
        _state?.RemoveState(CreatureState.Damaged);
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
