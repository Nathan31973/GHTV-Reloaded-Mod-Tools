using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PlayIntro : MonoBehaviour
{
    private VideoPlayer player;
    private bool preCache;
    private void Start()
    {
        player = GetComponent<VideoPlayer>();
        player.Prepare();
    }
    private void Update()
    {
        if(player.isPrepared & !preCache)
        {
            preCache = true;
            player.Play();
        }
        if(!player.isPlaying & preCache)
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
