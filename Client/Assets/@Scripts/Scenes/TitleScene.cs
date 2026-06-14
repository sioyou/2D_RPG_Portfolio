using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

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

        Managers.Resource.LoadAllAsync<Object>(AddressableLabels.Preload, false, () =>
        {
            Managers.Resource.LoadAllAsync<Object>(AddressableLabels.ScenePreload(Define.EScene.TitleScene), true, OnPreloadComplete);
        });
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
        Managers.Network.OnLoginSuccess += HandleLoginSuccess;
        Managers.Network.OnLoginFailed += HandleLoginFailed;
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
            Debug.LogError("[TitleScene] UI_TitleScene not found.");
    }

    void HandleLoginSuccess()
    {
        Managers.Scene.LoadScene(Define.EScene.GameScene);
    }

    void HandleLoginFailed()
    {
        Debug.LogWarning("[TitleScene] Login failed.");
    }

    void HandleGameServerConnectFailed()
    {
        Debug.LogWarning("[TitleScene] Failed to connect to game server.");
    }

    void OnDestroy()
    {
        if (Managers.Initialized == false)
            return;

        UnsubscribeNetworkEvents();
    }

    public override void Clear()
    {
        if (Managers.Initialized == false)
            return;

        UnsubscribeNetworkEvents();
    }

    void UnsubscribeNetworkEvents()
    {
        Managers.Network.OnLoginSuccess -= HandleLoginSuccess;
        Managers.Network.OnLoginFailed -= HandleLoginFailed;
        Managers.Network.OnGameServerConnectFailed -= HandleGameServerConnectFailed;
    }
}
