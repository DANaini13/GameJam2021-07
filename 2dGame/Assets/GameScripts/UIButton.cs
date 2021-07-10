using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    public delegate void OnClick();
    public OnClick on_click = null;

    public void OnBtnClick()
    {
        if (on_click == null) return;
        on_click();
    }
}
