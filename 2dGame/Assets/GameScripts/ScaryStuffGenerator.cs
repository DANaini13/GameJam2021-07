using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ScaryStuffGenerator : MonoBehaviour
{
    public string btn_needs_to_tap;
    private GameObject tap_btn_prefab;
    public string btn_cannot_tap;
    private GameObject stay_btn_prefab;
    public float tap_btn_ratio;
    public float stay_btn_ratio;
    public float empty_ratio;
    public int san_value_changed_when_wrong;

    public void Awake()
    {
        tap_btn_prefab = Resources.Load("Prefabs/ScaryBtns/" + btn_needs_to_tap) as GameObject;
        stay_btn_prefab = Resources.Load("Prefabs/ScaryBtns/" + btn_cannot_tap) as GameObject;
        canvas = GameObject.Find("Canvas").transform;
        if (tap_btn_prefab == null || stay_btn_prefab == null)
        {
            Debug.LogError("prefab 名字配置错了！");
        }
    }

    private Transform canvas;
    private UISceneFollower current_btn = null;
    public void CheckGenerate()
    {
        int tap_btn_index = Mathf.CeilToInt(100 * tap_btn_ratio);
        int stay_btn_index = Mathf.CeilToInt(100 * stay_btn_ratio) + tap_btn_index;
        int empty_index = Mathf.CeilToInt(100 * empty_ratio) + stay_btn_index + tap_btn_index;
        int generating_index = Random.Range(0, 100);
        if (generating_index < tap_btn_index)
        {
            Debug.Log("tap");
            var fellower = Instantiate(tap_btn_prefab, canvas).GetComponent<UISceneFollower>();
            fellower.GetComponent<UIButton>().on_click = OnClickTapBtn;
            fellower.fellowing_obj = transform;
            fellower.offset = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f), 3f);
            current_btn = fellower;
            fellower.auto_destory_after = 3;
            Invoke("OnTapBtnDidnotClick", 4.0f);
        }
        else if (generating_index < stay_btn_index)
        {
            Debug.Log("stay");
            var fellower = Instantiate(stay_btn_prefab, canvas).GetComponent<UISceneFollower>();
            fellower.GetComponent<UIButton>().on_click = OnClickStayBtn;
            fellower.fellowing_obj = transform;
            fellower.offset = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f), 2f);
            fellower.auto_destory_after = 3;
            current_btn = fellower;
        }
        Debug.Log("not");
        // 啥也不生成
        return;
    }

    void OnTapBtnDidnotClick()
    {
        if (tap_btn_clicked)
        {
            tap_btn_clicked = false;
            return;
        }
        ResultWrong();
    }

    private bool tap_btn_clicked = false;

    void OnClickTapBtn()
    {
        PlayerControl._instance.ClickBtn(this.transform.position);
        tap_btn_clicked = true;
        //正确点击恢复一定比例的san值
        SanManager._instance.san_value -= Mathf.RoundToInt(san_value_changed_when_wrong * 0.5f);
        if (SanManager._instance.san_value >= 100)
            SanManager._instance.san_value = 100;
        if (current_btn)
            current_btn.FadeOutAfter(0.01f);
    }

    void OnClickStayBtn()
    {
        PlayerControl._instance.ClickBtn(this.transform.position);
        ResultWrong();
        if (current_btn)
            current_btn.FadeOutAfter(0.01f);
    }

    public void ResultWrong()
    {
        var arg = new CRCustomArgs.ModifySanValueArg();
        arg.diff = san_value_changed_when_wrong;
        Debug.Log(arg.diff);
        CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.MODIFY_SAN_VALUE, arg);
        CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.ON_ANSWER_WRONG, null);
        //玩家点击失败，会强制开始呼吸
        PlayerControl._instance.StartBreath(true);
    }

}
