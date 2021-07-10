using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransGateManager : MonoBehaviour
{
    private Dictionary<int, List<TransGate>> trans_gate_map = new Dictionary<int, List<TransGate>>();
    
    private void Awake()
    {
        int id = 0;
        var trans_gates = GetComponentsInChildren<TransGate>();
        foreach (var trans_gate in trans_gates)
        {
            trans_gate.gate_tag = id;
            ++id;
            if (!trans_gate_map.ContainsKey(trans_gate.pair_key))
                trans_gate_map.Add(trans_gate.pair_key, new List<TransGate>());
            if (trans_gate_map[trans_gate.pair_key].Count >= 2)
            {
                Debug.LogError("传送门配置出错， 含有三个或三个以上的传送门使用相同的key. 传送门pair_key: " + trans_gate.pair_key);
                continue;
            }
            trans_gate_map[trans_gate.pair_key].Add(trans_gate);
        }
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.GRAB_GATE_PAIR_POSITION, this, GetTransPairPositionEvent);
    }

    private void OnDestroy()
    {
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.GRAB_GATE_PAIR_POSITION, this);
    }

    void GetTransPairPositionEvent(object arg)
    {
        var typed_arg = (CRCustomArgs.GrabGatePairPositionArg) arg;
        typed_arg.reslut = GetTransPairPosition(typed_arg.pair_key, typed_arg.gate_tag);
    }

    public Nullable<Vector3> GetTransPairPosition(int pair_key, int gate_tag)
    {
        if (!trans_gate_map.ContainsKey(pair_key)) return null;
        var list = trans_gate_map[pair_key];
        if (list.Count < 2) return null;
        if (list[0].gate_tag == gate_tag) return list[1].transform.position;
        else return list[0].transform.position;
    }
}
