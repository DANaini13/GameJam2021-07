using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomTextGenerator1 : MonoBehaviour
{

    private GameObject prefab;
    public float generate_interval;
    public Transform gossip_points_holder;

    private void Awake()
    {
        prefab = Resources.Load("Prefabs/random_text") as GameObject;
    }

    private float last_generate_time = 0;
    void Update()
    {
        if (Time.fixedTime - last_generate_time < generate_interval)
            return;
        last_generate_time = Time.fixedTime;
        var text = Instantiate(prefab, transform);
        var index = Random.Range(0, gossip_points_holder.childCount);
        var position = transform.GetChild(index).position;
        text.transform.position = position;
    }
}
