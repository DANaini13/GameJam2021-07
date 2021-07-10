using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaChangerBySanValue : MonoBehaviour
{
    private SpriteRenderer[] sprite_renders;
    
    private void Awake()
    {
        sprite_renders = GetComponentsInChildren<SpriteRenderer>();
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.ON_SAN_VALUE_CHANGED, this, OnSanValueChangedEvent);
    }

    private void Start()
    {
        foreach (var sprite_render in sprite_renders)
        {
            sprite_render.color = new Color(1, 1, 1, 0);
        }
    }

    void OnSanValueChangedEvent(object arg)
    {
        var typed_arg = (CRCustomArgs.OnSanValueChangedArg) arg;
        // 通过san值决定透明度
        float alpha = 1.0f - typed_arg.san;
        foreach (var sprite_render in sprite_renders)
        {
            sprite_render.color = new Color(1, 1, 1, alpha);
        }
    }

    private void OnDestroy()
    {
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.ON_SAN_VALUE_CHANGED, this);
    }
}
