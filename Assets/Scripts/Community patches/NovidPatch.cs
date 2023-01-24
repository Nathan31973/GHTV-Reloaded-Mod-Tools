using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using UnityEngine;
using YamlDotNet.RepresentationModel;

public class NovidPatch : MonoBehaviour
{
    // Start is called before the first frame update

    private WebClient webClient = null;
    public GameObject MessageBox;
    public GameObject fadeToBlack;
    public GameObject LoadingBox;
    private GameObject load;
    private Translater T;
    private void Start()
    {
        T = Translater.instance;
        DiscordController.instance.UpdateDiscordActivity("No Video Patch", "Enabling or disabling no video patch", "mainmenu", "Main Menu");
        //downloaded latest gameversion
        //checking if existing file there
        if (!File.Exists($"{Application.persistentDataPath}/RPCS3 Patch/imported_patch.yml"))
        {
            btnDownload_Click("http://ghtv.tool.novideopatch.stickgaming.net/", $"{Application.persistentDataPath}/RPCS3 Patch/imported_patch.yml");
        }
        
        
    }

    private void btnDownload_Click(string url, string filename)
    {
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[NovidPatch] Error. Check internet connection!");
        }
        else
        {
            Debug.Log("[NovidPatch] We have internet");
            // Is file downloading yet?
            if (webClient != null)
            {
                return;
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            if(!Directory.Exists($"{Application.persistentDataPath}/RPCS3 Patch"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/RPCS3 Patch");
            }
            //checking if existing file there
            if(File.Exists($"{Application.persistentDataPath}/RPCS3 Patch/imported_patch.yml"))
            {
                File.Delete($"{Application.persistentDataPath}/RPCS3 Patch/imported_patch.yml");
            }
            webClient = new WebClient();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver);
            webClient.DownloadFileAsync(new Uri($"{url}"), filename);


            webClient = new WebClient();
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            myHttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
            myHttpWebRequest.AllowAutoRedirect = true;
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            Debug.Log(myHttpWebResponse.ResponseUri);

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver);
            webClient.QueryString.Add("file", filename); // To identify the file 
            webClient.DownloadFileAsync(new Uri($"{myHttpWebResponse.ResponseUri}"), filename);

        }
    }

    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.Log("[NovidPatch] Download completed!");
    }
    // Update is called once per frame
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
                    Debug.Log($"[NovidPatch] {path} Is valid");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"[NovidPatch] {userData.instance.LocalFilePath} Is valid");
                    if (method == 1)
                    {
                        copyfiles();
                    }
                    else
                    {
                        removeDir();
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
                copyfiles();
            }
            else
            {
                removeDir();
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

    private void removeDir()
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
            Debug.LogError("[NovidPatch] USER HASN'T SELECTED A GAME REGION");
            return;
        }
        if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/GAMEUI.XML"))
        {
            File.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/GAMEUI.XML");
            
        }
        if(File.Exists($"{userData.instance.LocalFilePath}/patches/imported_patch.yml"))
        {
            File.Delete($"{userData.instance.LocalFilePath}/patches/imported_patch.yml");
        }
        GameObject t = Instantiate(MessageBox);
        t.GetComponent<GUI_MessageBox>().title = T.getText("COM_COMMON_RESTORE");
        t.GetComponent<GUI_MessageBox>().message = T.getText("COM_COMMON_DES_RESTORE");
        t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
    }
    private void copyfiles()
    {
        string region = "";
        bool vaidregion = true;
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
            Debug.LogError("[NovidPatch] USER HASN'T SELECTED A GAME REGION");
            return;
           vaidregion = false;

        }
        if (vaidregion)
        {
            if(!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI"))
            {
                Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI");
            }
            File.Copy($"{Application.persistentDataPath}/RPCS3 Patch/imported_patch.yml", $"{userData.instance.LocalFilePath}/patches/imported_patch.yml",true);
            File.Copy($"{Application.streamingAssetsPath}/NOVID_FIX/GAMEUI_PATCH.XML", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/UI/GAMEUI.XML", true);
            
            //show rpcs3 enable video

            //normal message box
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = T.getText("COM_COMMON_COMPLETE");
            t.GetComponent<GUI_MessageBox>().message = T.getText("COM_COMMON_NOVID_DES_COMPLETE");
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
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