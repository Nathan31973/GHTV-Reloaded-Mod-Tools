using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject fadeToBlack;
    public GameObject Settings;
    public GameObject CommuityPatches;
    public void Start()
    {
        gitHubManager.instance.GetVersion();
        DiscordController.instance.UpdateDiscordActivity("Main Menu", "Selecting a tool", "mainmenu", "Main Menu");
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    public void GOTOCOLOURCHAGER()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "ColourChanger";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }
    public void GOTOHYPERSPEEDCHANGER()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Hyper Speed";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }
    public void GOTOGEMCHANGER()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Gem Changer";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

    public void OPENSETTINGS()
    {
        Instantiate(Settings);
    }
    public void OPENCOMMUITYPATCH()
    {
        Instantiate(CommuityPatches);
    }
}
