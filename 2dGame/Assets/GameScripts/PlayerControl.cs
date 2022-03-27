using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl _instance;
    public Animator animator;
    public Vector2 standard_vector;
    public Vector3 player_offset;
    public List<int> play_state_dilimiters;
    private GameObject hidding_btn_prefab;
    private bool is_near_hiding_point;
    public bool is_monster_target;

    private void Awake()
    {
        _instance = this;
        breath_cur_vital_capacity = breath_max_vital_capacity;
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.ON_SAN_VALUE_CHANGED, this, OnSanValueChangedEvent);
        CREventSystem.Instance.ListenCustomeEventByKey(CRCustomEvents.TRANS_PLAYER_TO_POSITION, this, TransToPositionEvent);
        floor_btn = Resources.Load("Prefabs/stair_btn") as GameObject;
        hidding_btn_prefab = Resources.Load("Prefabs/hiding_btn") as GameObject;
    }

    private void OnDestroy()
    {
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.ON_SAN_VALUE_CHANGED, this);
        CREventSystem.EraseCustomeEventByKey(CRCustomEvents.TRANS_PLAYER_TO_POSITION, this);
    }

    void OnSanValueChangedEvent(object arg)
    {
        var typed_arg = (CRCustomArgs.OnSanValueChangedArg)arg;
        // 通过san值决定使用哪个角色state
        if (typed_arg.san_value < play_state_dilimiters[0] && typed_arg.san_value >= play_state_dilimiters[1])
        {
            animator.SetInteger("state", 3);
        }
        if (typed_arg.san_value < play_state_dilimiters[1] && typed_arg.san_value >= play_state_dilimiters[2])
        {
            animator.SetInteger("state", 2);
        }
        if (typed_arg.san_value < play_state_dilimiters[2] && typed_arg.san_value >= play_state_dilimiters[3])
        {
            animator.SetInteger("state", 1);
        }
    }

    void Start()
    {
        SetWalking(false);
    }

    void Update()
    {
        UpdateKeys();
        UpdateLightDirection();
        UpdateHiddingBtnState();
        UpdateMovement();
        UpdateBreath();
        UpdateMonsterTarget();
    }

    [Header("瞄准的目标")]
    public Transform aim_target;
    [Header("瞄准点的圆心")]
    public Transform aim_pivot;
    [Header("瞄准点的圆形半径")]
    public float aim_radius = 0.3f;
    [Header("当旋转处于该角度区间时，对坐标进行等比修正")]
    public Vector2 aim_fix_threshold = new Vector2(45f, 180f);
    [Header("旋转等于该角度时，修正值达到最大")]
    public float aim_fix_center_angle = 180f;
    [Header("坐标修正最大值")]
    public float aim_fix_offset = 1.2f;
    [Header("角度大于该值时，不再旋转")]
    public float aim_max_angle = 120f;

    private bool facing_right = true;

    void UpdateLightDirection()
    {
        var mouse_pos = Input.mousePosition;
        //得到旋转轴心的屏幕坐标
        var start_position_on_screen = Camera.main.WorldToScreenPoint(aim_pivot.position);
        //计算鼠标到轴心的方向向量
        var light_direction_v3 = (mouse_pos - start_position_on_screen).normalized;
        var light_direction = new Vector2(light_direction_v3.x, light_direction_v3.y);

        //计算电筒的夹角
        float angle = Vector2.Angle(standard_vector, light_direction);
        //约束电筒角度
        if (angle > aim_max_angle) angle = aim_max_angle;

        //让瞄准点始终位于一个圆上
        aim_target.position = (light_direction_v3 * aim_radius) + aim_pivot.position;
        //=====修正
        //瞄准越接近前方，瞄准点越往后方移动（方便手肘弯曲）
        if (angle > aim_fix_threshold.x && angle < aim_fix_threshold.y)
        {
            //算出当前距离正前方的比例（1表示指向前方，0表示在阈值边缘）
            float ratio = 0.0f;
            if (angle < aim_fix_center_angle)
                ratio = (angle - aim_fix_threshold.x) / (aim_fix_center_angle - aim_fix_threshold.x);
            else
                ratio = 1.0f - (angle - aim_fix_center_angle) / (aim_fix_threshold.y - aim_fix_center_angle);

            var pos = aim_target.localPosition;
            pos.x -= ratio * aim_fix_offset;
            aim_target.localPosition = pos;
        }

        //翻转
        if (light_direction.x >= 0)
        {
            // 向右侧，无需翻转
            transform.localScale = new Vector3(1, 1);
            // animator.SetFloat("hand_rotation", 1 - angle / 180.0f);
            aim_target.rotation = Quaternion.Euler(0f, 0f, -angle);
            facing_right = true;
        }
        else if (light_direction.x < 0)
        {
            // 向左侧，需要翻转
            transform.localScale = new Vector3(-1, 1);
            // animator.SetFloat("hand_rotation", 1 - angle / 180.0f);
            aim_target.rotation = Quaternion.Euler(0f, 0f, angle);
            facing_right = false;
        }
    }

    private bool holding_A = false;
    private bool holding_D = false;

    void UpdateKeys()
    {
        if (Input.GetKeyDown(KeyCode.A)) holding_A = true;

        if (Input.GetKeyUp(KeyCode.A)) holding_A = false;

        if (Input.GetKeyDown(KeyCode.D)) holding_D = true;

        if (Input.GetKeyUp(KeyCode.D)) holding_D = false;

        if (Input.GetKeyDown(KeyCode.E)) Hide();

        if (Input.GetMouseButtonDown(1)) TurnOnFlashLight();

        if (Input.GetKeyDown(KeyCode.T)) BreakFlashlight();

        if (Input.GetKeyDown(KeyCode.LeftShift)) HoldBreath();
        if (Input.GetKeyUp(KeyCode.LeftShift)) StartBreath();
    }


    public float walking_speed = 0.1f;
    public float walking_speed_factor = 10f;

    void UpdateMovement()
    {
        if (hiding) return;

        float real_walking_speed = walking_speed * (is_hold_breath ? hold_breath_move_speed_multiplier : 1.0f);

        bool walking_right = false;
        if (holding_A)
        { // 往左走
            transform.position -= new Vector3(real_walking_speed * Time.deltaTime, 0, 0);
            SetWalking(true);
            walking_right = false;
        }
        else if (holding_D)
        {// 往右走
            transform.position -= new Vector3(-real_walking_speed * Time.deltaTime, 0, 0);
            SetWalking(true);
            walking_right = true;
        }
        else
        {// 没走
            SetWalking(false);
        }

        //动画速度，面朝和移动方向一致时，为正，否则为负
        if (walking_right != facing_right)
        {
            animator.SetFloat("walking_speed", real_walking_speed * walking_speed_factor);
        }
        else
        {
            animator.SetFloat("walking_speed", real_walking_speed * walking_speed_factor * -1);
        }
    }

    void UpdateHiddingBtnState()
    {
        if (hidding_btn == null) return;
        if (hiding)
        {
            hidding_btn_text.text = "按下 E 离开";
        }
        else
        {
            hidding_btn_text.text = "按下 E 躲藏";
        }
    }

    private bool walking = false;
    private void SetWalking(bool walking)
    {
        if (this.walking == walking) return;
        animator.SetBool("walking", walking);
        this.walking = walking;
    }

    void TransToPositionEvent(object arg)
    {
        var typed_arg = (CRCustomArgs.TransPlayerToPositionArg)arg;
        transform.position = typed_arg.position;
    }

    private TransGate current_trans_gate = null;
    private UISceneFollower current_btn = null;
    private GameObject floor_btn = null;
    private UISceneFollower hidding_btn = null;
    private Text hidding_btn_text = null;
    private bool hiding = false;
    public Canvas canvas;

    public void OnTriggerEnter2D(Collider2D other)
    {
        //传送门
        if (other.gameObject.CompareTag("trans_gate"))
        {
            var hitted_trans_gate = other.gameObject.GetComponent<TransGate>();
            if (current_trans_gate != null && current_trans_gate.gate_tag == hitted_trans_gate.gate_tag)
                return;
            current_trans_gate = hitted_trans_gate;
            // 生成button
            var fellower = Instantiate(floor_btn, canvas.transform).GetComponent<UISceneFollower>();
            fellower.fellowing_obj = current_trans_gate.transform;
            current_btn = fellower;
            var button = fellower.GetComponent<UIButton>();
            button.on_click = OnTransBtnClick;
        }

        //黄字
        if (other.gameObject.CompareTag("scary_item"))
        {
            var hitted_item = other.gameObject.GetComponent<ScaryStuffGenerator>();
            hitted_item.CheckGenerate();
        }

        //怪物
        if (other.gameObject.CompareTag("monster"))
        {
            if (hiding) return;
            if (is_hold_breath) return;
            CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.ON_GAME_OVER, null);
        }

        //躲藏点
        if (other.gameObject.CompareTag("hiding_place"))
        {
            is_near_hiding_point = true;
            if (hidding_btn == null)
            {
                hidding_btn = Instantiate(hidding_btn_prefab, canvas.transform).GetComponent<UISceneFollower>();
                hidding_btn.fellowing_obj = other.gameObject.transform;
                // hidding_btn.GetComponent<UIButton>().on_click = Hide;
                hidding_btn_text = hidding_btn.transform.GetChild(0).GetComponent<Text>();
            }
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("trans_gate"))
        {
            if (current_trans_gate == null) return;
            current_btn.FadeOutAfter(1);
            current_trans_gate = null;
        }

        if (other.gameObject.CompareTag("hiding_place"))
        {
            if (!hiding)
            {
                is_near_hiding_point = false;
                if (hidding_btn == null) return;
                hidding_btn.FadeOutAfter(0.15f);
                hidding_btn = null;
            }
        }
    }

    public void OnTransBtnClick()
    {
        if (current_trans_gate == null) return;
        current_trans_gate.Trans();
    }

    public void Hide()
    {
        //与躲藏点处于交互状态才可以躲藏
        if (!is_near_hiding_point) return;

        //停止移动
        SetWalking(false);

        hiding = !hiding;
        if (hiding)
        {
            animator.SetBool("hide", true);
            GetComponent<CapsuleCollider2D>().enabled = false;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            animator.SetBool("hide", false);
            GetComponent<CapsuleCollider2D>().enabled = true;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public UnityEngine.Experimental.Rendering.Universal.Light2D flashlight;
    public float flashlight_default_intensity = 0.7f;
    public AudioSource audio_source;
    public AudioClip[] sfx_flashlight_click;
    public AudioClip sfx_btn_click;
    private int flashlight_reboot_count;
    private bool is_flashlight_control = true;
    private bool is_flashlight_on = true;
    public void BreakFlashlight()
    {
        if (!is_flashlight_on) return;

        audio_source.PlayOneShot(sfx_flashlight_click[1]);
        is_flashlight_on = false;
        flashlight.intensity = 0f;
        //设置重新开启需要按几次键
        flashlight_reboot_count = UnityEngine.Random.Range(1, 6);
    }

    void TurnOnFlashLight()
    {
        //没有控制权时不允许操作
        if (!is_flashlight_control) return;

        is_flashlight_on = !is_flashlight_on;

        //开灯
        if (is_flashlight_on)
        {
            audio_source.PlayOneShot(sfx_flashlight_click[0]);
            flashlight.intensity = flashlight_default_intensity;

            //如果重启次数没耗尽，则让协程关灯
            if (flashlight_reboot_count > 0)
            {
                //关闭控制权
                is_flashlight_control = false;
                flashlight_reboot_count--;
                StartCoroutine("ShutdownFlashLight");
            }
        }
        //关灯
        else
        {
            audio_source.PlayOneShot(sfx_flashlight_click[1]);
            flashlight.intensity = 0.0f;
        }
    }

    IEnumerator ShutdownFlashLight()
    {
        //延迟关灯，制造闪烁效果
        yield return new WaitForSeconds(0.05f);
        flashlight.intensity = 0f;
        //关灯后才恢复控制权
        is_flashlight_control = true;
    }

    public ParticleSystem ps_click;

    public void ClickBtn(Vector3 pos)
    {
        audio_source.PlayOneShot(sfx_btn_click);
        Transform ps = Instantiate(ps_click).transform;
        pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0f;
        ps.position = pos;
    }

    public AudioClip sfx_hold_breath;
    public AudioClip sfx_start_breath;
    public float hold_breath_move_speed_multiplier = 0.33f;
    public float breath_max_vital_capacity = 5f;
    public ProgressBar breath_ui_vital_bar;
    private float breath_cur_vital_capacity;
    private float breath_vital_capacity_recovery_speed = 2f;
    private bool is_hold_breath;
    void HoldBreath()
    {
        animator.SetBool("hold_breath", true);
        audio_source.clip = sfx_hold_breath;
        audio_source.Play();
        is_hold_breath = true;
    }

    void UpdateBreath()
    {
        if (!is_hold_breath)
        {
            if (breath_cur_vital_capacity < breath_max_vital_capacity)
            {
                breath_cur_vital_capacity += breath_vital_capacity_recovery_speed * Time.deltaTime;
                if (breath_cur_vital_capacity > breath_max_vital_capacity)
                    breath_cur_vital_capacity = breath_max_vital_capacity;
            }
        }
        else
        {
            breath_cur_vital_capacity -= Time.deltaTime;
            if (breath_cur_vital_capacity < 0)
                StartBreath();
        }
        var ratio = breath_cur_vital_capacity / breath_max_vital_capacity;
        breath_ui_vital_bar.SetProgress(ratio);
        var images = breath_ui_vital_bar.GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            if (image.transform == breath_ui_vital_bar.transform) continue;
            image.color = Color.Lerp(Color.red, Color.white, ratio);
        }
    }

    public void StartBreath()
    {
        animator.SetBool("hold_breath", false);
        var ratio = breath_cur_vital_capacity / breath_max_vital_capacity;
        if (ratio < 0.5f)
        {
            audio_source.clip = sfx_start_breath;
            audio_source.Play();
        }
        is_hold_breath = false;
    }

    void UpdateMonsterTarget()
    {
        //屏住呼吸，且没有开灯，则不会被发现
        if (is_hold_breath && !is_flashlight_on)
            is_monster_target = false;
        else
            is_monster_target = true;
    }

}
