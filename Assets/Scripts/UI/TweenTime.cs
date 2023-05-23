using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenTime : MonoBehaviour
{
    public Image image;
    public float tweenTime;


    public void ImageFill()
    {

            LeanTween.value(gameObject, 0, 1, tweenTime)
                .setEaseInSine()
                .setOnUpdate((value) =>
                {
                    image.fillAmount = value;
                });
        
    }

    public void ImageUnFill()
    {

            LeanTween.value(gameObject, 1, 0, tweenTime)
                .setEaseInSine()
                .setOnUpdate((value) =>
                {
                    image.fillAmount = value;
                });
        
    }
}
