using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    private List<GameObject> room_prefabs;
    public float width_interval;
    public float height_interva;
    public Vector2Int size;

    private void Awake()
    {
        // room_prefabs = Resources.LoadAll<GameObject>("Prefabs/Room").ToList();
        // int count = room_prefabs.Count;
        // if (count <= 0) return;
        // for (int row = 0; row < size.x; ++row)
        // {
        //     for (int col = 0; col < size.y; ++col)
        //     {
        //         var room_prefab = room_prefabs[Random.Range(0, count)];
        //         var position = new Vector3(row * width_interval, col * height_interva, 0);
        //         var room = Instantiate(room_prefab, transform);
        //         room.transform.position = position;
        //     }
        // }
    }
}
