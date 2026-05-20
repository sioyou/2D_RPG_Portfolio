using Protocol;
using UnityEngine;

public class MyPlayerObject : PlayerObject
{
    const float MoveDeadzoneSqr = 0.02f * 0.02f;

    [SerializeField] float _correctionThreshold = 0.5f;
    [SerializeField] float _syncInterval = 0.1f;

    InputComponent _input;
    Vector2 _moveDir;
    float _syncTimer;

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
    }

    protected override void Start()
    {
        base.Start();
        BindCamera();
        _input = gameObject.GetOrAddComponent<InputComponent>();
        BindMoveEvents();
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

    void OnDestroy()
    {
        if (Move != null)
            Move.ArrivedAtDestination -= OnMoveArrivedAtDestination;
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
        AddWorldDelta(_moveDir * (MoveSpeed * Time.deltaTime));
    }

    void UpdateAttack()
    {
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
        UpdateMoveStateFromInput(dir.sqrMagnitude > 0.0001f);
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

    void UpdateSendMovePacket()
    {
        if (State == null)
            return;

        int stateFlags = State.StateFlags;
        if (stateFlags == (int)CreatureState.None && _lastSentStateFlags == (int)CreatureState.None)
            return;

        Vector3 pos = transform.position;

        bool stateChanged = stateFlags != _lastSentStateFlags;
        bool dirChanged =
            Mathf.Approximately(_moveDir.x, _lastSentDirX) == false ||
            Mathf.Approximately(_moveDir.y, _lastSentDirY) == false;
        bool posChanged =
            Mathf.Approximately(pos.x, _lastSentPosX) == false ||
            Mathf.Approximately(pos.y, _lastSentPosY) == false;

        if (stateChanged == false && dirChanged == false && posChanged == false)
            return;

        _syncTimer += Time.deltaTime;
        if (stateChanged == false && _syncTimer < _syncInterval)
            return;

        _syncTimer = 0f;
        SendMoveSync(pos.x, pos.y, _moveDir.x, _moveDir.y, stateFlags);
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
