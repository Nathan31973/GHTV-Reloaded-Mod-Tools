using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GemPackWelcome : MonoBehaviour
{
    public GemChanger gemChanger;
    public Button Creator;
    public GemChanger_Importer Importer;
    public GameObject ImporterUI;
    public GameObject CreatorUI;
    public GameObject MessageBox2;
    public Button Reset;
    private void Start()
    {
        ImporterUI.SetActive(false);
        CreatorUI.SetActive(false);
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) // || Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor
        {
            Creator.interactable = true;
        }
        else
        {
            Creator.interactable = false;
        }
        if (userData.instance.LocalFilePath != "TEST")
        {
            Reset.interactable = true;
        }
    }
    public void openImporter()
    {
        gemChanger.main();
        ImporterUI.SetActive(true);
        Importer.GemChangerImporterStart();
    }
    public void openCreator()
    {
        GameObject a = Instantiate(gemChanger.messageBox);
        a.GetComponent<GUI_MessageBox>().title = "Python Required";
        a.GetComponent<GUI_MessageBox>().message = "You need to have Python 3.1 installed or you won't be able to convert textures.";
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(gemChanger.pythonNotices);
        CreatorUI.SetActive(true);

    }

    public void returntomainMenu()
    {
        GameObject a = Instantiate(gemChanger.fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }
    
    public void resetgems()
    {
        GameObject a = Instantiate(MessageBox2);
        a.GetComponent<GUI_MessageBox>().title = "Reset Gems?";
        a.GetComponent<GUI_MessageBox>().message = "Are you sure you want to remove any custom gems that stored in your game?";
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(doreset);
    }
    public void doreset()
    {
        List<string> GameVersion = new List<string>();
        GameVersion.Add("BLES02180");
        GameVersion.Add("BLUS31556");
        //removing the old gems if placed
        foreach (string version in GameVersion)
        {
            if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS"))
            {
                Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS", true);
            }
        }
        GameObject a = Instantiate(gemChanger.messageBox);
        a.GetComponent<GUI_MessageBox>().title = "Reset Complete";
        a.GetComponent<GUI_MessageBox>().message = "Customs gems have been removed from your game";
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(gemChanger.pythonNotices);
    }    
}
