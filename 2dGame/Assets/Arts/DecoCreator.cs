using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoCreator : MonoBehaviour
{
    public Transform[] prefabs;
    public float width;
    public float height;
    public int num;

    void Start()
    {
        for (int i = 0; i < num; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-width, width), Random.Range(-height, height), 0f);
            Vector3 pos = this.transform.position + offset;
            Transform prefab = Instantiate(prefabs[Random.Range(0, prefabs.Length)], this.transform).transform;
            prefab.position = pos;
        }

    }
}
