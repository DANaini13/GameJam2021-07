using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightIntensityChangerBySanVlaue : MonoBehaviour
{
    private Light2D[] lights;

    private void Awake()
    {
        lights = GetComponentsInChildren<Light2D>();
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.ON_SAN_VALUE_CHANGED, this, OnSanValueChangedEvent);
    }

    private void Start()
    {
        foreach (var light in lights)
        {
            light.intensity = 1;
        }
    }

    void OnSanValueChangedEvent(object arg)
    {
        var typed_arg = (CRCustomArgs.OnSanValueChangedArg) arg;
        // 通过san值决定透明度
        foreach (var light in lights)
        {
            light.intensity = typed_arg.san;
        }
    }
}
