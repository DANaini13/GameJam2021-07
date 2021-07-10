using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CRCustomArgs
{
    public class OnSanValueChangedArg
    {
        public int san_value;
        public float san;
    }

    public class GrabGatePairPositionArg
    {
        public int pair_key;
        public int gate_tag;
        public Nullable<Vector3> reslut;
    }

    public class TransPlayerToPositionArg
    {
        public Vector3 position;
    }

    public class ModifySanValueArg
    {
        public int diff;
    }
}

public class CRCustomEvents
{
    static public int ON_SAN_VALUE_CHANGED = 1;
    static public int GRAB_GATE_PAIR_POSITION = 2;
    static public int TRANS_PLAYER_TO_POSITION = 3;
    static public int MODIFY_SAN_VALUE = 4;
    static public int ON_GAME_OVER = 5;
    static public int ON_ANSWER_WRONG = 6;
}

public class CREventSystem : MonoBehaviour
{
    private static CREventSystem instance = null; 
    static public CREventSystem Instance
    {
        get
        {
            if (destroied)
            {
                return null;
            }
            if (instance == null)
            {
                instance = (CREventSystem)FindObjectOfType(typeof(CREventSystem));
                if (instance == null)
                {
                    var instanceObj = new GameObject();
                    instanceObj.name = "CREventSystem";
                    instance = instanceObj.AddComponent<CREventSystem>();
                    DontDestroyOnLoad(instance);
                }
            }
            return instance;
        }
    }

    static public bool IsEventSystemDestoried()
    {
        return destroied;
    }

    public delegate void OnCREventCall(object arg);
    private Dictionary<int, Dictionary<MonoBehaviour, OnCREventCall>> callback_map = new Dictionary<int, Dictionary<MonoBehaviour, OnCREventCall>>();

    public void ListenCustomeEventByKey(int key, MonoBehaviour listener, OnCREventCall callback)
    {
        if (!callback_map.ContainsKey(key))
            callback_map.Add(key, new Dictionary<MonoBehaviour, OnCREventCall>());
        if (!callback_map[key].ContainsKey(listener))
            callback_map[key].Add(listener, null);
        callback_map[key][listener] = callback;
    }

    private void eraseCustomeEventByKey(int key, MonoBehaviour listener)
    {
        if (!callback_map.ContainsKey(key))
            return;
        if (!callback_map[key].ContainsKey(listener))
            return;
        callback_map[key].Remove(listener);
        if (callback_map[key].Count > 0) return;
        callback_map.Remove(key);
    }
    
    static public void EraseCustomeEventByKey(int key, MonoBehaviour listener)
    {
        if (instance == null) return;
        instance.eraseCustomeEventByKey(key, listener);
    }

    public void DispatchCREventByKey(int key, object arg)
    {
         if (!callback_map.ContainsKey(key))
             return;
         foreach (var listener in callback_map[key])
         {
             if(listener.Value == null) continue;
             listener.Value(arg);
         }
    }

    private static bool destroied = false;
    public void OnDestroy()
    {
        destroied = true;
    }
}
