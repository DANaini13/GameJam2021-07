using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoChangeOnStart : MonoBehaviour
{
    public Sprite[] spList;
    void Start()
    {
        this.transform.GetComponent<SpriteRenderer>().sprite = spList[Random.Range(0, spList.Length)];
    }
}
