using Protocol;
using UnityEngine;

public class MyPlayerObject : PlayerObject
{
    const float MoveDeadzoneSqr = 0.02f * 0.02f;
    const float AttackCooldownTime = 0.5f;

    [SerializeField] float _correctionThreshold = 0.5f;
    [SerializeField] float _syncInterval = 0.1f;

    InputComponent _input;
    Vector2 _moveDir;
    Vector2 _lastAimDir = Vector2.right;
    float _syncTimer;
    float _attackCooldown;

    int _lastSentStateFlags = -1;
    float _lastSentPosX;
    float _lastSentPosY;
    float _lastSentDirX;
    float _lastSentDirY;

    protected override void OnInfoSet(ObjectInfo info)
    {
        base.OnInfoSet(info);
        Managers.Game.BindMyPlayer(this);
        State?.ClearActionFlags();

        if (Mathf.Abs(info.DirX) > 0.0001f || Mathf.Abs(info.DirY) > 0.0001f)
            _lastAimDir = new Vector2(info.DirX, info.DirY).normalized;
        else
            _lastAimDir = Vector2.right;
    }

    protected override void Start()
    {
        base.Start();
        BindCamera();
        _input = gameObject.GetOrAddComponent<InputComponent>();
        BindMoveEvents();
        BindAttackEvents();
    }

    void BindCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[MyPlayerObject] Main Camera not found.");
            return;
        }

        CameraController cc = cam.gameObject.GetOrAddComponent<CameraController>();
        cc.Target = this;
    }

    void BindMoveEvents()
    {
        if (Move == null)
            return;

        Move.ArrivedAtDestination -= OnMoveArrivedAtDestination;
        Move.ArrivedAtDestination += OnMoveArrivedAtDestination;
    }

    void BindAttackEvents()
    {
        if (_input == null)
            return;

        _input.OnAttackAction -= OnAttackInput;
        _input.OnAttackAction += OnAttackInput;
    }

    void OnDestroy()
    {
        if (Move != null)
            Move.ArrivedAtDestination -= OnMoveArrivedAtDestination;

        if (_input != null)
            _input.OnAttackAction -= OnAttackInput;
    }

    void OnAttackInput()
    {
        if (_attackCooldown > 0f)
            return;

        _attackCooldown = AttackCooldownTime;
        State?.AddState(CreatureState.Attack);
        Managers.Network.SendAttack(_lastAimDir.x, _lastAimDir.y);
    }

    protected override void Update()
    {
        UpdateInput();
        UpdateState();
    }

    void UpdateState()
    {
        if (State != null && State.HasState(CreatureState.Move))
            UpdateMove();

        if (State != null && State.HasState(CreatureState.Attack))
            UpdateAttack();

        if (State != null && State.HasState(CreatureState.Skill))
            UpdateSkill();

        UpdateSendMovePacket();
    }

    void UpdateMove()
    {
        float moveSpeed = Stat != null ? Stat.MoveSpeed : 0f;
        AddWorldDelta(_moveDir * (moveSpeed * Time.deltaTime));
    }

    void UpdateAttack()
    {
        _attackCooldown -= Time.deltaTime;
        if (_attackCooldown <= 0f)
        {
            _attackCooldown = 0f;
            State?.RemoveState(CreatureState.Attack);
        }
    }

    void UpdateSkill()
    {
    }

    void OnMoveArrivedAtDestination()
    {
        if (Interpolate == false)
            return;

        Interpolate = false;
        SyncDestinationToCurrent();
    }

    void UpdateInput()
    {
        if (_input == null)
            return;

        Vector2 dir = _input.MoveDirection;
        if (dir.sqrMagnitude > 1f)
            dir = dir.normalized;

        if (dir.sqrMagnitude < MoveDeadzoneSqr)
            dir = Vector2.zero;

        _moveDir = dir;
        if (_moveDir.sqrMagnitude > 0.0001f)
            _lastAimDir = _moveDir.normalized;

        UpdateMoveStateFromInput(dir.sqrMagnitude > 0.0001f);

        float faceDirX = _moveDir.sqrMagnitude > 0.0001f ? _moveDir.x : _lastAimDir.x;
        ApplyFacing(faceDirX);
    }

    void UpdateMoveStateFromInput(bool isMoving)
    {
        if (State == null)
            return;

        if (isMoving)
            State.AddState(CreatureState.Move);
        else
            State.RemoveState(CreatureState.Move);
    }

    public override void ApplyDestPosition(Vector2 pos)
    {
        Vector3 serverPos = new Vector3(pos.x, pos.y, transform.position.z);
        if (Vector3.Distance(transform.position, serverPos) <= _correctionThreshold)
            return;

        Interpolate = true;
        SetDestination(pos.x, pos.y);
    }

    public override void ApplyStateFlags(int stateFlags)
    {
        // 내 캐릭 상태는 로컬 입력·스킬에서 결정.
    }

    public override void ApplyMoveDirection(float dirX, float dirY)
    {
        // 내 캐릭 방향은 로컬 입력(UpdateInput)에서 결정.
    }

    public override void ApplyDie()
    {
        CancelTransientStateInvokes();
        State?.ApplyNetworkFlags(CreatureStateUtil.ToBitMask(CreatureState.Dead));
    }

    void UpdateSendMovePacket()
    {
        if (State == null)
            return;

        int stateFlags = State.StateFlags;
        Vector3 pos = transform.position;

        bool stateChanged = stateFlags != _lastSentStateFlags;
        bool dirChanged =
            Mathf.Approximately(_lastAimDir.x, _lastSentDirX) == false ||
            Mathf.Approximately(_lastAimDir.y, _lastSentDirY) == false;
        bool posChanged =
            Mathf.Approximately(pos.x, _lastSentPosX) == false ||
            Mathf.Approximately(pos.y, _lastSentPosY) == false;

        if (stateChanged == false && dirChanged == false && posChanged == false)
            return;

        _syncTimer += Time.deltaTime;
        if (stateChanged == false && dirChanged == false && _syncTimer < _syncInterval)
            return;

        _syncTimer = 0f;
        SendMoveSync(pos.x, pos.y, _lastAimDir.x, _lastAimDir.y, stateFlags);
    }

    void SendMoveSync(float posX, float posY, float dirX, float dirY, int stateFlags)
    {
        Managers.Network.SendMoveSync(posX, posY, dirX, dirY, stateFlags);

        _lastSentStateFlags = stateFlags;
        _lastSentPosX = posX;
        _lastSentPosY = posY;
        _lastSentDirX = dirX;
        _lastSentDirY = dirY;
    }
}
