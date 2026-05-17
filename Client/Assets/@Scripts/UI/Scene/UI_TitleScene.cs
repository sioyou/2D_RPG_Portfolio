using DG.Tweening;
using TMPro;
using UnityEngine;

public class UI_TitleScene : UI_Scene
{
    enum Texts
    {
        LoadingText
    }

    TMP_Text _loadingText;

    protected override void Awake()
    {
        base.Awake();
        BindTexts(typeof(Texts));

        _loadingText = GetText((int)Texts.LoadingText);
    }

    void OnEnable()
    {
        Managers.Network.OnGameServerConnected += HandleGameServerConnected;
        _loadingText.text = "";

        _loadingText.DOText("Loading...", 2f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    void OnDisable()
    {
        if (Managers.Initialized == false)
            return;

        Managers.Network.OnGameServerConnected -= HandleGameServerConnected;
    }

    void OnDestroy()
    {
        _loadingText.DOKill();
    }

    void HandleGameServerConnected()
    {
        _loadingText.gameObject.SetActive(false);
        Managers.UI.ShowPopupUI<UI_LoginPopup>();
    }

    public void BeginFlow()
    {
        Managers.Data.Init();

        Debug.Log("[UI_TitleScene] Connecting to server...");
        Managers.Network.ConnectGameServer();
    }
}
