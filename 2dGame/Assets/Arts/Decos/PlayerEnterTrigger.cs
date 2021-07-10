using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnterTrigger : MonoBehaviour
{
    Animator anim;
    public float triggerEnterChance = 100;
    public float triggerExitChance = 100;
    public bool isOnce = false;
    void Awake()
    {
        anim = this.GetComponent<Animator>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Random.Range(0, 100) > triggerEnterChance) return;
        if (other.name != "character") return;

        if (anim)
            anim.SetTrigger("triggerEnter");
        if (isOnce)
            this.GetComponent<Collider2D>().enabled = false;
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (Random.Range(0, 100) > triggerExitChance) return;
        if (other.name != "character") return;
        if (anim)
            anim.SetTrigger("triggerExit");
        if (isOnce)
            this.GetComponent<Collider2D>().enabled = false;
    }
}
