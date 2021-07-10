using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransGate : MonoBehaviour
{
    public int pair_key;
    public int gate_tag;

    public void Trans()
    {
        var arg = new CRCustomArgs.GrabGatePairPositionArg();
        arg.pair_key = pair_key;
        arg.gate_tag = gate_tag;
        CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.GRAB_GATE_PAIR_POSITION, arg);
        if (!arg.reslut.HasValue) return;
        var position = arg.reslut.Value;
        var arg1 = new CRCustomArgs.TransPlayerToPositionArg();
        arg1.position = position;
        CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.TRANS_PLAYER_TO_POSITION, arg1);
    }
}
