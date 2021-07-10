using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image filled_image;

    public void SetProgress(float progress)
    {
        filled_image.fillAmount = progress;
    }
}
