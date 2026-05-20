using Protocol;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class MyPlayerObject : PlayerObject
{
    const float MoveDeadzoneSqr = 0.02f * 0.02f;

    [SerializeField] float _correctionThreshold = 0.5f;
    [SerializeField] float _syncInterval = 0.1f;

    InputComponent _input;
    Vector2 _moveDir;
    float _syncTimer;

    protected override void OnInfoSet(ObjectInfo info)
    {
        base.OnInfoSet(info);
        Managers.Game.BindMyHero(this);
    }

    protected override void Start()
    {
        base.Start();
        BindCamera();
        _input = gameObject.GetOrAddComponent<InputComponent>();
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

    protected override void Update()
    {
        UpdateInput();
        UpdateLocalMove();
        UpdateSendMovePacket();
    }

    void LateUpdate()
    {
        EndCorrectionIfArrived();
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
    }

    void UpdateLocalMove()
    {
        if (_moveDir.sqrMagnitude <= 0.0001f)
            return;

        AddWorldDelta(_moveDir * (MoveSpeed * Time.deltaTime));
    }

    void EndCorrectionIfArrived()
    {
        if (Interpolate == false)
            return;

        float arriveSqr = Move != null ? Move.ArriveSqrThreshold : 0.0004f;
        if (SqrDistanceToDestination > arriveSqr)
            return;

        Interpolate = false;
        SyncDestinationToCurrent();
    }

    public override void ApplyDestPosition(Vector2 pos)
    {
        Vector3 serverPos = new Vector3(pos.x, pos.y, transform.position.z);
        if (Vector3.Distance(transform.position, serverPos) <= _correctionThreshold)
            return;

        Interpolate = true;
        SetDestination(pos.x, pos.y);
    }

    void UpdateSendMovePacket()
    {
        _syncTimer += Time.deltaTime;
        if (_syncTimer < _syncInterval)
            return;

        _syncTimer = 0f;

        Vector3 pos = transform.position;
        Managers.Network.SendMoveSync(pos.x, pos.y, _moveDir.x, _moveDir.y);
    }
}
