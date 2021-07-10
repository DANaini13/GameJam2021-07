using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public Animator animator;
    public Vector2 standard_vector;
    private Vector3 main_role_position_on_screen;
    
    void Start()
    {
        main_role_position_on_screen = Camera.main.WorldToScreenPoint(transform.position);
    }

    void Update()
    {
        UpdateKeys();
        UpdateLightDirection();
        UpdateMovement();
    }

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
        }
        else if(light_direction.x < 0)
        {
            // 向左侧，需要翻转
            transform.localScale = new Vector3(-1, 1);
            animator.SetFloat("hand_rotation", 1 - angle/180.0f);
        }
    }

    private bool holding_A = false;
    public bool holding_D = false;

    void UpdateKeys()
    {
        if (Input.GetKeyDown(KeyCode.A)) holding_A = true;

        if (Input.GetKeyUp(KeyCode.A)) holding_A = false;
        
        if (Input.GetKeyDown(KeyCode.D)) holding_D = true;

        if (Input.GetKeyUp(KeyCode.D)) holding_D = false;
    }
    

    public float speed = 0.1f;
    void UpdateMovement()
    {
        if (holding_A)
        { // 往左走
            transform.position -= new Vector3(speed, 0, 0);
        }

        if (holding_D)
        {// 往右走
            transform.position -= new Vector3(-speed, 0, 0);
        }
    }
}
