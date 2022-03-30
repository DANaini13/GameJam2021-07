using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorNumber : MonoBehaviour
{
    public Sprite[] numbers;
    public SpriteRenderer num_renderer;

    public void SetNumber(int i)
    {
        num_renderer.sprite = numbers[i];
    }
}
