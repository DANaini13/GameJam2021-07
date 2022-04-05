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
    public float walking_speed_modify;
    public float cheasing_interval;
    public Transform player;
    public AudioSource audio_source;
    private Animator anim;
    private BoxCollider2D collid;
    public float hunting_interval_min = 60f;
    public float hunting_interval_max = 90f;
    public float hunting_duration = 30f;
    public float hunting_interval_timer;
    public float hunting_interval;
    private bool is_hunting = false;

    private List<Vector3> stay_points = new List<Vector3>();

    private void Awake()
    {
        _instance = this;
        collid = this.transform.GetComponent<BoxCollider2D>();
        collid.enabled = false;
        anim = this.transform.GetComponentInChildren<Animator>();
        hunting_interval = Random.Range(hunting_interval_min, hunting_interval_max);
    }

    public float cheasing_timer;
    void Update()
    {
        CheckHunting();
        CheckCheasingPlayer();
        Haning();
        UpdateStayPoints();
        UpdateHeartBeat();
        Shake();
        if (cheasing_timer > 0f)
        {
            cheasing_timer -= Time.deltaTime;
            if (cheasing_timer <= 0)
                StopCheasing();
        }
    }

    public bool is_cheasing = false;        //是否在追逐玩家
    public bool cheasing_left = false;      //是否朝左走
    public float haning_speed_multiplier = 0.5f;    //闲逛时的移速乘数
    private bool is_haning;                 //当前是否在闲逛
    void Haning()
    {
        //如果在追玩家，则退出
        if (is_cheasing) return;
        //否则闲逛
        if (is_haning) Cheasing();
    }

    void CheckCheasingPlayer()
    {
        if (is_cheasing)
        {
            Cheasing();
            return;
        }
        // 0. 玩家是否可以被发现
        if (!PlayerControl._instance.is_monster_target) return;
        // 1. 跟玩家是否在同层
        if (Mathf.Abs(transform.position.y - player.position.y) > 5) return;
        // 2. 是否在追捕距离内
        var distance = Vector3.Distance(transform.position, player.position);
        if (distance > cheasing_distance) return;

        StartCheasing();
    }

    void CheckHunting()
    {
        //游戏还没开始
        if (!PlayerControl._instance.is_game_start) return;
        //正在猎杀，不进入
        if (is_hunting) return;
        //正在追逐，不进入
        if (is_cheasing) return;
        //倒计时没到期，不进入
        if (hunting_interval_timer < hunting_interval)
        {
            hunting_interval_timer += Time.deltaTime;
            return;
        }

        StartHunting();
    }
    void StartHunting()
    {
        is_cheasing = true;
        is_hunting = true;

        //刷新到玩家楼层
        ChangeStayPoints();

        //玩家电筒抽抽
        PlayerControl._instance.StartHuntingTime();

        //开启碰撞
        collid.enabled = true;

        anim.SetBool("walking", true);

        //开启计时，追逐结束后离开
        cheasing_timer = hunting_duration;
    }

    void StartCheasing()
    {
        is_cheasing = true;

        //开启碰撞
        collid.enabled = true;

        anim.SetBool("walking", true);

        //开启计时，追逐结束后离开
        cheasing_timer = cheasing_interval;
    }
    public ScaryStuffGenerator_Monster scary_item_prefab, scary_item_hunting_prefab;
    public float walking_speed_hunting = 5f;
    public float walking_speed_factor = 0.35f;
    public float scary_item_distance = 10f;
    public float scary_item_interval = 0.5f;
    public float scary_item_interval_haning = 1.5f;
    private float scary_item_timer;
    void Cheasing()
    {
        //距离玩家够近时，不间断出现需要玩家点击的字
        if (Vector3.Distance(this.transform.position, player.position) < scary_item_distance)
        {
            scary_item_timer += Time.deltaTime;
            var interval = is_cheasing ? scary_item_interval : scary_item_interval_haning;
            if (scary_item_timer >= interval)
            {
                scary_item_timer = 0f;
                var prefab = is_hunting ? scary_item_hunting_prefab : scary_item_prefab;
                var scary_item = Instantiate(prefab);
                scary_item.transform.position = this.transform.position;
                scary_item.CheckGenerate();
            }
        }

        //追逐时，持续朝着玩家移动
        //也即闲逛状态不会持续计算
        if (is_cheasing)
        {
            cheasing_left = true;
            if (player.position.x >= transform.position.x) cheasing_left = false;
        }

        //移动
        var real_speed = (walking_speed + walking_speed_modify) * (is_cheasing ? 1.0f : haning_speed_multiplier);
        if (is_hunting) real_speed = walking_speed_hunting;
        if (cannot_move) real_speed = 0f;

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
        //当追逐开始若干秒后，强制结束追逐
        is_cheasing = false;
        collid.enabled = false;
        walking_speed_modify = 0f;
        anim.SetBool("walking", false);
        ChangeStayPoints();
        //玩家电筒停止抽抽
        PlayerControl._instance.StopHuntingTime();
        //重置猎杀状态
        is_hunting = false;
        hunting_interval_timer = 0f;
        hunting_interval = Random.Range(hunting_interval_min, hunting_interval_max);
    }

    private float start_stay_time = 0;
    void UpdateStayPoints()
    {
        //游戏还没开始
        if (!PlayerControl._instance.is_game_start) return;
        //追逐中，不换位置
        if (is_cheasing) return;
        //距离玩家很近，不换位置
        if (Vector3.Distance(this.transform.position, player.position) < stay_distance) return;
        //刷新时间没到，不换位置
        if (Time.fixedTime - start_stay_time < stay_interval) return;

        ChangeStayPoints();
    }

    public AudioClip sfx_change_stay_point;
    void ChangeStayPoints()
    {
        AudioSource.PlayClipAtPoint(sfx_change_stay_point, this.transform.position);

        start_stay_time = Time.fixedTime;
        // 找个新的目的地
        bool found = false;
        // int count = stay_points.Count;
        int steps = 0;
        //尝试一千次，找不到就先不瞬移
        while (!found && steps < 1000)
        {
            steps++;
            //在玩家楼层找到一个点
            var position = player.position;
            var min = play_limit_distance;
            var max = RoomGenerator.room_count * RoomGenerator.room_length;
            var offset = 0f;
            //优先出现在玩家左边
            if (Random.Range(0, 2) == 0)
            {
                if (position.x > play_limit_distance)
                    offset = Random.Range(0f, position.x - play_limit_distance);
                else
                    offset = Random.Range(position.x + play_limit_distance, max);
            }
            //右边
            else
            {
                if (position.x < max - play_limit_distance)
                    offset = Random.Range(position.x + play_limit_distance, max);
                else
                    offset = Random.Range(0f, position.x - play_limit_distance);
            }

            position.x = offset;
            //只在存在的房间出现
            // if (!RoomGenerator._instance.HasRoomByPos(position)) continue;

            found = true;
            transform.position = position;
        }

        //一定几率开始闲逛
        is_haning = Random.Range(0, 3) == 0 ? false : true;
        //向玩家方向闲逛
        if (is_haning)
        {
            cheasing_left = true;
            if (player.position.x >= transform.position.x) cheasing_left = false;
        }
        anim.SetBool("walking", is_haning);
    }

    //抽抽，表示正在追玩家
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
        walking_speed_modify -= walking_speed_change;
        if (walking_speed_modify <= 0f)
            walking_speed_modify = 0f;
    }


    public float heart_beat_distance = 8f;
    public AudioClip heart_beat_sfx, heart_beat_rapid_sfx;
    private bool is_heart_beat_rapid;
    void UpdateHeartBeat()
    {
        if (Vector3.Distance(player.position, this.transform.position) < heart_beat_distance)
        {
            if (is_cheasing)
            {
                if (!is_heart_beat_rapid || !audio_source.isPlaying)
                {
                    audio_source.clip = heart_beat_rapid_sfx;
                    audio_source.Play();
                }
                is_heart_beat_rapid = true;
            }
            else
            {
                if (is_heart_beat_rapid || !audio_source.isPlaying)
                {
                    audio_source.clip = heart_beat_sfx;
                    audio_source.Play();
                }
                is_heart_beat_rapid = false;
            }
        }
        else
        {
            audio_source.clip = null;
            audio_source.Stop();
        }
    }

    private bool is_follow_transport = false;
    private Vector3 follow_transport_pos;
    public void PlayerTransport(Vector3 from, Vector3 to)
    {
        //没在追玩家，不跟着传送
        if (!is_cheasing) return;
        //只记录一次玩家传送
        if (is_follow_transport) return;

        is_follow_transport = true;
        //算出当前到传送点要走多久
        float dis = Vector3.Distance(this.transform.position, from);
        float time = dis / walking_speed;
        follow_transport_pos = to;
        Invoke("FollowTransport", time);
    }

    void FollowTransport()
    {
        is_follow_transport = false;
        //没在追人了，算了
        if (!is_cheasing) return;
        //玩家跟自己依然在同一层（可能玩家又跑回来了），不传送
        if (Mathf.Abs(transform.position.y - player.position.y) < 5) return;
        //传送
        this.transform.position = follow_transport_pos;
    }

    private bool cannot_move;
    //被玩家闪了
    public void Flashed()
    {
        if (!is_cheasing) return;
        if (Vector3.Distance(this.transform.position, player.position) > heart_beat_distance) return;

        MonsterAI._instance.cheasing_timer -= 10f;
        cannot_move = true;
        Invoke("ContinueMove", 2f);
    }

    void ContinueMove()
    {
        cannot_move = false;
    }
}
