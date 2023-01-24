using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DownloadMod : MonoBehaviour
{
    public GameObject LoadingBox;
    private GameObject load;
    public GameObject MessageBox;
    public GameObject messagebox2;
    private WebClient webClient = null;
    public string updateURL;
    public bool dissablePrompt = false;
    public int downloadPercentage = 0;
    public bool updateComplete = false;
    public bool installLiteComplete = false;

    public int GHLUpdateCopyPercentage = 0;

    private int Relocatedattemp = 0;

    public string finishfetchupdateDetails = "";

    private Translater T;
    public enum Version
    {
        None,
        PAL,
        USA,
        Lite
    }

    [Header("download string")]
    public string message = "Please wait while we download the file";

    public void btnDownload_Click(int type, string url, string filename)
    {
        downloadPercentage = 0;
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[DownloadMod] Error. Check internet connection!");
        }
        else
        {
            Debug.Log("[DownloadMod] We have internet");
            // Is file downloading yet?
            if (webClient != null)
            {
                return;
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            webClient = new WebClient();
            if (type == 0) //update details
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver);
                webClient.DownloadFileAsync(new Uri($"{url}"), filename);
            }
            else if (type == 1) //GHL Update zip
            {
                if (!dissablePrompt)
                {
                    load = Instantiate(LoadingBox);
                    load.GetComponent<GUI_MessageBox>().title = T.getText("STR_DOWNLOAD");
                    load.GetComponent<GUI_MessageBox>().message = message;
                }
                // Create a new HttpWebRequest Object to the mentioned URL.
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                myHttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
                myHttpWebRequest.AllowAutoRedirect = true;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                Debug.Log(myHttpWebResponse.ResponseUri);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver2);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                webClient.DownloadFileAsync(new Uri($"{myHttpWebResponse.ResponseUri}"), filename);
            }
            else if (type == 2)
            {
                //download ghlite  
                if (!dissablePrompt)
                {
                    load = Instantiate(LoadingBox);
                    load.GetComponent<GUI_MessageBox>().title = T.getText("STR_DOWNLOAD");
                    load.GetComponent<GUI_MessageBox>().message = T.getText("STR_DOWNLOAD_LITE");
                }
                StartCoroutine(downloadLITE(url, filename));
            }
        }
    }
    private IEnumerator downloadLITE(string url, string filename)
    {
        // Create a new HttpWebRequest Object to the mentioned URL.
        HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        yield return new WaitForEndOfFrame();
        myHttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
        myHttpWebRequest.AllowAutoRedirect = true;
        HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
        yield return new WaitForEndOfFrame();
        Debug.Log(myHttpWebResponse.ResponseUri);
        yield return new WaitForEndOfFrame();
        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver3);
        yield return new WaitForEndOfFrame();
        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
        yield return new WaitForEndOfFrame();
        webClient.DownloadFileAsync(new Uri($"{myHttpWebResponse.ResponseUri}"), filename);
        yield return new WaitForEndOfFrame();
    }
    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.Log("[DownloadMod] Download completed!");
        bool gameUpToDate = false;
        string[] lines = File.ReadAllLines(Application.persistentDataPath + "/GHL_PS3_Version.dat");
        string gameversion = lines[0];
        updateURL = lines[1];
        string clientver = "";
        string region = "";
        Debug.LogWarning("Update URL: " + updateURL);
        Debug.LogWarning("game region: " + userData.instance.gameVersion);
        ////for updating from older version of the tool
        if (userData.instance.gameVersion == userData.Version.PAL)
        {
            region = "BLES02180";
        }
        else if (userData.instance.gameVersion == userData.Version.USA)
        {
            region = "BLUS31556";
        }
        else if(userData.instance.gameVersion == userData.Version.Lite)
        {
            region = "BLES02180";
        }
        else
        {
            region = "BLES02180";
            Debug.LogError("[DownloadMod] USER HASN'T SELECTED A GAME REGION");
            //return;
        }
        string[] split = null;
        if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/PARAM.SFO"))
        {
            string text = File.ReadAllText($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/PARAM.SFO");
            split = text.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            foreach (string line in split)
            {
                Debug.Log(count);
                //Debug.Log(line);
                if (line.Contains(gameversion))
                {
                    Debug.LogWarning("We HAVE A MATCH");
                    gameUpToDate = true;
                }
                count++;
            }
        }
        else
        {
            //GHL update doesn't exist??????
            //load setup
            GameObject a = Instantiate(messagebox2);

            a.GetComponent<GUI_MessageBox>().title = T.getText("NET_COM_NO_GAME");
            a.GetComponent<GUI_MessageBox>().message = T.getText("NET_COM_NO_GAME_DES");
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(RelocatedRPCS3);
        }

        if (!gameUpToDate)
        {
            Debug.Log("GHL need to update");
            finishfetchupdateDetails = "Game need to update";
            //download GHL update from github
            if (!dissablePrompt)
            {
                GameObject a = Instantiate(messagebox2);

                a.GetComponent<GUI_MessageBox>().title = T.getText("STR_GHLR_UPDATE");
                a.GetComponent<GUI_MessageBox>().message = $"{T.getText("STR_GHLR_FOUND")} \n\n{T.getText("STR_LATESTVER")}{gameversion}\n{T.getText("STR_GAMEVER")}{split[57]}";
                a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(downloadGHTVReloaded);
            }
        }
        else
        {
            finishfetchupdateDetails = "Game need to update";
        }
        
    }

    public void downloadGHTVReloaded()
    {
        btnDownload_Click(1, updateURL, Application.persistentDataPath + "/GHTV_Reloaded_update.zip");
    }

    private void CompletedConver2(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.Log("[DownloadMod] Download completed!");
        if (!dissablePrompt)
        {
            load.GetComponent<GUI_MessageBox>().CloseAnim();
        }
        //apply the update
        Debug.LogWarning("[DownloadMod] Applying the update!");
        if(dissablePrompt)
        {
            message = T.getText("NET_COM_PREP_INSTALL");
        }

        //extracting files
        ZipFile.ExtractToDirectory(Application.persistentDataPath + "/GHTV_Reloaded_update.zip", Application.persistentDataPath + "/",true);

        StartCoroutine(CopyGHLUpdate());
    }

    private void CompletedConver3(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.Log("[DownloadMod] Download completed!");
        load.GetComponent<GUI_MessageBox>().message = T.getText("STR_INSTALLING_LITE");
        if(Directory.Exists($"{Application.persistentDataPath}/TV_Edition"))
        {
            Directory.Delete($"{Application.persistentDataPath}/TV_Edition", true);
        }
        Directory.CreateDirectory($"{Application.persistentDataPath}/TV_Edition");
        StartCoroutine(InstallLite());
        //Directory.Move()
    }

    private IEnumerator InstallLite()
    {
        yield return new WaitForEndOfFrame();
        ZipFile.ExtractToDirectory(Application.persistentDataPath + "/GuitarHeroLive-TVEdition.zip", Application.persistentDataPath + "/TV_Edition", true);
        yield return new WaitForEndOfFrame();

        string sourceDir = $"{Application.persistentDataPath}/TV_Edition/RPCS3";
        string destinationDir = $"{userData.instance.LocalFilePath}";

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
            GHLUpdateCopyPercentage = (int)Math.Round((double)(100 * count) / allFiles.Length);
            if (!dissablePrompt)
            {
                load.GetComponent<GUI_MessageBox>().message = $"{T.getText("STR_INSTALLING_LITE")}\n\n{GHLUpdateCopyPercentage}%";
                load.GetComponent<GUI_MessageBox>().addmessage();
            }
            else
            {
                message = $"{T.getText("STR_INSTALLING_LITE")} {GHLUpdateCopyPercentage}%";
            }
            yield return new WaitForEndOfFrame();
            File.Copy(newPath, newPath.Replace(sourceDir, destinationDir), true);
            Debug.Log($"COPY FILE:{newPath} TO {newPath.Replace(sourceDir, destinationDir)}");
            count++;
        }
        Directory.Delete($"{Application.persistentDataPath}/TV_Edition/RPCS3", true); //remove the install files
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        installLiteComplete = true;
    }

   public IEnumerator CopyGHLUpdate()
    {
        yield return new WaitForEndOfFrame();

        string region = "";
        bool vailidRegion = true;
        string SFOfile = "";
        ////for updating from older version of the tool
        if (userData.instance.gameVersion == userData.Version.PAL)
        {
            region = "BLES02180";
            SFOfile = $"{Application.persistentDataPath}/GHTVReloadedUpdate/RPCS3_region_settings/PAL_PARAM.SFO";
        }
        else if (userData.instance.gameVersion == userData.Version.USA)
        {
            region = "BLUS31556";
            SFOfile = $"{Application.persistentDataPath}/GHTVReloadedUpdate/RPCS3_region_settings/USA_PARAM.SFO";
        }
        else if (userData.instance.gameVersion == userData.Version.Lite)
        {
            region = "BLES02180";
            SFOfile = $"{Application.persistentDataPath}/GHTVReloadedUpdate/RPCS3_region_settings/PARAM_LITE.SFO";
        }
        else
        {
            Debug.LogError("[DownloadMod] USER HASN'T SELECTED A GAME REGION");
            vailidRegion = false;
        }
        if (vailidRegion)
        {
            if (!dissablePrompt)
            {
                load = Instantiate(LoadingBox);
                load.GetComponent<GUI_MessageBox>().title = T.getText("STR_APPLYUPDATE");
                load.GetComponent<GUI_MessageBox>().message = T.getText("STR_APPLYUPDATEMSG");
            }
            string sourceDir = $"{Application.persistentDataPath}/GHTVReloadedUpdate/RPCS3";
            string destinationDir = $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}";

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
            Debug.Log("[DownloadMod] allFiles length " + allFiles.Length);
            int count = 1;

            //for some reason linux has a hidden files or maks allfiles report 1 more file than windows
            try
            {
                foreach (string newPath in allFiles)
                {
                    GHLUpdateCopyPercentage = (int)Math.Round((double)(100 * count) / allFiles.Length);
                    if (!dissablePrompt)
                    {
                        load.GetComponent<GUI_MessageBox>().message = $"{T.getText("STR_APPLYUPDATEMSG")}\n\n{GHLUpdateCopyPercentage}%";
                        load.GetComponent<GUI_MessageBox>().addmessage();
                    }
                    else
                    {
                        message = $"{T.getText("NET_COM_APPLY_MOD")} {GHLUpdateCopyPercentage}%";
                    }
                   

                    File.Copy(newPath, newPath.Replace(sourceDir, destinationDir), true);
                    Debug.Log($"[DownloadMod] File copy {count}/{allFiles.Length}");
                    Debug.Log($"COPY FILE:{newPath} TO {newPath.Replace(sourceDir, destinationDir)}");
                    count++;


                }
            }
            catch(Exception e)
            {
                Debug.LogWarning("[DownloadMod] Error message " + e.Message);
                Debug.Log($"[DownloadMod] Failed to copy {allFiles[count - 1]} TO {allFiles[count-1].Replace(sourceDir, destinationDir)}");
            }
            count++;
            GHLUpdateCopyPercentage = (int)Math.Round((double)(100 * count) / allFiles.Length);
            if (!dissablePrompt)
            {
                load.GetComponent<GUI_MessageBox>().message = $"{T.getText("STR_APPLYUPDATEMSG")}\n\n{GHLUpdateCopyPercentage}%";
            }
            else
            {
                message = $"Apply mod {GHLUpdateCopyPercentage}/{allFiles.Length}";
            }
            try
            {
                Debug.Log($"COPY FILE:{SFOfile} TO" + $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/PARAM.SFO");
                File.Copy(SFOfile, $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/PARAM.SFO", true); //copying parma.SFO (each region has there own version)
            }
            catch(Exception e)
            {
                Debug.LogWarning("[DownloadMod] Error message " + e.Message);
                Debug.Log($"COPY FILE:{SFOfile} TO" + $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/PARAM.SFO");
            }
            if (!dissablePrompt)
            {
                load.GetComponent<GUI_MessageBox>().CloseAnim();
                GameObject t = Instantiate(MessageBox);
                t.GetComponent<GUI_MessageBox>().title = T.getText("STR_GAMEUPDATEDONE");
                t.GetComponent<GUI_MessageBox>().message = T.getText("STR_GAME_UP_TO_DATE");
            }
            else
            {
                message = T.getText("NET_COM_UPDATE_DONE");
            }
            updateComplete = true;
            Debug.LogWarning("[DownloadMod] UPDATE COMPLETED");
        }
    }

    private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required
        Debug.Log("[DownloadMod] Download Recevedbyte: " + e.BytesReceived + " Download totalbyte: " + e.TotalBytesToReceive);
        downloadPercentage = e.ProgressPercentage;
        
        // use these variables if needed
        if (!dissablePrompt)
        {
            load.GetComponent<GUI_MessageBox>().message = $"{T.getText("STR_DOWNLOAD_LITE")}\n\n {T.getText("STR_DOWNLOADED")} {e.ProgressPercentage}%";
            load.GetComponent<GUI_MessageBox>().addmessage();
        }
        else
        {
           message = $"{T.getText("STR_DOWNLOADED")} {e.ProgressPercentage}%";
        }
    }

    public void RelocatedRPCS3()
    {
        bool pathhasrpcs3 = false;

        var paths = StandaloneFileBrowser.OpenFolderPanel(T.getText("MSG_SEL_RPCS3"), T.getText("MGS_RPCS3_FOLD"), false);
        foreach (var path in paths)
        {
            var files = Directory.GetFiles(path, "rpcs3" + ".*");
            if (files.Length > 0)
            {
                pathhasrpcs3 = true;
                Debug.Log($"[SETUP] rpcs3 is in this dir");
                userData.instance.LocalFilePath = path;
                Debug.Log($"[SETUP] {userData.instance.LocalFilePath} Is valid");
                // at least one matching file exists
                // file name is files[0]
            }
            else
            {
                Debug.Log("[SETUP] rpcs3 isn't in this dir");
            }
        }

        if (!pathhasrpcs3)
        {
            Relocatedattemp++;
            GameObject t = Instantiate(MessageBox);
            userData.instance.LocalFilePath = "TEST";

            if (Relocatedattemp >= 3)
            {
                Debug.Log("[DownloadMod] Failed to find RPCS3, Throwing user to setup to make sure they have everything.");
                t.GetComponent<GUI_MessageBox>().title = T.getText("NET_COM_PATH_FAILL");
                t.GetComponent<GUI_MessageBox>().message = T.getText("NET_COM_PATH_FAILL_DES");
                t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(GOTOSETUP);
            }
            else
            {
                t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
                t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
                t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(RelocatedRPCS3);
            }
        }
    }
    public void GOTOSETUP()
    {
        SceneManager.LoadScene("Setup");
    }
    private void Awake()
    {
        T = Translater.instance;
    }
}
