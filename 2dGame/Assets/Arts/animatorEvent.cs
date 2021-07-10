using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animatorEvent : MonoBehaviour
{
    public void DestroyThis()
    {
        Destroy(this.transform.parent.gameObject);
    }

    public AudioClip clip;
    public void PlaySnd()
    {
        Transform prefab = new GameObject().transform;
        prefab.position = this.transform.position;
        AudioSource audio = prefab.gameObject.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.spatialBlend = 0.85f;
        audio.loop = false;
        audio.Play();
        Destroy(audio.gameObject, 2f);
    }

    public Collider2D colid, colid2, colid3;
    public void CancleScary()
    {
        if (colid)
            colid.enabled = false;
        if (colid2)
            colid2.enabled = false;
        if (colid3)
            colid3.enabled = false;
    }
}
