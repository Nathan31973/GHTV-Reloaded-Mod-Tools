using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenPannel : MonoBehaviour
{
    public float tweentime = 0.6f;
    // Start is called before the first frame update
    void OnEnable()
    {
        LeanTween.rotateX(gameObject, 0f, tweentime)
            .setEaseInOutSine();
    }

    public void Dissable()
    {
        LeanTween.rotateX(gameObject, 90f, tweentime)
            .setEaseInOutSine(); 
        StartCoroutine(turnoff());
    }
    IEnumerator turnoff()
    {
        while(LeanTween.isTweening(gameObject))
        {
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
        
    }
}
