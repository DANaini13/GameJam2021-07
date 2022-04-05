using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Tips : Item
{
    public GameObject tip;
    public Text text;
    private bool is_interacted;
    public override void Interact()
    {
        if (!is_interacted)
        {
            Vector2Int v2 = RoomGenerator._instance.GetEndRoomIndex();
            int count = RoomGenerator.room_count;
            int level = RoomGenerator._instance.level_count;

            Vector2Int book_v2 = RoomGenerator._instance.book_room_index;
            Vector2Int book_class_v2 = RoomGenerator._instance.book_room_classnum;

            //先获得书本的线索
            if (PlayerControl._instance.clue_num_book <= 2)
            {
                PlayerControl._instance.clue_num_book++;
                if (PlayerControl._instance.clue_num_book <= 1)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        if (book_v2.x < count / 3)
                            text.text = ("书本在某楼层偏左的位置");
                        else if (book_v2.x >= (count / 3) * 2)
                            text.text = ("书本在某楼层偏右的位置");
                        else
                            text.text = ("书本在某楼层偏中间的位置");
                    }
                    else
                    {
                        if (book_v2.y < level / 2)
                            text.text = ("书本在较低的楼层");
                        else if (book_v2.y >= (level / 2) * 2)
                            text.text = ("书本在较高的楼层");
                        else
                            text.text = ("书本在中间楼层");
                    }
                }
                else
                    text.text = ("书本在" + book_class_v2.y + "楼第" + book_class_v2.x + "间教室");
            }
            //否则提示终点位置
            else
            {
                PlayerControl._instance.clue_num_end++;
                if (PlayerControl._instance.clue_num_end <= 1)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        if (v2.x < count / 3)
                            text.text = ("终点在某楼层偏左的位置");
                        else if (v2.x >= (count / 3) * 2)
                            text.text = ("终点在某楼层偏右的位置");
                        else
                            text.text = ("终点在某楼层偏中间的位置");
                    }
                    else
                    {
                        if (v2.y < level / 2)
                            text.text = ("终点在较低的楼层");
                        else if (v2.y >= (level / 2) * 2)
                            text.text = ("终点在较高的楼层");
                        else
                            text.text = ("终点在中间楼层");
                    }
                }
                else
                {
                    text.text = ("终点在" + (v2.y + 1) + "楼");
                    if (v2.x < count / 3)
                        text.text += ("偏左的位置");
                    else if (v2.x >= (count / 3) * 2)
                        text.text += ("偏右的位置");
                    else
                        text.text += ("偏中间的位置");
                }
            }
            is_interacted = true;
        }
        tip.SetActive(true);
    }

    public override void Exit()
    {
        tip.SetActive(false);
    }
}
