using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterAI : MonoBehaviour
{
    public static MonsterAI _instance;
    public float stay_interval = 10;
    public float stay_distance = 20f;
    public float play_limit_distance = 10;
    public float cheasing_distance = 9;
    public float walking_speed = 1;
    public float walking_speed_change = 0.5f;
    private float walking_speed_modify;
    public float cheasing_interval;
    public Transform stay_points_holder;
    public Transform player;
    private Animator anim;
    private CapsuleCollider2D collid;
    private Rigidbody2D rigidb;

    private List<Vector3> stay_points = new List<Vector3>();

    private void Awake()
    {
        _instance = this;
        collid = this.GetComponent<CapsuleCollider2D>();
        rigidb = this.GetComponent<Rigidbody2D>();
        anim = this.transform.GetComponentInChildren<Animator>();
        int child_count = stay_points_holder.childCount;
        for (int i = 0; i < child_count; ++i)
        {
            stay_points.Add(stay_points_holder.GetChild(i).position);
        }
    }

    void Update()
    {
        CheckCheasingPlayer();
        Haning();
        UpdateStayPoints();
        UpdateCollider();
        Shake();
    }

    public bool is_cheasing = false;
    public bool cheasing_left = false;

    public float haning_speed_multiplier = 0.5f;
    public float haning_Update_timer = 2f;
    private float haning_timer;
    void Haning()
    {
        if (is_cheasing) return;
        haning_timer += Time.deltaTime;
        if (haning_timer < haning_Update_timer)
        {
            Cheasing();
            return;
        }

        haning_timer = 0f;
        cheasing_left = Random.Range(0, 2) == 0 ? true : false;
        anim.SetBool("walking", true);
    }

    void CheckCheasingPlayer()
    {
        if (is_cheasing)
        {
            Cheasing();
            return;
        }
        // 0. 判断玩家是否可以被发现
        if (!PlayerControl._instance.is_monster_target) return;
        // 1. 判断跟玩家是否在同层
        if (Mathf.Abs(transform.position.y - player.position.y) > 5) return;
        // 2. 是否在追捕距离内
        var distance = Vector3.Distance(transform.position, player.position);
        if (distance > cheasing_distance) return;
        StartCheasing();
    }

    void StartCheasing()
    {
        is_cheasing = true;
        // 1. 判断玩家在左侧还是右侧，向那个方向移动
        cheasing_left = true;
        if (player.position.x >= transform.position.x) cheasing_left = false;
        anim.SetBool("walking", true);
        Invoke("StopCheasing", cheasing_interval);
    }
    public ScaryStuffGenerator_Monster scary_item_prefab;
    public float walking_speed_factor = 0.35f;
    public float scary_item_distance = 10f;
    public float scary_item_interval = 0.5f;
    public float scary_item_interval_haning = 1.5f;
    private float scary_item_timer;
    async void Cheasing()
    {
        //距离玩家够近时，不间断出现需要玩家点击的字
        if (Vector3.Distance(this.transform.position, player.position) < scary_item_distance)
        {
            scary_item_timer += Time.deltaTime;
            var interval = is_cheasing ? scary_item_interval : scary_item_interval_haning;
            if (scary_item_timer >= interval)
            {
                scary_item_timer = 0f;
                var scary_item = Instantiate(scary_item_prefab);
                scary_item.transform.position = this.transform.position + Vector3.up * Random.Range(2f, 5f) + Vector3.left * Random.Range(-2f, 2f) + Vector3.forward * 3f;
                scary_item.CheckGenerate();
            }
        }

        var real_speed = (walking_speed + walking_speed_modify) * (is_cheasing ? 1.0f : haning_speed_multiplier);
        if (cheasing_left)
        {
            anim.SetFloat("walking_speed", real_speed * walking_speed_factor);
            transform.position -= new Vector3(real_speed * Time.deltaTime, 0, 0);
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            anim.SetFloat("walking_speed", -real_speed * walking_speed_factor);
            transform.position -= new Vector3(-real_speed * Time.deltaTime, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void StopCheasing()
    {
        is_cheasing = false;
        walking_speed_modify = 0f;
        anim.SetBool("walking", false);
        ChangeStayPoints();
    }

    void UpdateCollider()
    {
        if (is_cheasing)
        {
            collid.enabled = true;
            rigidb.simulated = true;
        }
        else if (Vector3.Distance(player.position, this.transform.position) < cheasing_distance)
        {
            collid.enabled = false;
            rigidb.simulated = false;
        }
        else
        {
            collid.enabled = true;
            rigidb.simulated = true;
        }
    }

    private float start_stay_time = 0;
    void UpdateStayPoints()
    {
        //追逐中，不换位置
        if (is_cheasing) return;
        //没在追逐状态，但距离玩家近，不换位置
        else if (Vector3.Distance(this.transform.position, player.position) < stay_distance) return;
        //刷新时间没到，不换位置
        if (Time.fixedTime - start_stay_time < stay_interval) return;

        ChangeStayPoints();
    }

    void ChangeStayPoints()
    {
        start_stay_time = Time.fixedTime;
        // 找个新的目的地
        bool found = false;
        int count = stay_points.Count;
        while (!found)
        {
            var position = stay_points[Random.Range(0, count)];
            //不在玩家视野里凭空出现
            if (Vector3.Distance(position, player.position) < play_limit_distance) continue;
            //只在玩家楼层出现
            if (Mathf.Abs(position.y - player.position.y) > 5) continue;

            found = true;
            transform.position = position;
        }
    }

    void Shake()
    {
        if (is_cheasing)
            this.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(-15f, 15f), 0f);
        else
            this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void Fast()
    {
        walking_speed_modify += walking_speed_change;
    }

    public void Slow()
    {
        walking_speed_modify -= walking_speed_change * 0.5f;
        if (walking_speed_modify <= -1f)
            walking_speed_modify = -1f;
    }
}
