using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("一共有多少层")]
    public int level_count = 4;
    [Header("每层多少个房间")]
    public int room_count = 9;
    [Header("每层的第几个房间是楼梯")]
    public int[] stairs_index = new int[3] { 1, 4, 7 };
    [Header("每层楼的高度")]
    public float level_height = 15.0f;
    [Header("房间长度")]
    public float room_length = 30.0f;
    [Header("房间半高，用来放地板")]
    public float room_half_height = 4.5f;

    public Transform[] room_normal_prefabs;
    public Transform stairs_down_prefab;
    public Transform stairs_up_prefab;
    public Transform level_num_prefab;
    public Transform ground_prefab;
    public Transform wall_prefab;

    private int[,] room_data;

    void Awake()
    {
        GeneratePath(0);
        GenerateRooms();
        GenerateStairs();
    }

    [Header("主路径长度")]
    public int path_steps = 12;
    void GeneratePath(int try_times)
    {
        //================================================让寻路可以连续爬楼！！！！！！！！
        if (try_times >= 1000)
        {
            Debug.Log("找不到，不找了");
            return;
        }

        room_data = new int[room_count, level_count];

        //标记楼梯层
        for (int y = 0; y < level_count; y++)
            foreach (var x in stairs_index)
                room_data[x, y] = -1;

        List<Vector2Int> room_list = new List<Vector2Int>();
        //找到一个初始房间
        int startX = 0;
        int startY = 0;
        while (true)
        {
            startX = Random.Range(0, room_count);
            startY = Random.Range(0, level_count);
            if (room_data[startX, startY] == 0)//不能刷在楼梯房
                break;
        }

        int nextX = startX;
        int nextY = startY;
        room_list.Add(new Vector2Int(nextX, nextY));
        room_data[nextX, nextY] = 9;//标记开始房间

        //开始寻路
        int steps = path_steps;
        int dir = 0;
        bool is_climb = false;
        while (steps >= 0)
        {
            //到达新楼层时，决定一个前进方向
            if (dir == 0) dir = Random.Range(0, 2) == 0 ? 1 : -1;

            //前进
            nextX += dir;
            //走到死路了，退出
            if (!Is_Room_Exist(nextX, nextY))
                break;
            //走到重复路了，退出
            if (room_data[nextX, nextY] > 0)
                break;

            //走到楼梯间，看看要不要换楼层
            if (room_data[nextX, nextY] == -1)
            {
                //先算一下前方还有多少楼梯
                int count = 1;
                foreach (var s in stairs_index)
                {
                    if (dir > 0)
                    {
                        if (s > nextX)
                            count++;
                    }
                    else
                    {
                        if (s < nextX)
                            count++;
                    }
                }
                //根据剩余楼梯数决定进楼梯的几率
                is_climb = Random.Range(0, count) == 0 ? true : false;

                //爬！看看上还是下
                if (is_climb)
                {
                    List<int> list = new List<int>();
                    if (Is_Room_Exist(nextX, nextY + 1)) list.Add(1);
                    if (Is_Room_Exist(nextX, nextY - 1)) list.Add(-1);
                    int offsetY = list[Random.Range(0, list.Count)];
                    //根据爬楼梯的方向，定义楼梯间的标记
                    //10-向上传送/100-向下传送
                    room_data[nextX, nextY] = offsetY > 0 ? 10 : 100;

                    //爬完之后，定义新房间的标记
                    nextY += offsetY;
                    room_data[nextX, nextY] = offsetY > 0 ? 100 : 10;

                    //爬完换层，重新定一个前进方向
                    dir = 0;
                    //多走两步
                    steps -= 2;
                }
                //不爬，那当前房间换成空房间
                else
                    room_data[nextX, nextY] = 1;
            }
            //没走到楼梯间，当前房间换成空房间
            else
                room_data[nextX, nextY] = 1;

            steps--;
        }

        //步长没有走完，说明没找到路径，重试
        if (steps > 0)
            GeneratePath(try_times + 1);
        else
            Debug.Log("找到路径，总尝试次数" + try_times);
    }

    bool Is_Room_Exist(int x, int y)
    {
        if (x < 0 || x >= room_count || y < 0 || y >= level_count)
            return false;
        return true;
    }

    void GenerateRooms()
    {
        for (int y = 0; y < level_count; y++)
        {
            for (int x = 0; x < room_count; x++)
            {
                if (room_data[x, y] <= 0) continue;

                int r = Random.Range(0, room_normal_prefabs.Length);
                var prefab = room_normal_prefabs[r];
                var room = Instantiate(prefab, this.transform).transform;
                room.position = new Vector3(room_length * x + room_length * 0.5f, level_height * y + room_half_height, 0f);
            }
            var ground = Instantiate(ground_prefab, this.transform).transform;
            ground.gameObject.SetActive(true);
            var length = room_count * room_length;
            ground.position = new Vector3(length * 0.5f, level_height * y, 0f);
            ground.localScale = new Vector3(length, 1f, 1f);

            var wall = Instantiate(wall_prefab, this.transform).transform;
            wall.gameObject.SetActive(true);
            wall.position = new Vector3(0f, level_height * y, 0f);
            wall.localScale = new Vector3(1f, level_height, 1f);

            wall = Instantiate(wall_prefab, this.transform).transform;
            wall.gameObject.SetActive(true);
            wall.position = new Vector3(length, level_height * y, 0f);
            wall.localScale = new Vector3(1f, level_height, 1f);
        }
    }

    void GenerateStairs()
    {
        float offset = 2.2f;
        for (int y = 0; y < level_count; y++)
        {
            foreach (var x in stairs_index)
            {
                Transform prefab = null;
                if (y > 0)
                {
                    prefab = Instantiate(stairs_down_prefab, this.transform).transform;
                    prefab.position = new Vector3(room_length * x + room_length * 0.5f + offset, level_height * y + room_half_height, 0f);
                }

                if (y < level_count - 1)
                {
                    prefab = Instantiate(stairs_up_prefab, this.transform).transform;
                    prefab.position = new Vector3(room_length * x + room_length * 0.5f - offset, level_height * y + room_half_height, 0f);
                }

                prefab = Instantiate(level_num_prefab, this.transform).transform;
                prefab.position = new Vector3(room_length * x + room_length * 0.5f, level_height * y + room_half_height + 1.5f, 0f);
                prefab.GetComponent<FloorNumber>().SetNumber(y);
            }
        }
    }
}
