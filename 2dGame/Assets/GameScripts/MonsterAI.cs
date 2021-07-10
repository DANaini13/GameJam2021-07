using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterAI : MonoBehaviour
{
    public float stay_interval = 5;
    public float play_limit_distance = 10;
    public float cheasing_distance = 20;
    public float walking_speed = 1;
    public float cheasing_interval;
    public Transform stay_points_holder;
    public Transform player;
    
    private List<Vector3> stay_points = new List<Vector3>();

    private void Awake()
    {
        int child_count = stay_points_holder.childCount;
        for (int i = 0; i < child_count; ++i)
        {
            stay_points.Add(stay_points_holder.GetChild(i).position);
        }
    }

    void Update()
    {
        CheckCheasingPlayer();
        UpdateStayPoints();
    }

    public bool is_cheasing = false;
    public bool cheasing_left = false;

    void CheckCheasingPlayer()
    {
        if (is_cheasing)
        {
            Cheasing();
            return;
        }
        // 1. 判断跟玩家是否在同层
        if (Mathf.Abs(transform.position.y - player.position.y) > 5) return;
        // 2. 距离是否小于20
        var distance = Vector3.Distance(transform.position, player.position);
        if (distance > 20) return;
        StartCheasing();
    }

    void StartCheasing()
    {
        is_cheasing = true;
        // 1. 判断玩家在左侧还是右侧，向那个方向移动
        cheasing_left = true;
        if (player.position.x >= transform.position.x) cheasing_left = false;
        Invoke("StopCheasing", cheasing_interval);
    }

    void Cheasing()
    {
        if (cheasing_left) transform.position -= new Vector3(walking_speed * Time.deltaTime, 0, 0);
        else transform.position -= new Vector3(-walking_speed * Time.deltaTime, 0, 0);
    }

    void StopCheasing()
    {
        is_cheasing = false;
        UpdateStayPoints();
    }

    private float start_stay_time = 0;
    void UpdateStayPoints()
    {
        if (is_cheasing) return;
        if (Time.fixedTime - start_stay_time < stay_interval)
            return;
        start_stay_time = Time.fixedTime;
        // 找个新的目的地
        bool found = false;
        int count = stay_points.Count;
        while (!found)
        {
            var position = stay_points[Random.Range(0, count)];
            if (Vector3.Distance(position, player.position) < play_limit_distance)
                continue;
            found = true;
            transform.position = position;
        }
    }
}
