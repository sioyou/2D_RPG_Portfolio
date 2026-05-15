using UnityEngine;

public class TitleScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        SceneType = Define.EScene.TitleScene;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    protected override void Start()
    {
        base.Start();

        Managers.Init();
        Managers.UI.ShowSceneUI<UI_TitleScene>();
    }

    public override void Clear()
    {
    }
}
