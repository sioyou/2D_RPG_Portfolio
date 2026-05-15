using UnityEngine;
using UnityEngine.EventSystems;

public class TitleScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        SceneType = Define.EScene.TitleScene;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        Application.runInBackground = true;

        PreserveEventSystem();
    }

    protected override void Start()
    {
        base.Start();

        Managers.Init();

        Managers.Resource.LoadAllAsync<Object>("Preload", OnPreloadComplete);
    }

    void PreserveEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("[TitleScene] EventSystem not found in scene. Add EventSystem to TitleScene.");
            return;
        }

        DontDestroyOnLoad(eventSystem.gameObject);
    }

    void OnPreloadComplete()
    {
        Managers.Network.OnGameServerConnected += HandleGameServerConnected;
        Managers.Network.OnGameServerConnectFailed += HandleGameServerConnectFailed;

        UI_TitleScene ui = FindFirstObjectByType<UI_TitleScene>();
        if (ui != null)
        {
            Managers.UI.SceneUI = ui;
            ui.BeginFlow();
            return;
        }

        ui = Managers.UI.ShowSceneUI<UI_TitleScene>();
        if (ui != null)
            ui.BeginFlow();
        else
            Managers.Network.ConnectGameServer();
    }

    void HandleGameServerConnected()
    {
        Managers.Scene.LoadScene(Define.EScene.GameScene);
    }

    void HandleGameServerConnectFailed()
    {
        Debug.LogWarning("[TitleScene] Failed to connect to game server.");
    }

    void OnDestroy()
    {
        if (Managers.Initialized == false)
            return;

        Managers.Network.OnGameServerConnected -= HandleGameServerConnected;
        Managers.Network.OnGameServerConnectFailed -= HandleGameServerConnectFailed;
    }

    public override void Clear()
    {
        if (Managers.Initialized == false)
            return;

        Managers.Network.OnGameServerConnected -= HandleGameServerConnected;
        Managers.Network.OnGameServerConnectFailed -= HandleGameServerConnectFailed;
    }
}
