using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public Animator animator;
    public Vector2 standard_vector;
    public Vector3 player_offset;
    public List<int> play_state_dilimiters;
    private Vector3 main_role_position_on_screen;
    private GameObject hidding_btn_prefab;

    private void Awake()
    {
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
        var typed_arg = (CRCustomArgs.OnSanValueChangedArg) arg;
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
        main_role_position_on_screen = Camera.main.WorldToScreenPoint(transform.position + player_offset);
        SetWalking(false);
    }

    void Update()
    {
        UpdateKeys();
        UpdateLightDirection();
        UpdateHiddingBtnState();
        if (hiding) return;
        UpdateMovement();
    }

    private bool facing_right = true;
    
    void UpdateLightDirection()
    {
        // 算法描述： 计算鼠标和角色之间的方向向量，计算头顶的夹角。如果方向向量向右，不需要翻转，如果向左则需要翻转角色
        var mouse_pos = Input.mousePosition;
        var light_direction_v3 = (mouse_pos - main_role_position_on_screen).normalized;
        var light_direction = new Vector2(light_direction_v3.x, light_direction_v3.y);
        float angle = Vector2.Angle(standard_vector, light_direction);
        if (light_direction.x >= 0)
        {
            // 向右侧，无需翻转
            transform.localScale = new Vector3(1, 1);
            animator.SetFloat("hand_rotation", 1 - angle/180.0f);
            facing_right = true;
        }
        else if(light_direction.x < 0)
        {
            // 向左侧，需要翻转
            transform.localScale = new Vector3(-1, 1);
            animator.SetFloat("hand_rotation", 1 - angle/180.0f);
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
    }


    public float walking_speed = 0.1f;
    public float walking_speed_factor = 10f;
    
    void UpdateMovement()
    {
        bool walking_right = false;
        if (holding_A)
        { // 往左走
            transform.position -= new Vector3(walking_speed * Time.deltaTime, 0, 0);
            SetWalking(true);
            walking_right = false;
        }else if (holding_D)
        {// 往右走
            transform.position -= new Vector3(-walking_speed * Time.deltaTime, 0, 0);
            SetWalking(true);
            walking_right = true;
        }
        else
        {// 没走
            SetWalking(false);
        }
        if(walking_right != facing_right)
        {
            animator.SetFloat("walking_speed", walking_speed * walking_speed_factor);
        }
        else
        {
            animator.SetFloat("walking_speed", walking_speed * walking_speed_factor * -1);
        }
    }

    void UpdateHiddingBtnState()
    {
        if (hidding_btn == null) return;
        if (hiding)
        {
            hidding_btn_text.text = "离开";
        }
        else
        {
            hidding_btn_text.text = "躲藏";
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
        var typed_arg = (CRCustomArgs.TransPlayerToPositionArg) arg;
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

        if (other.gameObject.CompareTag("scary_item"))
        {
            var hitted_item = other.gameObject.GetComponent<ScaryStuffGenerator>();
            hitted_item.CheckGenerate();
        }

        if (other.gameObject.CompareTag("monster"))
        {
            if (hiding) return;
            CREventSystem.Instance.DispatchCREventByKey(CRCustomEvents.ON_GAME_OVER, null);
        }

        if (other.gameObject.CompareTag("hiding_place"))
        {
            if (hidding_btn == null)
            {
                hidding_btn = Instantiate(hidding_btn_prefab, canvas.transform).GetComponent<UISceneFollower>();
                hidding_btn.fellowing_obj = other.gameObject.transform;
                hidding_btn.GetComponent<UIButton>().on_click = OnHiddingBtnClick;
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
            if (hidding_btn == null) return;
            hidding_btn.FadeOutAfter(1);
            hidding_btn = null;
        }
    }

    public void OnTransBtnClick()
    {
        Debug.Log("sfdasdfsdf");
        if (current_trans_gate == null) return;
        current_trans_gate.Trans();
    }

    public void OnHiddingBtnClick()
    {
        hiding = !hiding;
        //todo 调用动画
    }
}
