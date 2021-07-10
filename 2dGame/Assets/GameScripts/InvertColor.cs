using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertColor : MonoBehaviour
{
    public Material material;
    public bool active = false;

    void OnRenderImage(RenderTexture source, RenderTexture target)
    {
        if (material != null && active)
        {
            Graphics.Blit(source, target, material);
        }
        else
        {
            Graphics.Blit(source, target);
        }
    }
}
