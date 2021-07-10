using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    // Start is called before the first frame update
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
        san = (float)san_value/100.0f;
        san_bar.SetProgress(san);
        var arg = new CRCustomArgs.OnSanValueChangedArg();
        arg.san = san;
        arg.san_value = san_value;
        CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.ON_SAN_VALUE_CHANGED, arg); 
    }
}
