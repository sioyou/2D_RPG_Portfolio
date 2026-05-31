using UnityEngine;
using UnityEngine.UI;

public class UI_WorldHpBar : UI_Base
{
    private Slider _slider;

    protected override void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.value = 1;
    }

    public void SetHpValue(float value)
    {
        _slider.value = value;
    }
}
