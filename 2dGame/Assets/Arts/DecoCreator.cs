using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoCreator : MonoBehaviour
{
    public Transform[] prefabs;
    public float width;
    public float height;
    public float angle;
    public int num;
    public float chance = 100f;
    public float minSize = 1f;
    public float maxSize = 1f;

    public bool use_average;
    public bool use_samePrefabIndex;

    void Start()
    {
        int spIndex = 0;
        for (int i = 0; i < num; i++)
        {
            if (Random.Range(0, 100) > chance) continue;

            Vector3 offset = new Vector3(Random.Range(-width, width), Random.Range(-height, height), 0f);
            if (use_average) offset = new Vector3(-width + width * 2 / (num - 1) * i, -height + height * 2 / (num - 1), 0f);

            Vector3 pos = this.transform.position + offset;
            Transform prefab = Instantiate(prefabs[Random.Range(0, prefabs.Length)], this.transform).transform;
            prefab.position = pos;
            prefab.Rotate(Vector3.forward * Random.Range(-angle, angle));
            prefab.localScale = Vector3.one * Random.Range(minSize, maxSize);

            if (use_samePrefabIndex)
            {
                if (i == 0) spIndex = prefab.GetComponentInChildren<DecoChangeOnStart>().RandomSetSp();
                else prefab.GetComponentInChildren<DecoChangeOnStart>().SetSp(spIndex);
                Debug.Log(spIndex);
            }
            else
                prefab.GetComponentInChildren<DecoChangeOnStart>().RandomSetSp();
        }

    }
}
