using SFB;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HyperSpeed : MonoBehaviour
{
    public float hyperSpeed = 1.0f;
    public float defaultSpeed = 1.0f;
    public float maxSpeed = 5.0f;
    public float minSpeed = 0.5f;

    private double offset = 0;
    public string path;

    [Header("GameObjects")]
    public Slider slider;

    public TextMeshProUGUI percentage;
    public MeshRenderer highwayMat;
    public GameObject Message;
    public GameObject fadeToBlack;
    public GameObject Loading;

    private WebClient webClient = null;
    private bool downloadDone = false;
    private GameObject load;

    public DateTime lastCheck;
    public DateTime dt;
    private void Start()
    {
        DiscordController.instance.UpdateDiscordActivity("Hyper Speed Changer", "Adjusting the highway speed for GHL", "hyperspeed", "Hyper Speed Changer");
        updateText(hyperSpeed);
        slider.value = hyperSpeed;
        slider.maxValue = maxSpeed;
        slider.minValue = minSpeed;

        //checking if time has been more than a week or it a wendsday
        lastCheck = UnixTimeToDateTime(userData.instance.hyperspeedLastDL);
        dt = DateTime.Now;
        if (DateTime.Now > lastCheck.Date.AddDays(7))
        {
            Debug.LogWarning("[HyperSpeed] A week has past since we last cache, deleting hyperspeed cache");
            if(Directory.Exists(Application.persistentDataPath + "/External_Tools/HyperSpeed"))
            {
                Directory.Delete(Application.persistentDataPath + "/External_Tools/HyperSpeed", true);
            }
            userData.instance.hyperspeedLastDL = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        else if ((dt.Hour >= 10 && dt.Hour <= 12) && dt.DayOfWeek == DayOfWeek.Wednesday)
        {
            Debug.LogWarning("[HyperSpeed] It the timeframe when GHTV get new content deleting hyperspeed cache");
            if (Directory.Exists(Application.persistentDataPath + "/External_Tools/HyperSpeed"))
            {
                Directory.Delete(Application.persistentDataPath + "/External_Tools/HyperSpeed", true);
            }
            userData.instance.hyperspeedLastDL = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }

    public static DateTime UnixTimeToDateTime(long unixtime)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dtDateTime.AddMilliseconds(unixtime).ToLocalTime();
    }

    private void Update()
    {
        offset += (Time.deltaTime * hyperSpeed) / 10.0;
        highwayMat.material.mainTextureOffset = new Vector2(0, Time.realtimeSinceStartup * hyperSpeed);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject a = Instantiate(fadeToBlack);
            a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
            a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
            a.GetComponent<FadeToBlack>().anim.Play();
        }
    }

    public void done()
    {
        if (userData.instance.LocalFilePath == "TEST")
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR RPCS3 FOLDER", "RPSC3 FOLDER", false);
            foreach (var path in paths)
            {
                if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
                {
                    Debug.Log($"[HyperSpeed] {path} Is valid");
                    userData.instance.LocalFilePath = path;
                }
                else
                {
                    GameObject t = Instantiate(Message);
                    t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
                    t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
                }
            }
        }
        //checking if path is vaild
        if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
        {
            Debug.Log($"[HyperSpeed] {userData.instance.LocalFilePath} Is valid");
            StartCoroutine(SaveToDisk(userData.instance.LocalFilePath));
        }
        else
        {
            GameObject t = Instantiate(Message);
            userData.instance.LocalFilePath = "TEST";
            t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
            t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
        }
    }

    private IEnumerator SaveToDisk(string path)
    {
        load = null;
        if (path == null || path == "")
        {
            GameObject h = Instantiate(Message);
            h.GetComponent<GUI_MessageBox>().title = "Failed To Save Hyper Speed";
            h.GetComponent<GUI_MessageBox>().message = $"An Error has occured while saving\nPlease try again\n\nERROR CODE: HSsysNoneFailed";
            Debug.LogError("[HyperSpeed] path is null while saving");
            yield break;
        }
        if (!Directory.Exists(Application.persistentDataPath + "/External_Tools/HyperSpeed/OVERRIDE/TRACKS_CLEAN"))
        {
            load = Instantiate(Loading);
            load.GetComponent<GUI_MessageBox>().title = "Saving Hyper Speed";
            load.GetComponent<GUI_MessageBox>().message = "Please wait....";

            Download();
            yield return new WaitUntil(() => downloadDone == true);
        }
        if(load == null)
        {
            load = Instantiate(Loading);
            load.GetComponent<GUI_MessageBox>().title = "Saving Hyper Speed";
            load.GetComponent<GUI_MessageBox>().message = "Please wait....";
        }
        Debug.Log("[HyperSpeed] Copying trackconfig to game");
        var directories = Directory.GetDirectories(Application.persistentDataPath + "/External_Tools/HyperSpeed/OVERRIDE/TRACKS_CLEAN");
        foreach (var d in directories)
        {
            yield return new WaitForEndOfFrame();
            var a = d.Split(@"TRACKS_CLEAN\");
            if (a.Length > 0)
            {
                if (File.Exists(d + "/TRACKCONFIG.XML"))
                {
                    XDocument xmlFile = XDocument.Load(d + "/TRACKCONFIG.XML");

                    var dif = from c in xmlFile.Elements("Track").Elements("Highway")
                              select c;

                    foreach (XElement c in dif)
                    {
                        c.Attribute("newbeginner").Value = $"{hyperSpeed}";
                        c.Attribute("neweasy").Value = $"{hyperSpeed}";
                        c.Attribute("newmedium").Value = $"{hyperSpeed}";
                        c.Attribute("newhard").Value = $"{hyperSpeed}";
                        c.Attribute("newexpert").Value = $"{hyperSpeed}";
                    }
                    //save the XML back as file
                    //checking if directory exist
                    if (!Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}")) ;
                    {
                        Directory.CreateDirectory($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}");
                    }
                    if (!Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}")) ;
                    {
                        Directory.CreateDirectory($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}");
                    }
                    xmlFile.Save($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}/TRACKCONFIG.XML");
                    xmlFile.Save($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}/TRACKCONFIG.XML");
                }
            }
        }

        load.GetComponent<GUI_MessageBox>().CloseAnim();
        yield return new WaitForSeconds(1f);
        Debug.Log("[HyperSpeed] Finished Copying");
        GameObject t = Instantiate(Message);
        t.GetComponent<GUI_MessageBox>().title = "Hyper Speed Saved!";
        t.GetComponent<GUI_MessageBox>().message = $"Your Hyper been save to your Guitar Hero Live game.\n\nThe next song you load your Hyper Speed will be apply";
        t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        yield return null;
    }

    public void SliderChangeVal(float n)
    {
        hyperSpeed = (float)Math.Round(n, 2);
        updateText(hyperSpeed);
    }

    public void DefaultSpeed()
    {
        hyperSpeed = 1.0f;
        updateText(hyperSpeed);
        slider.value = hyperSpeed;
    }

    public void RecomendedSpeed()
    {
        hyperSpeed = 2.0f;

        updateText(hyperSpeed);
        slider.value = hyperSpeed;
    }

    private void updateText(float n)
    {
        var x = (int)(((decimal)n % 1) * 100);
        string b = $"{n}";
        string[] a = b.Split(new char[] { '.' });
        if ((int)n == n && n >= 1)
        {
            percentage.text = $"%{n}00";
        }
        else if (n < 1)
        {
            percentage.text = $"%{x}";
        }
        else if (a[1] != null && a[1].Length == 1)
        {
            percentage.text = $"%{$"{n}".ToString().Replace(".", "")}0";
        }
        else
        {
            percentage.text = $"%{$"{n}".ToString().Replace(".", "")}";
        }
    }

    public void ReturnToMainMenu()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

    private void Download()
    {

            //checking if we have internet connection
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("[HyperSpeed] Error. Check internet connection!");
                GameObject a = Instantiate(Message);
                a.GetComponent<GUI_MessageBox>().title = "No Active Internet Connection";
                a.GetComponent<GUI_MessageBox>().message = $"No internet connect exist\n\nMake sure you are connected to the internet and try again.";
                a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
                return;
            }
            else
            {
                Debug.Log("[HyperSpeed] We have internet");
            }
            // Is file downloading yet?
            if (webClient != null)
                return;

            webClient = new WebClient();
            downloadDone = false;
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            webClient.QueryString.Add("file", Application.persistentDataPath + "\\Hyperspeed.zip"); // To identify the file 
            webClient.DownloadFileAsync(new Uri("http://ghtv.tools.hyper.stickgaming.net/"), Application.persistentDataPath + "\\Hyperspeed.zip");
        
    }
    void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required   
        Debug.Log("[HyperSpeed] Download Recevedbyte: " + e.BytesReceived + " Download totalbyte: " + e.TotalBytesToReceive);
        // use these variables if needed
        load.GetComponent<GUI_MessageBox>().message = $"Downloading Hyper Speed data\n\n Dowloaded: {e.ProgressPercentage}%";
        load.GetComponent<GUI_MessageBox>().addmessage();


    }
    private void Completed(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.LogWarning(e);
        Debug.Log("[HyperSpeed] Download completed!");
        extractFile("Hyperspeed.zip");
    }

    private void extractFile(string filename)
    {
        long length = new FileInfo(Application.persistentDataPath + "\\Hyperspeed.zip").Length;
        if (length > 0)
        {
            if (!Directory.Exists(Application.persistentDataPath + "\\External_Tools"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "\\External_Tools");
            }
            if (!Directory.Exists(Application.persistentDataPath + "\\External_Tools\\HyperSpeed"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "\\External_Tools\\HyperSpeed");
            }
            ZipFile.ExtractToDirectory(Application.persistentDataPath + $"\\{filename}", Application.persistentDataPath + "\\External_Tools\\HyperSpeed");
            File.Delete(Application.persistentDataPath + "\\Hyperspeed.zip");
            downloadDone = true;
        }
        else
        {
            GameObject t = Instantiate(Message);
            t.GetComponent<GUI_MessageBox>().title = "Failed to download!";
            t.GetComponent<GUI_MessageBox>().message = $"The tool failed to download the hyper speed libary.\n\nPlease try again";
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        }
    }
}