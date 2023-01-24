using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StreakPatch : MonoBehaviour
{
    public GameObject LoadingMessage;
    public GameObject MessageBox;
    public GameObject fadeToBlack;
    private Translater T;
    private void Start()
    {
        T = Translater.instance;
        DiscordController.instance.UpdateDiscordActivity("No Streak Patch", "Enabling or disabling Streak patch", "mainmenu", "Main Menu");
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
            var paths = StandaloneFileBrowser.OpenFolderPanel(T.getText("MSG_SEL_RPCS3"), T.getText("MGS_RPCS3_FOLD"), false);
            foreach (var path in paths)
            {
                if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
                {
                    Debug.Log($"[Streak Patch] {path} Is valid");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"[Streak Patch] {userData.instance.LocalFilePath} Is valid");
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
                    t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
                    t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
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
            t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
            t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
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
        string region = "";

        ////for updating from older version of the tool
        if (userData.instance.gameVersion == userData.Version.PAL || userData.instance.gameVersion == userData.Version.Lite)
        {
            region = "BLES02180";
        }
        else if (userData.instance.gameVersion == userData.Version.USA)
        {
            region = "BLUS31556";
        }
        else
        {
            Debug.LogError("[Streak Patch] USER HASN'T SELECTED A GAME REGION");
            return;
        }

        if (type == "Patch")
        {
            CheckPath();
            File.Copy($"{Application.streamingAssetsPath}/STREAK_FIX/StreakPatch.xml", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/CONFIGS/CONFIG_STREAK.XML", true);
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = T.getText("COM_COMMON_COMPLETE");
            t.GetComponent<GUI_MessageBox>().message = T.getText("COM_COMMON_DES_COMPLETE");
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        }
        else if (type == "Restore")
        {
            CheckPath();
            File.Copy($"{Application.streamingAssetsPath}/STREAK_FIX/StreakDefault.xml", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/CONFIGS/CONFIG_STREAK.XML", true);
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = T.getText("COM_COMMON_RESTORE");
            t.GetComponent<GUI_MessageBox>().message = T.getText("COM_COMMON_DES_RESTORE");
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        }
        else
        {
            Debug.LogError($"[Streak Patch] A unknow type of {type}");
        }
    }
    private void CheckPath()
    {
        string region = "";

        ////for updating from older version of the tool
        if (userData.instance.gameVersion == userData.Version.PAL || userData.instance.gameVersion == userData.Version.Lite)
        {
            region = "BLES02180";
        }
        else if (userData.instance.gameVersion == userData.Version.USA)
        {
            region = "BLUS31556";
        }
        else
        {
            Debug.LogError("[Streak Patch] USER HASN'T SELECTED A GAME REGION");
            return;
        }
        if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/CONFIGS"))
        {
            Debug.Log("[Streak Patch] Creating dir");
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/CONFIGS");
        }
        else
        {
            Debug.Log("[Streak Patch] Game path dir has already been made");
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
