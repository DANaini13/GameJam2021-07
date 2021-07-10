using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFellower : MonoBehaviour
{
    public Transform fellowing_obj;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = fellowing_obj.position + offset;
    }
}
