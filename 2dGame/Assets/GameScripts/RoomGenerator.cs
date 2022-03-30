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
    private Vector2Int start_room;

    void Awake()
    {
        GeneratePath(0);
        GenerateRooms();
        GenerateStairs();
    }

    void Start()
    {
        PlayerControl._instance.transform.position = new Vector3(start_room.x * room_length, start_room.y * level_height, 0f);
    }

    [Header("主路径长度")]
    public int path_steps = 12;
    void GeneratePath(int try_times)
    {
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
        start_room = new Vector2Int(nextX, nextY);
        room_data[nextX, nextY] = 9;//标记开始房间

        //开始寻路
        int steps = path_steps;
        int dir = 0;
        int offsetY = 0;
        bool is_climb = false;
        while (steps >= 0)
        {
            //如果刚才爬了楼，一定几率继续爬
            if (is_climb && Random.Range(0, 3) == 0)
            {
                //不是死路，也不是重复房间，继续爬
                if (Is_Room_Exist(nextX, nextY + offsetY) && room_data[nextX, nextY + offsetY] == -1)
                {
                    //根据爬楼梯的方向，定义楼梯间的标记
                    Climb(nextX, nextY, offsetY);
                    //爬完换层，重新定一个前进方向
                    nextY += offsetY;
                    dir = 0;
                    //走了一步
                    steps--;
                    //下一轮寻路
                    continue;
                }
            }
            //关闭爬楼梯标记
            else
                is_climb = false;

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

            steps--;

            //没走到楼梯间，当前房间换成空房间
            if (room_data[nextX, nextY] == 0)
                room_data[nextX, nextY] = 1;
            //走到楼梯间
            else if (room_data[nextX, nextY] == -1)
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
                is_climb = false;
                if (Random.Range(0, count) == 0)
                {
                    //看看能不能爬
                    List<int> list = new List<int>();
                    if (Is_Room_Exist(nextX, nextY + 1) && room_data[nextX, nextY + 1] == -1) list.Add(1);
                    if (Is_Room_Exist(nextX, nextY - 1) && room_data[nextX, nextY - 1] == -1) list.Add(-1);
                    if (list.Count > 0)
                    {
                        //开爬！
                        is_climb = true;
                        offsetY = list[Random.Range(0, list.Count)];
                        Climb(nextX, nextY, offsetY);

                        //爬完换层，重新定一个前进方向
                        nextY += offsetY;
                        dir = 0;

                        //走了两步
                        steps--;
                    }
                }

                //不爬，那当前房间换成毁坏楼梯房
                if (!is_climb)
                    room_data[nextX, nextY] = 111;
            }
        }

        //步长没有走完，说明没找到路径，重试
        if (steps > 0)
            GeneratePath(try_times + 1);
        else
            Debug.Log("找到路径，总尝试次数" + try_times);
    }

    void Climb(int x, int y, int offset)
    {
        //根据爬楼梯的方向，定义楼梯间的标记
        //10-向上传送/100-向下传送/110-可上下
        if (room_data[x, y] == -1) room_data[x, y] = 0;
        room_data[x, y] += offset > 0 ? 10 : 100;
        //爬完之后，定义新房间的标记
        y += offset;
        if (room_data[x, y] == -1) room_data[x, y] = 0;
        room_data[x, y] += offset > 0 ? 100 : 10;
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
                int room_type = room_data[x, y];
                if (room_type < 0) continue;

                Transform prefab = null;
                //可以往下爬
                if (y > 0)
                {
                    if (room_type == 100 || room_type == 110)
                    {
                        prefab = Instantiate(stairs_down_prefab, this.transform).transform;
                        prefab.position = new Vector3(room_length * x + room_length * 0.5f + offset, level_height * y + room_half_height, 0f);
                    }
                }

                //往上
                if (y < level_count - 1)
                {
                    if (room_type == 10 || room_type == 110)
                    {
                        prefab = Instantiate(stairs_up_prefab, this.transform).transform;
                        prefab.position = new Vector3(room_length * x + room_length * 0.5f - offset, level_height * y + room_half_height, 0f);
                    }
                }

                //楼层号
                prefab = Instantiate(level_num_prefab, this.transform).transform;
                prefab.position = new Vector3(room_length * x + room_length * 0.5f, level_height * y + room_half_height + 1.5f, 0f);
                prefab.GetComponent<FloorNumber>().SetNumber(y);
            }
        }
    }
}
