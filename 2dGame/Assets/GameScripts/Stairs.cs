using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    public bool is_up;

    public void Trans(Transform go)
    {
        if (is_up)
            go.Translate(new Vector3(5.8f, RoomGenerator.level_height, 0f));
        else
            go.Translate(new Vector3(-5.8f, -RoomGenerator.level_height, 0f));
    }
}
