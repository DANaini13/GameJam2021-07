using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public GameObject game_over;
    public Image game_win;
    public Transform player;
    public Transform monster;
    public GameObject hand;

    private void Awake()
    {
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.ON_GAME_OVER, this, OnGameOverEvent);
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.ON_GAME_WIN, this, OnGameWin);
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.M))
    //         OnGameOverEvent(this);
    // }

    private void OnDestroy()
    {
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.ON_GAME_OVER, this);
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.ON_GAME_WIN, this);
    }

    public void OnGameOverEvent(object arg)
    {
        hand.transform.localScale = Vector3.zero;
        hand.SetActive(true);
        hand.transform.DOScale(Vector3.one * 5, 0.3f).SetEase(Ease.InQuint);
        Invoke("Over", 0.3f);
    }

    private void Over()
    {
        game_over.SetActive(true);
    }

    public void OnGameWin(object arg)
    {

        player.transform.position = new Vector3(0, -2.81f, 0);
        player.GetComponent<SanManager>().san_value = 100;
        monster.transform.position = new Vector3(127.7f, 19.4f, 0);
        var sequence = DOTween.Sequence();
        sequence.Append(game_win.DOFade(1, 1.0f)).onStepComplete = () =>
        {
        };
        sequence.Append(game_win.DOFade(0, 1.0f));
        sequence.onComplete = () =>
        {
        };
        sequence.Play();
    }

    public void OnGameWin()
    {
        OnGameWin(null);
    }
}
