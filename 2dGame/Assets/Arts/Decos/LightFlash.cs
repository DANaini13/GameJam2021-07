using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlash : MonoBehaviour
{
    public float frequency = 1;
    public float chance = 10;
    float timer;
    Animator anim;

    void Awake()
    {
        anim = this.transform.GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= frequency)
        {
            timer = 0f;
            if (Random.Range(0, 100) < chance && anim) anim.SetTrigger("flash");
        }
    }
}
