using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RandomText : MonoBehaviour
{
    private Text text;
    
    private void Start()
    {
        text = GetComponent<Text>();
        var texts = new[]
        {
            "这是哪里啊?",
            "好黑…",
            "什么声音!",
            "打雷的声音好大...",
            "那天, 教室剧烈的晃动, 老师却说应该安心听课",
            "大彭那小子可真厉害, 刚开始地震的时候他第一个窜出了教室",
            "我们三个人在石板下躺了两天两夜",
            "我想好好呼吸",
            "我也想喝自己的尿, 可是我被石板压着, 我够不到",
            "是不是过了今天就能得救了",
            "小艾还好吗? 这个时候她肯定在睡懒觉吧, 真羡慕她啊",
            "为什么我一直走不出去这个教学楼, 我不是得救了吗",
            "难道我还没得救?",
        };

        int length = texts.Length;
        string str = texts[Random.Range(0, length)];
        text.text = str;
        text.DOFade(1, 0.5f);
        Invoke("FadeOut", 3);
    }

    void FadeOut()
    {
        text.DOFade(0, 0.5f);
        GameObject.Destroy(gameObject, 0.5f);
    }

}
