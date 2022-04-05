using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Coin : Item
{
    public AudioSource audio_source;
    public override void Interact()
    {
        AudioSource.PlayClipAtPoint(audio_source.clip, this.transform.position);
        // audio_source.PlayOneShot(audio_source.clip);
        PlayerControl._instance.AddCoin();
        Destroy(this.transform.parent.gameObject);
    }
}
