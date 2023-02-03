using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using UnityEngine;
using TMPro;
using System.Xml.Linq;
using System.Linq;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject fadeToBlack;
    public GameObject Settings;
    public GameObject CommuityPatches;
    private WebClient webClient = null;
    public GameObject messagebox2;
    private string updateURL;
    public GameObject LoadingBox;
    private GameObject load;
    public GameObject MessageBox;

    [Header("Message of the Day")]
    public GameObject MOTD;
    public TextMeshProUGUI motdText;
    private long UpdateMOTD;
    private bool firstMOTDCheck = false;
    private bool failedtogetMOTD = false;
    private bool downloadedMOTD = false;

    private Translater t;
    public void Start()
    {
        t = Translater.instance;
        //getting latest version of GHL
        if (userData.instance.AutoGameUpdate)
        {
            StartCoroutine(waitFORMOTD());
        }
        //get message of the day

        StartCoroutine(getmotd());
        if (!userData.instance.hasDoneFirstTimeSetup)
        {
            SceneManager.LoadScene("Setup");
        }

        gitHubManager.instance.GetVersion();
        DiscordController.instance.UpdateDiscordActivity("Main Menu", "Selecting a tool", "mainmenu", "Main Menu");
    }

    public IEnumerator waitFORMOTD()
    {
        yield return new WaitUntil(() => downloadedMOTD == true);
        CheckForGHLUpdate();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void GOTOCOLOURCHAGER()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "ColourChanger";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

    public void GOTOHYPERSPEEDCHANGER()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Hyper Speed";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

    public void GOTOGEMCHANGER()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Gem Changer";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

    public void OPENSETTINGS()
    {
        Instantiate(Settings);
    }

    public void OPENCOMMUITYPATCH()
    {
        Instantiate(CommuityPatches);
    }

    //get the latest version of GHTV mod

    private void CheckForGHLUpdate()
    {
        btnDownload_Click(0, "https://raw.githubusercontent.com/Nathan31973/GHTV-Reloaded-Mods-Tools-Assets/main/GHL_PS3_LatestVersion.dat", Application.persistentDataPath + "/GHL_PS3_Version.dat");
    }

    private void btnDownload_Click(int type, string url, string filename)
    {
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[MainMenu] Error. Check internet connection!");
        }
        else
        {
            Debug.Log("[MainMenu] We have internet");
            // Is file downloading yet?
            if (webClient != null)
            {
                Debug.LogError("[MainMenu] Webclient in used");
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
                load = Instantiate(LoadingBox);
                load.GetComponent<GUI_MessageBox>().title = t.getText("STR_DOWNLOAD");
                load.GetComponent<GUI_MessageBox>().message = t.getText("STR_DOWNLOAD_GHLR_UPDATE");
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
            else if(type == 2)
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver3);
                webClient.DownloadFileAsync(new Uri($"{url}"), filename);
            }
        }
    }

    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.Log("[MainMenu] Download completed!");
        bool gameUpToDate = false;
        string[] lines = File.ReadAllLines(Application.persistentDataPath + "/GHL_PS3_Version.dat");
        string gameversion = lines[0];
        updateURL = lines[1];
        string clientver = "";
        string region = "";
        Debug.LogWarning("Update URL: " + updateURL);

        ////for updating from older version of the tool
        if (userData.instance.gameVersion == userData.Version.PAL)
        {
            region = "BLES02180";
        }
        else if (userData.instance.gameVersion == userData.Version.USA)
        {
            region = "BLUS31556";
        }
        else
        {
            Debug.LogError("[MAINMENU] USER HASN'T SELECTED A GAME REGION");
            return;
        }
        string[] split = null;
        //removing the old gems if placed
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
            //check if the game is isn't the latest
            //we want to make sure that GHL is updated
            //load setup
        }

        if (!gameUpToDate)
        {
            Debug.Log("GHL need to update");
            //download GHL update from github
            GameObject a = Instantiate(messagebox2);
            a.GetComponent<GUI_MessageBox>().title = t.getText("STR_GHLR_UPDATE");
            a.GetComponent<GUI_MessageBox>().message = $"{t.getText("STR_GHLR_FOUND")} \n\n{t.getText("STR_LATESTVER")}{gameversion}\n{t.getText("STR_GAMEVER")}{split[57]}";
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(confirmdownload);
        }
    }

    public void confirmdownload()
    {
        btnDownload_Click(1, updateURL, Application.persistentDataPath + "/GHTV_Reloaded_update.zip");
    }

    private void CompletedConver2(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.Log("[MainMenu] Download completed!");
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        //apply the update
        Debug.LogWarning("[MainMenu] Applying the update!");
        //extracting files
        ZipFile.ExtractToDirectory(Application.persistentDataPath + "/GHTV_Reloaded_update.zip", Application.persistentDataPath + "/", true);
        StartCoroutine(CopyGHLUpdate());
    }

    public IEnumerator CopyGHLUpdate()
    {
        yield return new WaitForEndOfFrame();

        string region = "";
        bool vailidRegion = true;
        string SFOfile = "";
        string strCache = t.getText("STR_APPLYUPDATEMSG");
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
            load = Instantiate(LoadingBox);
            load.GetComponent<GUI_MessageBox>().title = t.getText("STR_APPLYUPDATE");
            load.GetComponent<GUI_MessageBox>().message = strCache;

            string sourceDir = $"{Application.persistentDataPath}/GHTVReloadedUpdate/RPCS3";
            string destinationDir = $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/";

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
                load.GetComponent<GUI_MessageBox>().message = $"{strCache}\n\n{percentComplete}%";
                load.GetComponent<GUI_MessageBox>().addmessage();
                yield return new WaitForEndOfFrame();
                File.Copy(newPath, newPath.Replace(sourceDir, destinationDir), true);
                Debug.Log($"COPY FILE:{newPath} TO {newPath.Replace(sourceDir, destinationDir)}");
                count++;
            }
            File.Copy(SFOfile, $"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/PARAM.SFO", true); //copying parma.SFO (each region has there own version)
            load.GetComponent<GUI_MessageBox>().CloseAnim();
            GameObject n = Instantiate(MessageBox);
            n.GetComponent<GUI_MessageBox>().title = t.getText("STR_GAMEUPDATEDONE");
            n.GetComponent<GUI_MessageBox>().message = t.getText("STR_GAME_UP_TO_DATE");
        }
    }

    private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required
        Debug.Log("[MainMenu] Download Recevedbyte: " + e.BytesReceived + " Download totalbyte: " + e.TotalBytesToReceive);
        // use these variables if needed
        load.GetComponent<GUI_MessageBox>().message = $"{t.getText("STR_DOWNLOAD_GHLR_UPDATE")}\n\n {t.getText("STR_DOWNLOADED")} {e.ProgressPercentage}%";
        load.GetComponent<GUI_MessageBox>().addmessage();
    }


    private IEnumerator getmotd()
    {
        downloadedMOTD = false;

        //checking if we don't have motd already save
        if (File.Exists(Application.persistentDataPath + "/motd.xml"))
        {
            File.Delete(Application.persistentDataPath + "/motd.xml");
        }

        //grabing motd from github
        //download the latest
        Debug.Log("[MainMenu] DOWNLOADING MOTD");
        yield return new WaitUntil(() => webClient == null);
        btnDownload_Click(2, "https://raw.githubusercontent.com/Nathan31973/GHTV-Reloaded-Mods-Tools-Assets/main/motd.xml", Application.persistentDataPath + "/motd.xml");
        yield return new WaitUntil(() => downloadedMOTD == true);
        //read it
        string text = "";
        bool display = false;
        long highestfrom = 0;
        int heighestpriority = 0;
        if (File.Exists(Application.persistentDataPath + "/motd.xml"))
        {
            XDocument xmlFile = XDocument.Load(Application.persistentDataPath + "/motd.xml");

            var dif = from c in xmlFile.Elements("Motd").Elements("MessageDefinition")
                      select c;

            foreach (XElement c in dif)
            {
                yield return new WaitForEndOfFrame();
                if ((bool)c.Attribute("IsEnabled"))
                {
                    if (c.Element("Message").Attribute("Language").Value == t.lang)//put lang selection here
                    {
                        if ((long)c.Attribute("DisplayFrom") <= DateTimeOffset.Now.ToUnixTimeMilliseconds() && (long)c.Attribute("DisplayFrom") > highestfrom)
                        {
                            if ((long)c.Attribute("DisplayUntil") >= DateTimeOffset.Now.ToUnixTimeMilliseconds() && (long)c.Attribute("DisplayFrom") > UpdateMOTD)
                            {
                                if((int)c.Attribute("Priority") >= heighestpriority)
                                {
                                    highestfrom = (long)c.Attribute("DisplayFrom");
                                    UpdateMOTD = (long)c.Attribute("DisplayUntil");
                                    heighestpriority = (int)c.Attribute("Priority");
                                    text = c.Element("Message").Value;
                                    display = true;
                                }
                            }
                        }
                    }
                }
            }
            firstMOTDCheck = true;
        }
        
        if (!display)
        {
            Debug.Log("[MainMenu] No MOTD found, dissable MOTD");
            MOTD.SetActive(false);
            motdText.text = text;
        }
        else
        {
            //removing motd from catch
            if(File.Exists(Application.persistentDataPath + "/motd.xml"))
            {
                File.Delete(Application.persistentDataPath + "/motd.xml");
            }
            Debug.Log("[MainMenu] MOTD found, enable MOTD");
            MOTD.SetActive(true);
            motdText.text = " " + text;
            MOTD.GetComponent<Scrollingtext>().UpdateText();

        }
    }


    private void CompletedConver3(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.Log("[MainMenu] Download completed!");
        downloadedMOTD = true;
    }

    private void OnApplicationQuit()
    {
        //removing motd from catch
        if (File.Exists(Application.persistentDataPath + "/motd.xml"))
        {
            File.Delete(Application.persistentDataPath + "/motd.xml");
        }
    }

}