using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UISceneFollower : MonoBehaviour
{
    public int tag;
    public int auto_destory_after = 0;
    public Transform fellowing_obj;
    public Vector3 offset = new Vector3(0, 0, 0);
    private RectTransform rect_transform;

    private bool need_auto_destory = false;
    private float start_time = 0;
    private float init_time = 0;

    void Start()
    {
        init_time = Time.fixedTime;
        FadeIn(0.15f);
        need_auto_destory = auto_destory_after > 0;
        start_time = Time.fixedTime;
        rect_transform = GetComponent<RectTransform>();
        Update();
    }

    private bool started_destory = false;
    void Update()
    {
        if (Time.fixedTime - init_time > auto_destory_after && need_auto_destory)
        {
            FadeOutAfter(1.0f);
        }
        if (rect_transform == null) return;

        if (fellowing_obj == null)
        {
            FadeOutAfter(0.15f);
            return;
        }
        var screen_position = Camera.main.WorldToScreenPoint(fellowing_obj.position + offset);
        rect_transform.position = screen_position;
    }

    void FadeIn(float seconds)
    {
        var images = GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            var origin = image.color.a;
            image.DOFade(0, 0);
            image.DOFade(origin, seconds);
        }
        var texts = GetComponentsInChildren<Text>();
        foreach (var text in texts)
        {
            var origin = text.color.a;
            text.DOFade(0, 0);
            text.DOFade(origin, seconds);
        }
    }

    public void FadeOutAfter(float seconds)
    {
        started_destory = true;
        var images = GetComponentsInChildren<Image>();
        foreach (var image in images) image.DOFade(0, seconds);
        GameObject.Destroy(gameObject, seconds);
        var texts = GetComponentsInChildren<Text>();
        foreach (var text in texts) text.DOFade(0, seconds);
    }

}
