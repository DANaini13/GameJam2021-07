using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private GameObject room_prefab;
    public float width_interval;
    public float height_interva;
    public Vector2Int size;

    private void Awake()
    {
        room_prefab = Resources.Load("Prefabs/room_baisc") as GameObject;
        for (int row = 0; row < size.x; ++row)
        {
            for (int col = 0; col < size.y; ++col)
            {
                var position = new Vector3(row * width_interval, col * height_interva, 0);
                var room = Instantiate(room_prefab, transform);
                room.transform.position = position;
            }
        }
    }
}
