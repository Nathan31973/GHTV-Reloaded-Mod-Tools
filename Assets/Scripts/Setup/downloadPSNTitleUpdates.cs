using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using UnityEngine;

public class downloadPSNTitleUpdates : MonoBehaviour
{
    private WebClient webClient = null;

    [Header("Messageboxes")]
    public GameObject MessageBox;

    private GameObject load;
    public GameObject LoadingBox;
    public Setup main;

    private void Start()
    {
        startScript();
    }
    public void startScript()
    {
        main.psn = this;
        //finding game version
        if (userData.instance.gameVersion == userData.Version.PAL)
        {
            btnDownload_Click("http://b0.ww.np.dl.playstation.net/tppkg/np/BLES02180/BLES02180_T5/bcc8a750fbd7103b/EP0002-BLES02180_00-GUITARHERONEXTP3-A0106-V0100-PE.pkg", $"{Application.persistentDataPath}/GuitarHeroLiveUpdates/Latest Guitar Hero Live Updates.pkg");
        }
        else if (userData.instance.gameVersion == userData.Version.USA)
        {
            btnDownload_Click("http://b0.ww.np.dl.playstation.net/tppkg/np/BLUS31556/BLUS31556_T6/84cd4d89ab5ff01e/UP0002-BLUS31556_00-GUITARHERONEXTP3-A0107-V0100-PE.pkg", $"{Application.persistentDataPath}/GuitarHeroLiveUpdates/Latest Guitar Hero Live Updates.pkg");
        }
        else if (userData.instance.gameVersion == userData.Version.Lite)
        {
            Debug.LogWarning("[DOWNLOAD PSN TITLE UPDATES] We shouldn't be needing to download the offical update for Lite version");
        }
        else
        {
            Debug.LogError("[DOWNLOAD PSNTITLE UPDATES] unknow game version");
        }
    }

    private void btnDownload_Click(string url, string filename)
    {
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[DOWNLOAD PSN TITLE UPDATES] Error. Check internet connection!");
            //throw an error message for not internet connection
        }
        else
        {
            Debug.Log("[DOWNLOAD PSN TITLE UPDATES] We have internet");
            // Is file downloading yet?
            if (webClient != null)
            {
                return;
            }
            if (!Directory.Exists($"{Application.persistentDataPath}/GuitarHeroLiveUpdates/"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/GuitarHeroLiveUpdates");
            }
            if(File.Exists(filename))
            {
                File.Delete(filename);
            }    
            if (!File.Exists(filename))
            {


                webClient = new WebClient();

                load = Instantiate(LoadingBox);
                load.GetComponent<GUI_MessageBox>().title = "Downloading";
                load.GetComponent<GUI_MessageBox>().message = "Please wait while we download the official Guitar Hero Live Update";
                // Create a new HttpWebRequest Object to the mentioned URL.
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                myHttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
                myHttpWebRequest.AllowAutoRedirect = true;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                Debug.Log(myHttpWebResponse.ResponseUri);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                webClient.DownloadFileAsync(new Uri($"{myHttpWebResponse.ResponseUri}"), filename);
            }
            else
            {
                opendownloadfolder();
            }

        }
    }

    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        Debug.Log("[DOWNLOAD PSN TITLE UPDATES] Download completed!");
        if (File.Exists($"{Application.persistentDataPath}/GuitarHeroLiveUpdates/Latest Guitar Hero Live Updates.pkg"))
        {
            opendownloadfolder();
        }
    }

    private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required
        Debug.Log("[DOWNLOAD PSN TITLE UPDATES] Download Recevedbyte: " + e.BytesReceived + " Download totalbyte: " + e.TotalBytesToReceive);
        // use these variables if needed
        load.GetComponent<GUI_MessageBox>().message = $"Please wait while we download the official Guitar Hero Live Update\n\n Dowloaded: {e.ProgressPercentage}%";
        load.GetComponent<GUI_MessageBox>().addmessage();
    }
    
    public void opendownloadfolder()
    {
        Application.OpenURL(@$"{Application.persistentDataPath}/GuitarHeroLiveUpdates/");
    }
}