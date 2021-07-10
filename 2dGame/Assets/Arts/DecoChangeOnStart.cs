using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoChangeOnStart : MonoBehaviour
{
    public bool use_changeOnStart;
    public Sprite[] spList;
    int spIndex;

    void Start()
    {
        if (use_changeOnStart) RandomSetSp();
    }

    public int RandomSetSp()
    {
        spIndex = Random.Range(0, spList.Length);
        Refresh();
        return spIndex;
    }

    public void SetSp(int index)
    {
        if (index >= spList.Length || index < 0) index = 0;
        spIndex = index;
        Refresh();
    }

    void Refresh()
    {
        if(spList[spIndex]==null)Destroy(this.transform.parent.gameObject);
        this.transform.GetComponent<SpriteRenderer>().sprite = spList[spIndex];
    }
}
