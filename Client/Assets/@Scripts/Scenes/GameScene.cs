using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    bool _leaveSent;

    protected override void Awake()
    {
        base.Awake();

        Debug.Log("@>> GameScene Init()");
        SceneType = EScene.GameScene;
    }

    protected override void Start()
    {
        base.Start();
        Managers.Init();
        Managers.Network.SendEnterGame();
    }

    public override void Clear()
    {
        Managers.Object.Clear();
    }

    void OnApplicationQuit()
    {
        LeaveGame();
    }

    void OnDestroy()
    {
        if (SceneType != EScene.GameScene)
            return;

        LeaveGame();
    }

    void LeaveGame()
    {
        if (_leaveSent)
            return;

        _leaveSent = true;
        Managers.Network.LeaveGameAndDisconnect();
    }
}
