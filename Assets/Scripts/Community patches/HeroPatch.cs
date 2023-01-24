using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HeroPatch : MonoBehaviour
{
    public GameObject MessageBox;
    public GameObject fadeToBlack;
    public GameObject LoadingBox;
    private GameObject load;

    public RawImage before;
    public RawImage after;

    public Texture2D[] BeforeIMG;
    public Texture2D[] AfterIMG;
    public int ImgCountmax;
    public float timeToChange = 4;
    private Translater T;
    // Start is called before the first frame update
    private void Start()
    {
        T = Translater.instance;
        if (BeforeIMG.Length != AfterIMG.Length)
        {
            Debug.LogError("[Hero Patch] YOUR BEFORE AND AFTER IMG ISN'T IS NOT MATCHING LENGTH");
        }
        else
        {
            ImgCountmax = BeforeIMG.Length - 1;
            StartCoroutine(ImageCHanger());
        }
        
        DiscordController.instance.UpdateDiscordActivity("Hero Patch", "Enabling or disabling Hero patch", "mainmenu", "Main Menu");
        
    }
    IEnumerator ImageCHanger()
    {
        int count = 0;
        while(true)
        {
            yield return new WaitForSeconds(timeToChange);
            if(count > ImgCountmax)
            {
                count = 0;
            }
            before.texture = BeforeIMG[count];
            after.texture = AfterIMG[count];
            count++;
        }
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
            var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR RPCS3 FOLDER", "RPSC3 FOLDER", false);
            foreach (var path in paths)
            {
                if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
                {
                    Debug.Log($"[HeroPatch] {path} Is valid");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"[HeroPatch] {userData.instance.LocalFilePath} Is valid");
                    if (method == 1)
                    {
                        StartCoroutine(copyfiles());
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
                StartCoroutine(copyfiles());
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
            Debug.LogError("[HeroPatch] USER HASN'T SELECTED A GAME REGION");
            return;
        }
        if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/FAR/HEROPOWERS"))
        {
            Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/FAR/HEROPOWERS", true);
        }
        if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/GHTV/HEROPOWERS"))
        {
            Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/GHTV/HEROPOWERS", true);
        }
        GameObject t = Instantiate(MessageBox);
        t.GetComponent<GUI_MessageBox>().title = T.getText("COM_COMMON_RESTORE");
        t.GetComponent<GUI_MessageBox>().message = T.getText("COM_COMMON_DES_RESTORE");
        t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
    }
    private IEnumerator copyfiles()
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
            Debug.LogError("[HeroPatch] USER HASN'T SELECTED A GAME REGION");
            vaidregion = false;
           
        }
        if (vaidregion)
        {
            load = Instantiate(LoadingBox);
            load.GetComponent<GUI_MessageBox>().title = T.getText("COM_COMMON_APPLYING");
            load.GetComponent<GUI_MessageBox>().message = T.getText("COM_COMMON_APPLYING_DES");

            string sourceDir = $"{Application.streamingAssetsPath}/HERO_FIX";
            string destinationDir = $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE";

            var allDirectories = Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories);
            foreach (string dir in allDirectories)
            {
                yield return new WaitForEndOfFrame();
                string dirToCreate = dir.Replace(sourceDir, destinationDir);
                if (!Directory.Exists(dirToCreate))
                {
                    Directory.CreateDirectory(dirToCreate);
                }
            }
            var allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            int count = 0;
            foreach (string newPath in allFiles)
            {
                int percentComplete = (int)Math.Round((double)(100 * count) / allFiles.Length);
                load.GetComponent<GUI_MessageBox>().message = $"Please wait while we Apply the Patch\n\n{percentComplete}/{allFiles.Length}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                yield return new WaitForEndOfFrame();
                File.Copy(newPath, newPath.Replace(sourceDir, destinationDir), true);
                Debug.Log($"[HeroPatch] COPY FILE:{newPath} TO {newPath.Replace(sourceDir, destinationDir)}");
                count++;
            }
            load.GetComponent<GUI_MessageBox>().CloseAnim();
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = T.getText("COM_COMMON_COMPLETE");
            t.GetComponent<GUI_MessageBox>().message = T.getText("COM_COMMON_DES_COMPLETE");
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
