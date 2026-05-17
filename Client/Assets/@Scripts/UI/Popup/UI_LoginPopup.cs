using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginPopup : UI_Popup
{
    [SerializeField]
    private Button _loginButton;
    [SerializeField]
    private TMP_InputField _inputField;

    protected override void Awake()
    {
        base.Awake();
        _loginButton = Utils.FindChild<Button>(gameObject, "LoginButton", true);
        _loginButton.onClick.AddListener(OnClickLoginButton);
        _inputField = Utils.FindChild<TMP_InputField>(gameObject, "IdInput", true);
    }

    void OnEnable()
    {
        _inputField.text = "";

    }

    private void OnClickLoginButton()
    {
        var id = _inputField.text;

        Managers.Network.SendLogin(id);
        _loginButton.enabled = false;
    }
}
