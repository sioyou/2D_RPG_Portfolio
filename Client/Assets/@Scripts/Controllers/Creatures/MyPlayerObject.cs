using Protocol;
using UnityEngine;
using static Define;

public class MyPlayerObject : PlayerObject
{
    const float MoveDeadzoneSqr = 0.02f * 0.02f;
    const float AttackCooldownTime = 0.5f;

    [SerializeField] float _syncInterval = MoveValidation.MaxSyncIntervalSec;

    InputComponent _input;
    Vector2 _moveDir;
    Vector2 _lastAimDir = Vector2.right;
    float _syncTimer;
    float _attackCooldown;

    Vector2 _serverPos;
    bool _hasServerPos;

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

        _serverPos = new Vector2(info.PosX, info.PosY);
        _hasServerPos = true;
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
        if (_hasServerPos)
        {
            Vector2 pos = transform.position;
            float moveSpeed = Stat != null ? Stat.MoveSpeed : 0f;
            float maxDistance = MoveValidation.CalcMaxDistance(moveSpeed, _syncInterval + MoveValidation.MaxAllowedDeltaSec);
            float cheatRejectDistance = MoveValidation.CalcCheatRejectDistance(maxDistance);

            if (Vector2.Distance(pos, _serverPos) > cheatRejectDistance)
            {
                Interpolate = false;
                SetPosition(new Vector3(_serverPos.x, _serverPos.y, transform.position.z));
                SyncDestinationToCurrent();
            }
        }

        float moveSpeedDelta = Stat != null ? Stat.MoveSpeed : 0f;
        Vector2 delta = _moveDir * (moveSpeedDelta * Time.deltaTime);
        Vector2 curPos = transform.position;

        Vector2 resolved = Managers.Map.IsReady
            ? Managers.Map.ResolveMove(curPos, curPos + delta, CREATURE_COLLISION_RADIUS)
            : curPos;

        AddWorldDelta(resolved - curPos);
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
        _serverPos = pos;
        _hasServerPos = true;

        Vector3 serverPos = new Vector3(pos.x, pos.y, transform.position.z);
        float dist = Vector3.Distance(transform.position, serverPos);
        if (dist <= 0.05f)
            return;

        if (dist <= MoveValidation.ServerSnapThreshold)
            return;

        Interpolate = false;
        SetPosition(serverPos);
        SyncDestinationToCurrent();

        _lastSentPosX = pos.x;
        _lastSentPosY = pos.y;
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

        float sendPosX = pos.x;
        float sendPosY = pos.y;
        ClampSendPosition(ref sendPosX, ref sendPosY);

        SendMoveSync(sendPosX, sendPosY, _lastAimDir.x, _lastAimDir.y, stateFlags);
    }

    void ClampSendPosition(ref float posX, ref float posY)
    {
        if (_hasServerPos == false)
            return;

        float moveSpeed = Stat != null ? Stat.MoveSpeed : 0f;
        float maxDistance = MoveValidation.CalcMaxDistance(moveSpeed, _syncInterval + MoveValidation.MaxSyncIntervalSec);

        float dx = posX - _serverPos.x;
        float dy = posY - _serverPos.y;
        float distanceSq = dx * dx + dy * dy;
        float maxDistanceSq = maxDistance * maxDistance;

        if (distanceSq > maxDistanceSq && distanceSq > 0.0001f)
        {
            float distance = Mathf.Sqrt(distanceSq);
            float ratio = maxDistance / distance;
            posX = _serverPos.x + dx * ratio;
            posY = _serverPos.y + dy * ratio;
        }

        if (Managers.Map.IsReady)
        {
            Vector2 resolved = Managers.Map.ResolveMove(_serverPos, new Vector2(posX, posY), CREATURE_COLLISION_RADIUS);
            posX = resolved.x;
            posY = resolved.y;
        }
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
