using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnterTrigger : MonoBehaviour
{
    Animator anim;
    public float triggerEnterChance = 100;
    public float triggerExitChance = 100;
    void Awake()
    {
        anim = this.GetComponent<Animator>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(Random.Range(0,100)>triggerEnterChance)return;

        if (anim)
            anim.SetTrigger("triggerEnter");
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(Random.Range(0,100)>triggerExitChance)return;
        if (anim)
            anim.SetTrigger("triggerExit");
    }
}
