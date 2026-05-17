using UnityEngine;

public class UI_Popup : UI_Base
{
    public Canvas UICanvas;

    protected override void Awake()
    {
        base.Awake();

        UICanvas = Managers.UI.SetCanvas(gameObject);
    }
    
    public virtual void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI(this);
    }

}
