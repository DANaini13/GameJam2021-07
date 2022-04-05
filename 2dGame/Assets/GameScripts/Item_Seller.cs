using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Seller : Item
{
    public GameObject tip;
    public Text text;
    public override void Interact()
    {
        tip.SetActive(true);
    }

    public override void Exit()
    {
        tip.SetActive(false);
    }
}
