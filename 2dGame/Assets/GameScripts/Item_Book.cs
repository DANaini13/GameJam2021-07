using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Book : Item
{
    public AudioSource audio_source;
    public override void Interact()
    {
        AudioSource.PlayClipAtPoint(audio_source.clip, this.transform.position);
        // audio_source.PlayOneShot(audio_source.clip);
        PlayerControl._instance.GetBook();
        Destroy(this.transform.parent.gameObject);
    }
}
