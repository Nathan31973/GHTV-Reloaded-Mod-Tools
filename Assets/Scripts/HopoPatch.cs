using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HopoPatch : MonoBehaviour
{
    public GameObject LoadingMessage;
    public GameObject MessageBox;
    public GameObject fadeToBlack;

    private void Start()
    {
        DiscordController.instance.UpdateDiscordActivity("Hopo Patch", "Enabling or disabling hopo patch", "mainmenu", "Main Menu");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject a = Instantiate(fadeToBlack);
            a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
            a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
            a.GetComponent<FadeToBlack>().anim.Play();
        }
    }

    public void ApplyPatch()
    {
        hasRPCS3Folder(1);

    }
    public void RemovePatch()
    {
        hasRPCS3Folder(0);
    }

    private void hasRPCS3Folder(int method)
    {
        if (userData.instance.LocalFilePath == "TEST")
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR RPCS3 FOLDER", "RPSC3 FOLDER", false);
            foreach (var path in paths)
            {
                if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
                {
                    Debug.Log($"{path} Is valid");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"{userData.instance.LocalFilePath} Is valid");
                    if(method == 1)
                    {
                        CopyXML("Patch");
                    }
                    else
                    {
                        CopyXML("Restore");
                    }
                }
                else
                {
                    GameObject t = Instantiate(MessageBox);
                    userData.instance.LocalFilePath = "TEST";
                    t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
                    t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
                    if (method == 1)
                    {
                        t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ApplyPatch);
                    }
                    else
                    {
                        t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(RemovePatch);
                    }
                }
            }
        }
        else if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
        {
            if (method == 1)
            {
                CopyXML("Patch");
            }
            else
            {
                CopyXML("Restore");
            }
        }
        else
        {
            GameObject t = Instantiate(MessageBox);
            userData.instance.LocalFilePath = "TEST";
            t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
            t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
            if (method == 1)
            {
                t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ApplyPatch);
            }
            else
            {
                t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(RemovePatch);
            }
        }
    }
    private void CopyXML(string type)
    {
        if(type == "Patch")
        {
            CheckPath();
            File.Copy($"{Application.streamingAssetsPath}/HOPO_FIX/HopoFix.xml", $"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/CONFIGS/HUD/GUITAR/CONFIGHUDCOMMON.XML", true);
            File.Copy($"{Application.streamingAssetsPath}/HOPO_FIX/HopoFix.xml", $"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/CONFIGS/HUD/GUITAR/CONFIGHUDCOMMON.XML", true);
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = "Patch Complete";
            t.GetComponent<GUI_MessageBox>().message = $"We have successfully apply the patch\n\nPlease restart your game";
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        }   
        else if(type == "Restore")
        {
            CheckPath();
            File.Copy($"{Application.streamingAssetsPath}/HOPO_FIX/OgHopo.xml", $"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/CONFIGS/HUD/GUITAR/CONFIGHUDCOMMON.XML", true);
            File.Copy($"{Application.streamingAssetsPath}/HOPO_FIX/OgHopo.xml", $"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/CONFIGS/HUD/GUITAR/CONFIGHUDCOMMON.XML", true);
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = "Restore Complete";
            t.GetComponent<GUI_MessageBox>().message = $"We have restore/remove the HOPO Patch\n\nPlease restart your game";
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        }
        else
        {
            Debug.LogError($"[Hopo Pathc] A unknow type of {type}");
        }
    }
    private void CheckPath()
    {
        if(!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/CONFIGS/HUD/GUITAR") | !Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/CONFIGS/HUD/GUITAR"))
        {
            Debug.Log("[HOPO Patch] Creating dir");
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/CONFIGS/HUD/GUITAR");
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/CONFIGS/HUD/GUITAR");
        }
        else
        {
            Debug.Log("[HOPO Patch] Game path dir has already been made");
        }
    }

    public void ReturnToMainMenu()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }
}
