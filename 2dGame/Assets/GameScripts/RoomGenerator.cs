using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator _instance;
    [Header("一共有多少层")]
    public int level_count = 4;
    [Header("每层多少个房间")]
    public static int room_count = 9;
    [Header("每层的第几个房间是楼梯")]
    public int[] stairs_index = new int[3] { 1, 4, 7 };
    [Header("每层楼的高度")]
    public static float level_height = 15.0f;
    [Header("室内场景偏移")]
    public static float classroom_offset = 200.0f;
    [Header("房间长度")]
    public static float room_length = 30.0f;
    [Header("房间半高，用来放地板")]
    public float room_half_height = 4.5f;
    [Header("岔路最多扩展几步")]
    public int expand_steps = 3;
    public string seed_input;
    private int seed;

    public Transform room_empty;
    public Transform room_stairs;
    public Transform[] room_normal_prefabs;
    public int classroom_count;
    public Transform[] room_door_prefabs;
    public Transform[] room_classroom_prefabs;
    public Transform room_book_prefab;
    public Transform room_start;
    public Transform room_end;
    public Transform room_lantern;
    public Transform room_ghost;
    public Transform stairs_down_prefab;
    public Transform stairs_down_block_prefab;
    public Transform stairs_up_prefab;
    public Transform stairs_up_block_prefab;
    public Transform level_num_prefab;
    public Transform obstacle_prefab;
    public Transform ground_prefab;
    public Transform wall_prefab;

    private int[,] room_data;

    void Awake()
    {
        _instance = this;
        if (seed_input.Equals(string.Empty)) seed_input = System.DateTime.Now.ToString();
        seed = seed_input.GetHashCode();
        Random.InitState(seed);

        GeneratePath(0);
        GenerateClassroom();
        GenerateSpecialRooms();
        List<Vector2Int> list = GenerateRandomRoom(path_list);
        for (int i = 0; i < expand_steps - 1; i++)
            list = GenerateRandomRoom(list);
        InstantiateRooms();
        InstantiateStairs();
    }

    void Start()
    {
        //玩家传送到起始点
        int x = path_list[0].x;
        int y = path_list[0].y;
        PlayerControl._instance.transform.position = new Vector3(x * room_length + room_length * 0.5f, y * level_height, 0f);
        //怪物传送到终点
        x = path_list[path_list.Count - 1].x;
        y = path_list[path_list.Count - 1].y;
        MonsterAI._instance.transform.position = new Vector3(x * room_length + room_length * 0.5f, y * level_height, 0f);
    }

    [Header("主路径长度")]
    public int path_steps = 12;
    List<Vector2Int> path_list;
    void GeneratePath(int try_times)
    {
        if (try_times >= 2000)
        {
            Debug.Log("找不到，不找了");
            return;
        }

        path_list = new List<Vector2Int>();
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
        room_data[nextX, nextY] = 9;//标记开始房间
        path_list.Add(new Vector2Int(nextX, nextY));

        //开始寻路
        int steps = path_steps;
        int dir = 0;
        int offsetY = 0;
        bool is_climb = false;
        while (steps > 0)
        {
            //如果刚才爬了楼，一定几率继续爬
            if (is_climb && Random.Range(0, 3) == 0)
            {
                //下一层存在，也没走重复路
                if (Is_Room_Exist(nextX, nextY + offsetY) && room_data[nextX, nextY + offsetY] == -1)
                {
                    //下一层楼梯间不能与主路径接壤
                    if (!Is_Room_Exist(nextX - 1, nextY + offsetY) || !Is_Room_Occupied(nextX - 1, nextY + offsetY))
                    {
                        if (!Is_Room_Exist(nextX + 1, nextY + offsetY) || !Is_Room_Occupied(nextX + 1, nextY + offsetY))
                        {
                            //根据爬楼梯的方向，定义楼梯间的标记
                            Climb(nextX, nextY, offsetY);
                            //爬完换层，重新定一个前进方向
                            nextY += offsetY;
                            dir = 0;
                            // //走了一步
                            // steps--;
                            // //终点不能是楼梯间
                            // if (steps <= 0)
                            //     steps = 1;
                            //下一轮寻路
                            continue;
                        }
                    }
                }
            }
            //关闭爬楼梯标记
            else
                is_climb = false;

            //到达新楼层时，决定一个前进方向
            if (dir == 0) dir = Random.Range(0, 2) == 0 ? 1 : -1;

            //=====前进
            nextX += dir;
            //走到死路，退出
            if (!Is_Room_Exist(nextX, nextY))
                break;
            //重复路，退出
            if (Is_Room_Occupied(nextX, nextY))
                break;
            //下一步会与主路径接壤，重试
            if (Is_Room_Exist(nextX + dir, nextY) && Is_Room_Occupied(nextX + dir, nextY))
                break;

            steps--;

            //没走到楼梯间，当前房间换成空房间
            if (room_data[nextX, nextY] == 0)
            {
                room_data[nextX, nextY] = 1;
                path_list.Add(new Vector2Int(nextX, nextY));
            }
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
                //对几率进行补正，不然容易到尽头才换层
                if (Random.Range(0, count + 1) <= 1)
                {
                    //看看能不能爬
                    List<int> list = new List<int>();
                    if (Is_Room_Exist(nextX, nextY + 1) && room_data[nextX, nextY + 1] == -1)
                    {
                        //下一层楼梯间不能跟主路径接壤
                        if (Is_Room_Exist(nextX - 1, nextY + 1) && Is_Room_Occupied(nextX - 1, nextY + 1))
                            break;
                        if (Is_Room_Exist(nextX + 1, nextY + 1) && Is_Room_Occupied(nextX + 1, nextY + 1))
                            break;
                        else
                            list.Add(1);
                    }
                    if (Is_Room_Exist(nextX, nextY - 1) && room_data[nextX, nextY - 1] == -1)
                    {

                        //下一层楼梯间不能跟主路径接壤
                        if (Is_Room_Exist(nextX - 1, nextY - 1) && Is_Room_Occupied(nextX - 1, nextY - 1))
                            break;
                        if (Is_Room_Exist(nextX + 1, nextY - 1) && Is_Room_Occupied(nextX + 1, nextY - 1))
                            break;
                        else
                            list.Add(-1);
                    }
                    if (list.Count > 0)
                    {
                        //开爬！
                        is_climb = true;
                        offsetY = list[Random.Range(0, list.Count)];
                        Climb(nextX, nextY, offsetY);

                        //爬完换层，重新定一个前进方向
                        nextY += offsetY;
                        dir = 0;

                        // //走了一步
                        // steps--;

                        //终点不能是楼梯间
                        if (steps <= 0)
                            steps = 1;
                    }
                }

                //不爬，那当前房间换成毁坏楼梯房
                if (!is_climb)
                {
                    room_data[nextX, nextY] = 111;
                    path_list.Add(new Vector2Int(nextX, nextY));
                }
            }
        }

        //=========寻路结束了
        //步长没有走完，说明没找到路径，重试
        if (steps > 0)
            GeneratePath(try_times + 1);
        //========步长走完了
        else
        {
            //算一下空房间数够不够
            int count = 0;
            foreach (var room in path_list)
                if (room_data[room.x, room.y] == 1)
                    count++;

            //房间数不够，继续重试（加特殊房，和终点房）
            if (count < classroom_count + 2 + 1)
                GeneratePath(try_times + 1);
            //标记结束房间
            else
            {
                Vector2Int end_index = path_list[path_list.Count - 1];
                room_data[end_index.x, end_index.y] = 6;
                Debug.Log("找到路径，总尝试次数" + try_times);
            }
        }
    }

    void Climb(int x, int y, int offset)
    {
        //根据爬楼梯的方向，定义楼梯间的标记
        //10-向上传送/100-向下传送/110-可上下
        if (room_data[x, y] == -1) room_data[x, y] = 0;
        room_data[x, y] += offset > 0 ? 10 : 100;
        path_list.Add(new Vector2Int(x, y));
        //爬完之后，定义新房间的标记
        y += offset;
        if (room_data[x, y] == -1) room_data[x, y] = 0;
        room_data[x, y] += offset > 0 ? 100 : 10;
        path_list.Add(new Vector2Int(x, y));
    }

    bool Is_Room_Exist(int x, int y)
    {
        if (x < 0 || x >= room_count || y < 0 || y >= level_count)
            return false;
        return true;
    }

    bool Is_Room_Occupied(int x, int y)
    {
        if (room_data[x, y] > 0)
            return true;
        return false;
    }

    public Vector2Int book_room_index;
    public Vector2Int book_room_classnum;
    void GenerateClassroom()
    {
        //设置一个书本房
        book_room_index = SetNormalRoomTo(4);
        //普通教室
        for (int i = 0; i < classroom_count - 1; i++)
            SetNormalRoomTo(2);
    }

    void GenerateSpecialRooms()
    {
        SetNormalRoomTo(8);
        SetNormalRoomTo(7);
    }

    Vector2Int SetNormalRoomTo(int type)
    {
        int x = 0, y = 0;

        //复制一份list
        List<Vector2Int> list = new List<Vector2Int>();
        foreach (var room in path_list)
            list.Add(room);

        //随机添加进新的list
        List<Vector2Int> random_list = new List<Vector2Int>();
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(0, list.Count);
            random_list.Add(list[r]);
            list.RemoveAt(r);
            i--;
        }

        foreach (var room in random_list)
        {
            //找到第一个空房间
            x = room.x;
            y = room.y;
            if (room_data[x, y] == 1)
            {
                room_data[x, y] = type;
                return new Vector2Int(x, y);
            }
        }

        Debug.LogError("找不到多余的空房间了");
        return new Vector2Int(-1, -1);
    }

    List<Vector2Int> GenerateRandomRoom(List<Vector2Int> list)
    {
        var rand = 2;
        List<Vector2Int> rooms = new List<Vector2Int>();
        if (list.Count == 0) return rooms;
        //遍历所有房间，一定几率朝左右上下扩展
        foreach (var v2 in list)
        {
            if (Random.Range(0, rand) != 0) continue;

            //左
            if (Is_Room_Exist(v2.x - 1, v2.y) && !Is_Room_Occupied(v2.x - 1, v2.y))
            {
                //如果是楼梯间，则换成损坏楼梯房
                if (room_data[v2.x - 1, v2.y] == -1)
                {
                    room_data[v2.x - 1, v2.y] = 111;
                    continue;
                }
                //一定概率替换成教室房间
                int type = Random.Range(0, 3) == 0 ? 3 : 1;
                room_data[v2.x - 1, v2.y] = type;
                rooms.Add(new Vector2Int(v2.x - 1, v2.y));
                continue;
            }
            //右
            if (Is_Room_Exist(v2.x + 1, v2.y) && !Is_Room_Occupied(v2.x + 1, v2.y))
            {
                //如果是楼梯间，则换成损坏楼梯房
                if (room_data[v2.x + 1, v2.y] == -1)
                {
                    room_data[v2.x + 1, v2.y] = 111;
                    continue;
                }
                //一定概率替换成教室房间
                int type = Random.Range(0, 3) == 0 ? 3 : 1;
                room_data[v2.x + 1, v2.y] = type;
                rooms.Add(new Vector2Int(v2.x + 1, v2.y));
                continue;
            }
            //楼梯间
            if (room_data[v2.x, v2.y] == -1 || room_data[v2.x, v2.y] >= 10)
            {
                //上面的房间存在
                if (Is_Room_Exist(v2.x, v2.y + 1))
                {
                    //当前房间本来是不可以往上的
                    if (room_data[v2.x, v2.y] != 10 && room_data[v2.x, v2.y] != 110)
                    {
                        if (room_data[v2.x, v2.y] == -1 || room_data[v2.x, v2.y] == 111)
                            room_data[v2.x, v2.y] = 10;
                        else
                            room_data[v2.x, v2.y] += 10;

                        if (room_data[v2.x, v2.y + 1] == -1 || room_data[v2.x, v2.y + 1] == 111)
                            room_data[v2.x, v2.y + 1] = 100;
                        else
                            room_data[v2.x, v2.y + 1] += 100;

                        rooms.Add(new Vector2Int(v2.x, v2.y + 1));
                        continue;
                    }

                }
                //下
                //下面的房间存在
                if (Is_Room_Exist(v2.x, v2.y - 1))
                {
                    //当前房间本来是不可以往下的
                    if (room_data[v2.x, v2.y] != 100 && room_data[v2.x, v2.y] != 110)
                    {
                        if (room_data[v2.x, v2.y] == -1 || room_data[v2.x, v2.y] == 111)
                            room_data[v2.x, v2.y] = 100;
                        else
                            room_data[v2.x, v2.y] += 100;

                        if (room_data[v2.x, v2.y - 1] == -1 || room_data[v2.x, v2.y - 1] == 111)
                            room_data[v2.x, v2.y - 1] = 10;
                        else
                            room_data[v2.x, v2.y - 1] += 10;

                        rooms.Add(new Vector2Int(v2.x, v2.y - 1));
                        continue;
                    }

                }
            }
        }
        return rooms;
    }

    void InstantiateRooms()
    {
        for (int y = 0; y < level_count; y++)
        {
            int class_num = 0;
            for (int x = 0; x < room_count; x++)
            {
                var room_type = room_data[x, y];
                if (room_type <= 0)
                {
                    var obstacle = Instantiate(obstacle_prefab, this.transform).transform;
                    obstacle.position = new Vector3(room_length * x + room_length * 0.5f - room_length * 0.5f, level_height * y + room_half_height, 0f);
                    obstacle = Instantiate(obstacle_prefab, this.transform).transform;
                    obstacle.position = new Vector3(room_length * x + room_length * 0.5f + room_length * 0.5f, level_height * y + room_half_height, 0f);
                }
                else
                {
                    int r = Random.Range(0, room_normal_prefabs.Length);
                    var prefab = room_normal_prefabs[r];
                    //教室
                    if (room_type == 2 || room_type == 3 || room_type == 4)
                    {
                        //生成室内场景
                        var rprefab = room_classroom_prefabs[Random.Range(0, room_classroom_prefabs.Length)];
                        if (room_type == 4)
                            rprefab = room_book_prefab;
                        var classroom = Instantiate(rprefab, this.transform).transform;
                        classroom.position = new Vector3(room_length * x + room_length * 0.5f, level_height * y + room_half_height + classroom_offset, 0f);
                        prefab = room_door_prefabs[Random.Range(0, room_door_prefabs.Length)];
                        //设置门牌
                        var numbers = prefab.GetComponentsInChildren<FloorNumber>();
                        foreach (var number in numbers)
                        {
                            number.SetNumber(y);
                            number.Set2ndNumber(class_num);
                        }
                        if (room_type == 4)
                            book_room_classnum = new Vector2Int(class_num, y);
                        class_num++;
                    }
                    //起始房间
                    else if (room_type == 9)
                        prefab = room_start;
                    //终点
                    else if (room_type == 6)
                        prefab = room_end;
                    //幽灵房
                    else if (room_type == 8)
                        prefab = room_ghost;
                    //灯笼房
                    else if (room_type == 7)
                        prefab = room_lantern;
                    //电梯房
                    else if (room_type >= 10)
                        prefab = room_stairs;
                    var room = Instantiate(prefab, this.transform).transform;
                    room.position = new Vector3(room_length * x + room_length * 0.5f, level_height * y + room_half_height, 0f);
                }
            }
            //普通楼层碰撞
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

            //教室楼层碰撞
            ground = Instantiate(ground_prefab, this.transform).transform;
            ground.gameObject.SetActive(true);
            length = room_count * room_length;
            ground.position = new Vector3(length * 0.5f, level_height * y + classroom_offset, 0f);
            ground.localScale = new Vector3(length, 1f, 1f);

            wall = Instantiate(wall_prefab, this.transform).transform;
            wall.gameObject.SetActive(true);
            wall.position = new Vector3(0f, level_height * y + classroom_offset, 0f);
            wall.localScale = new Vector3(1f, level_height, 1f);

            wall = Instantiate(wall_prefab, this.transform).transform;
            wall.gameObject.SetActive(true);
            wall.position = new Vector3(length, level_height * y + classroom_offset, 0f);
            wall.localScale = new Vector3(1f, level_height, 1f);
        }
    }

    void InstantiateStairs()
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
                    var p = stairs_down_block_prefab;
                    if (room_type == 100 || room_type == 110)
                        p = stairs_down_prefab;
                    prefab = Instantiate(p, this.transform).transform;
                    prefab.position = new Vector3(room_length * x + room_length * 0.5f + offset, level_height * y + room_half_height, 0f);
                    if (room_type == 100 || room_type == 110)
                        prefab.Find("TransGate").gameObject.AddComponent<Stairs>().is_up = false;
                }

                //往上
                if (y < level_count - 1)
                {
                    var p = stairs_up_block_prefab;
                    if (room_type == 10 || room_type == 110)
                        p = stairs_up_prefab;
                    prefab = Instantiate(p, this.transform).transform;
                    prefab.position = new Vector3(room_length * x + room_length * 0.5f - offset, level_height * y + room_half_height, 0f);
                    if (room_type == 10 || room_type == 110)
                        prefab.Find("TransGate").gameObject.AddComponent<Stairs>().is_up = true;
                }

                //楼层号
                prefab = Instantiate(level_num_prefab, this.transform).transform;
                prefab.position = new Vector3(room_length * x + room_length * 0.5f, level_height * y + room_half_height + 1.5f, 0f);
                prefab.GetComponent<FloorNumber>().SetNumber(y);
            }
        }
    }

    public bool HasRoomByPos(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / room_length);
        int y = Mathf.FloorToInt(pos.y / level_height);
        if (!Is_Room_Exist(x, y)) return false;
        if (room_data[x, y] <= 0) return false;
        return true;
    }

    public Vector2Int GetEndRoomIndex()
    {
        Vector2Int v2 = path_list[path_list.Count - 1];
        return v2;
    }
}
