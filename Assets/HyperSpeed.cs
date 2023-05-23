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
    private Translater T;
    private void Start()
    {
        T = Translater.instance;
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
                    t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
                    t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
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
            t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
            t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
        }
    }

    private IEnumerator SaveToDisk(string path)
    {
        //version checker
        string region = "";
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
            Debug.LogError("[HyperSpeed] USER HASN'T SELECTED A GAME REGION");
            yield break;
        }

        load = null;
        if (path == null || path == "")
        {
            GameObject h = Instantiate(Message);
            h.GetComponent<GUI_MessageBox>().title = T.getText("HYPER_STR_SAVED_FAILED");
            h.GetComponent<GUI_MessageBox>().message = T.getText("HYPER_STR_SAVED_FAILED_DES");
            Debug.LogError("[HyperSpeed] path is null while saving");
            yield break;
        }
        if (!Directory.Exists(Application.persistentDataPath + "/External_Tools/HyperSpeed/OVERRIDE/TRACKS_CLEAN") || !Directory.Exists(Application.persistentDataPath + "/External_Tools/HyperSpeed/AUDIO_CLEAN"))
        {
            load = Instantiate(Loading);
            load.GetComponent<GUI_MessageBox>().title = T.getText("HYPER_STR_SAVING");
            load.GetComponent<GUI_MessageBox>().message = T.getText("STR_PLZ_WAIT");

            Download();
            yield return new WaitUntil(() => downloadDone == true);
        }
        if(load == null)
        {
            load = Instantiate(Loading);
            load.GetComponent<GUI_MessageBox>().title = T.getText("HYPER_STR_SAVING");
            load.GetComponent<GUI_MessageBox>().message = T.getText("STR_PLZ_WAIT");
        }
        Debug.Log("[HyperSpeed] Copying GHTV trackconfig to game");
        var directories = Directory.GetDirectories(Application.persistentDataPath + "/External_Tools/HyperSpeed/OVERRIDE/TRACKS_CLEAN");
        foreach (var d in directories)
        {
            Debug.Log($"[HyperSpeed] dir = {d}");
            yield return new WaitForEndOfFrame();
            var a = d.Split(@"TRACKS_CLEAN\");
            if(Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                a = d.Split(@"TRACKS_CLEAN/");
            }
            if (a.Length > 0)
            {
                if (File.Exists(d + "/TRACKCONFIG.XML"))
                {
                    editGHTVxml(d, a, region, path);

                }
            }
        }



        Debug.Log("[HyperSpeed] Copying GHL trackconfig to game");
        directories = Directory.GetDirectories(Application.persistentDataPath + "/External_Tools/HyperSpeed/OVERRIDE/AUDIO_CLEAN/AUDIOTRACKS");
        foreach (var d in directories)
        {
            Debug.Log($"[HyperSpeed] dir = {d}");
            yield return new WaitForEndOfFrame();
            var a = d.Split(@"AUDIOTRACKS\");
            if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                a = d.Split(@"AUDIOTRACKS/");
            }
            if (a.Length > 0)
            {
                if (File.Exists(d + "/TRACKCONFIG.XML"))
                {

                    editGHLxml(d, a, region, path);
                }
            }
        }


        load.GetComponent<GUI_MessageBox>().CloseAnim();
        yield return new WaitForSeconds(1f);
        Debug.Log("[HyperSpeed] Finished Copying");
        GameObject t = Instantiate(Message);
        t.GetComponent<GUI_MessageBox>().title = T.getText("HYPER_STR_SAVED");
        t.GetComponent<GUI_MessageBox>().message = T.getText("HYPER_STR_SAVED_DES");
        t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        yield return null;
    }

    private void editGHTVxml(string d, string[] a, string region,string path)
    {
        try
        {
            Debug.Log($"[HyperSpeed] opening {a[1]}.xml from {d}");
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
            if (!Directory.Exists($"{path}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}"))
            {
                Directory.CreateDirectory($"{path}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}");
            }
            Debug.Log($"[HyperSpeed] Song ID {a[1]} saved");
            xmlFile.Save($"{path}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/TRACKS/{a[1]}/TRACKCONFIG.XML");
        }
        catch (Exception e)
        {
            Debug.LogError($"[HyperSpeed] Failed to save SondID {a[1]}");
            Debug.LogError($"[HyperSpeed] {e.Message}");
        }
    }
    private void editGHLxml(string d, string[] a, string region, string path)
    {
        try
        {
            Debug.Log($"[HyperSpeed] opening {a[1]}.xml from {d}");
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
            if (!Directory.Exists($"{path}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/AUDIO/AUDIOTRACKS/{a[1]}")) ;
            {
                Directory.CreateDirectory($"{path}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/AUDIO/AUDIOTRACKS/{a[1]}");
            }
            Debug.Log($"[HyperSpeed] Song ID {a[1]} saved");
            xmlFile.Save($"{path}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/AUDIO/AUDIOTRACKS/{a[1]}/TRACKCONFIG.XML");
        }
        catch(Exception e)
        {
            Debug.LogError($"[HyperSpeed] Failed to save SondID {a[1]}");
            Debug.LogError($"[HyperSpeed] {e.Message}");
        }
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
                a.GetComponent<GUI_MessageBox>().title = T.getText("NET_COM_NONE_ACTIVE");
                a.GetComponent<GUI_MessageBox>().message = T.getText("NET_COM_NONE_ACTIVE_DES");
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
            if (File.Exists($"{Application.persistentDataPath}/Hyperspeed.zip"))
            {
            File.Delete($"{Application.persistentDataPath}/Hyperspeed.zip");
            }
            webClient = new WebClient();
            downloadDone = false;
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://ghtv.tools.hyper.stickgaming.net/");
            myHttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
            myHttpWebRequest.AllowAutoRedirect = true;
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            Debug.Log(myHttpWebResponse.ResponseUri);

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            webClient.QueryString.Add("file", Application.persistentDataPath + "/Hyperspeed.zip"); // To identify the file 
            webClient.DownloadFileAsync(new Uri($"{myHttpWebResponse.ResponseUri}"), Application.persistentDataPath + "/Hyperspeed.zip");
        
    }
    void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required   
        Debug.Log("[HyperSpeed] Download Recevedbyte: " + e.BytesReceived + " Download totalbyte: " + e.TotalBytesToReceive);
        // use these variables if needed
        load.GetComponent<GUI_MessageBox>().message = $"{T.getText("HYPER_STR_DOWNLOAD_DATA")}\n\n{T.getText("STR_DOWNLOADED")} {e.ProgressPercentage}%";
        load.GetComponent<GUI_MessageBox>().addmessage();


    }
    private void Completed(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        load = null;
        Debug.LogWarning(e);
        Debug.Log("[HyperSpeed] Download completed!");
        extractFile("Hyperspeed.zip");
    }

    private void extractFile(string filename)
    {
        Debug.Log("[Hyper Speed] Extracting files");
        long length = new FileInfo(Application.persistentDataPath + "/Hyperspeed.zip").Length;
        if (length > 0)
        {
            //cheap bug fix
            if(Directory.Exists(Application.persistentDataPath + "/External_Tools/HyperSpeed"))
            {
                Directory.Delete(Application.persistentDataPath + "/External_Tools/HyperSpeed", true);
            }
            if (!Directory.Exists(Application.persistentDataPath + "/External_Tools"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/External_Tools");
            }
            if (!Directory.Exists(Application.persistentDataPath + "/External_Tools/HyperSpeed"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/External_Tools/HyperSpeed");
            }
            ZipFile.ExtractToDirectory(Application.persistentDataPath + $"/{filename}", Application.persistentDataPath + "/External_Tools/HyperSpeed");
            File.Delete(Application.persistentDataPath + "/Hyperspeed.zip");
            downloadDone = true;
        }
        else
        {
            GameObject t = Instantiate(Message);
            t.GetComponent<GUI_MessageBox>().title = T.getText("NET_COM_FAILED_DOWNLOAD");
            t.GetComponent<GUI_MessageBox>().message = T.getText("HYPER_STR_DOWNLOAD_DATA_FAIL");
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        }
    }
}