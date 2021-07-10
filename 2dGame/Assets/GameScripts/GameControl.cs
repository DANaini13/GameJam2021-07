using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    public GameObject game_over;
    
    private void Awake()
    {
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.ON_GAME_OVER, this, OnGameOverEvent);
    }

    private void OnDestroy()
    {
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.ON_GAME_OVER, this);
    }

    public void OnGameOverEvent(object arg)
    {
        game_over.SetActive(true);
    }
}
