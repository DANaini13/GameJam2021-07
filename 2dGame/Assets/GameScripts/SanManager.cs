using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;

public class SanManager : MonoBehaviour
{
    public ProgressBar san_bar;
    [Range(0, 100)]
    public int san_value = 100;
    [Header("san 值下降间隔")]
    public float san_value_mines_interval = 1.0f;
    [Header("san 值下降的量")]
    public int san_value_mines_amount = 1;
    private int last_san_value = 100;
    private float san = 0;
    public Volume volume;
    public Light2D light;

    private void Awake()
    {
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.MODIFY_SAN_VALUE, this, OnModifySanValueEvent);
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.ON_ANSWER_WRONG, this, OnAnswerWrongEvent);
    }

    private void OnDestroy()
    {
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.MODIFY_SAN_VALUE, this);
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.ON_ANSWER_WRONG, this);
    }

    void Start()
    {
        Update();
    }

    void Update()
    {
        SanValueUpdate();
        CheckSanValueChanged();
    }

    private float last_update_san_time = 0;
    private void SanValueUpdate()
    {
        if (Time.fixedTime - last_update_san_time < san_value_mines_interval) return;
        san_value -= san_value_mines_amount;
        last_update_san_time = Time.fixedTime;
    }

    private void CheckSanValueChanged()
    {
        if (last_san_value == san_value) return;
        last_san_value = san_value;
        // 派发事件
        san = (float)san_value / 100.0f;
        san_bar.SetProgress(san);
        var arg = new CRCustomArgs.OnSanValueChangedArg();
        arg.san = san;
        arg.san_value = san_value;
        CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.ON_SAN_VALUE_CHANGED, arg);
    }

    public void OnModifySanValueEvent(object arg)
    {
        var typed_arg = (CRCustomArgs.ModifySanValueArg)arg;
        san_value += typed_arg.diff;
        if (san_value <= 0)
        {
            san_value = 0;
            CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.ON_GAME_OVER, null);
            return;
        }

        if (san_value > 100) san_value = 100;
    }

    public void OnAnswerWrongEvent(object arg)
    {
        //点击错误则破坏电筒
        PlayerControl._instance.BreakFlashlight();
        volume.enabled = true;
        volume.weight = 1f;
        // light.intensity = 1;
        // Invoke("disable_volume", 0.3f);
        StartCoroutine("DisableVolume");
    }

    IEnumerator DisableVolume()
    {
        while (true)
        {
            yield return 0;
            volume.weight -= 0.01f;
            if (volume.weight <= 0)
                break;
        }
        volume.weight = 0f;
        volume.enabled = false;
    }

    private void disable_volume()
    {
        volume.enabled = false;
        // light.intensity = 0.01f;
    }
}
