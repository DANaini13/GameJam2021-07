using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    public bool is_up;
    public bool is_classroom;

    public Vector3 Trans(Transform go)
    {
        float offsetX = is_classroom ? 0f : 5.8f;
        float offsetY = is_classroom ? RoomGenerator.classroom_offset : RoomGenerator.level_height;

        var pos = go.position;
        if (is_up)
            pos += (new Vector3(offsetX, offsetY, 0f));
        else
            pos += (new Vector3(-offsetX, -offsetY, 0f));
        go.transform.position = pos;
        return pos;
    }
}
