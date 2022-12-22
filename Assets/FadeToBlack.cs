using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeToBlack: MonoBehaviour
{
    public bool destory = false;
    public bool changelevel = false;
    public string levelToChangeScene;
    public AnimationClip[] animClip;
    public Animation anim;

    private void Start()
    {
        anim = gameObject.GetComponent<Animation>();
    }
    // Update is called once per frame
    void Update()
    {
        if(destory)
        {
            Destroy(gameObject);
        }
        else if(changelevel)
        {
            try
            {
                SceneManager.LoadScene(levelToChangeScene);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}
