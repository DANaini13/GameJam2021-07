using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorNumber : MonoBehaviour
{
    public Sprite[] numbers;
    public SpriteRenderer num_renderer;
    public SpriteRenderer num_2nd_renderer;

    public void SetNumber(int i)
    {
        if (i >= numbers.Length) i = numbers.Length - 1;
        else if (i < 0) i = 0;
        num_renderer.sprite = numbers[i];
    }

    public void Set2ndNumber(int i)
    {
        if (i >= numbers.Length) i = numbers.Length - 1;
        else if (i < 0) i = 0;
        num_2nd_renderer.sprite = numbers[i];
    }
}
