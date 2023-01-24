using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RestartButtonPatch : MonoBehaviour
{
    public GameObject LoadingMessage;
    public GameObject MessageBox;
    public GameObject TutVideo;
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
            File.Copy($"{Application.streamingAssetsPath}/RESTART_FIX/ONDEMANDPAUSEDIALOG.XML", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/DIALOGS/GHTV/ONDEMAND/ONDEMANDPAUSEDIALOG.XML", true);
            File.Copy($"{Application.streamingAssetsPath}/RESTART_FIX/PAUSEMENU.XML", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/GHTV/PAUSE/MENUS/ONDEMAND/PAUSEMENU.XML", true);
            GameObject t = Instantiate(TutVideo);
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu2);
        }
        else if (type == "Restore")
        {
            if(Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/GHTV/PAUSE/MENUS/ONDEMAND"))
            {
                Debug.Log("[RestartButtonPatch] /UI/GHTV/PAUSE/MENUS/ONDEMAND dir deleted");
                Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/GHTV/PAUSE/MENUS/ONDEMAND", true);
            }
            if(Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/DIALOGS/GHTV/ONDEMAND"))
            {
                Debug.Log("[RestartButtonPatch] /UI/DIALOGS/GHTV/ONDEMAND dir deleted");
                Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/DIALOGS/GHTV/ONDEMAND", true);
            }
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
        if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/DIALOGS/GHTV/ONDEMAND"))
        {
            Debug.Log("[Streak Patch] Creating dir");
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/DIALOGS/GHTV/ONDEMAND");
        }
        else
        {
            Debug.Log("[Streak Patch] Game path dir has already been made");
        }
        if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/GHTV/PAUSE/MENUS/ONDEMAND"))
        {
            Debug.Log("[Streak Patch] Creating dir");
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/GHTV/PAUSE/MENUS/ONDEMAND");
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

    public void ReturnToMainMenu2()
    {
        StartCoroutine(delayMainmenu());
    }

    private IEnumerator delayMainmenu()
    {
        yield return new WaitForSeconds(1.2f);
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }
}
