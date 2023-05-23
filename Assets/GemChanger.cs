using Ini;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class GemChanger : MonoBehaviour
{
    private WebClient webClient = null;
    public GameObject UI;
    public GameObject messageBox;
    public GameObject messageBox2;
    public GameObject loadingBox;
    public GameObject messageBoxWithText;
    public GameData GemcreatorMenu;
    public GameObject fadeToBlack;
    private GameObject load;
    public Button openstrumbutton;
    public Button trailsButton;
    private GameObject Textbox;

    [Header("UI")]
    public Toggle ModeToggleSwitch;

    public Toggle gemBoxToggle;
    public Animation black;

    public NoteType noteType;

    public enum NoteType
    {
        Normal,
        HeroPowerCollect,
        HeroPowerActive,
        Hopo,
        Streak
    }

    [Header("Settings")]
    public int TextureWidth = 256;

    public int TextureHeight = 256;

    [Header("gems")]
    public GameObject Gem;

    public GameObject OpenGem;
    public GameObject OpenGemOther;

    public GameObject GemBox;

    public List<GameObject> Trails;

    [Header("Default Gem Texture")]
    public Texture openGemDefault;

    public Texture openGemHeroActiveDefault;
    public Texture openGemHeroCollectDefault;
    public Texture openGemStreakDefault;

    public Texture gemRightDefault;
    public Texture gemLeftDefault;

    public Texture gemRightHeroActiveDefault;
    public Texture gemLeftHeroActiveDefault;

    public Texture gemRightHeroCollectDefault;
    public Texture gemLeftHeroCollectDefault;

    public Texture gemRightHopoDefault;
    public Texture gemLeftHopoDefault;

    public Texture gemRightStreakDefault;
    public Texture gemLeftStreakDefault;

    public Texture gemBoxDefault;

    public Texture trailDefault;
    public Texture trailHeroDefault;

    [Header("User Texture")]
    public Texture openGem;

    public Texture openGemHeroActive;
    public Texture openGemHeroCollect;
    public Texture openGemStreak;

    public Texture gemLeft;
    public Texture gemRight;

    public Texture gemRightHeroActive;
    public Texture gemLeftHeroActive;

    public Texture gemRightHeroCollect;
    public Texture gemLeftHeroCollect;

    public Texture gemRightHopo;
    public Texture gemLeftHopo;

    public Texture gemRightSreak;
    public Texture gemLeftStreak;

    public Texture gemBox;

    public Texture trail;
    public Texture trailHero;

    public bool isDownloading = false;
    public List<string> Highways;
    public List<string> OnDiskHighways;
    private bool DoneConverting = false;
    private bool userWantToConvert = false;
    private Translater T;

    //private void Awake()
    //{
    //    //make highway list all uppercase
    //    for (int i = 0; i < Highways.Count; i++)
    //    {
    //        Highways[i] = Highways[i].ToUpper();
    //    }
    //}
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //clean up
            Directory.GetFiles(Application.persistentDataPath + $"/GemMaker/Texture/", "*.*")
                     .Where(item => item.EndsWith(".png") || item.EndsWith(".img"))
                     .ToList()
                     .ForEach(item => File.Delete(item));

            GameObject a = Instantiate(fadeToBlack);
            a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
            a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
            a.GetComponent<FadeToBlack>().anim.Play();
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        //we need the highway names to be caps
        T = Translater.instance;
        Highways = Highways.ConvertAll(d => d.ToUpper());
        resetGemToDefault();
        DiscordController.instance.UpdateDiscordActivity("Gem Changer", "Selection an Option", "gem_changer", "Gem Changer");
    }

    public void StartGemChanger()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) // || Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor
        {
            DiscordController.instance.UpdateDiscordActivity("Gem Changer", "Creating Gems", "gem_changer", "Gem Changer");

            if (Directory.Exists(Application.persistentDataPath + "\\External_Tools\\GHLIMGConverter-master"))
            {
                if (!Directory.Exists(Application.persistentDataPath + "\\External_Tools\\FARtools-master"))
                {
                    getFarTools();
                }
                if (PVRTexToolInstall() == "INI")
                {
                    main();
                }
                else
                {
                    Step2();
                }
            }
            else
            {
                btnDownload_Click("https://github.com/AnthoChamb/GHLIMGConverter/archive/refs/heads/master.zip", "GHLIMGConverter.zip");
            }
        }
        else
        {
            Debug.Log("[GemChanger] Error. Platform isn't supported");
            Debug.Log("[GemChanger] " + Application.platform);
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = T.getText("STR_ERROR");
            a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_PLATFORM_ISNT_SUPPORTED");
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void resetGemToDefault()
    {
        gemLeft = gemLeftDefault;
        gemRight = gemRightDefault;

        openGem = openGemDefault;
        openGemHeroActive = openGemHeroActiveDefault;
        openGemHeroCollect = openGemHeroCollectDefault;
        openGemStreak = openGemStreakDefault;

        gemLeftHeroCollect = gemLeftHeroCollectDefault;
        gemRightHeroCollect = gemRightHeroCollectDefault;

        gemLeftHeroActive = gemLeftHeroActiveDefault;
        gemRightHeroActive = gemRightHeroActiveDefault;

        gemLeftHopo = gemLeftHopoDefault;
        gemRightHopo = gemRightHopoDefault;

        gemLeftStreak = gemLeftStreakDefault;
        gemRightSreak = gemRightStreakDefault;

        gemBox = gemBoxDefault;

        trail = trailDefault;
        trailHero = trailHeroDefault;
        UpdateGem();
    }

    public void ReturnToMainMenu()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

    private void btnDownload_Click(string url, string filename)
    {
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[GemChanger] Error. Check internet connection!");
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = T.getText("NET_COM_NONE_ACTIVE");
            a.GetComponent<GUI_MessageBox>().message = T.getText("NET_COM_NONE_ACTIVE_DES");
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
            return;
        }
        else
        {
            Debug.Log("[GemChanger] We have internet");

            // Is file downloading yet?
            if (webClient != null)
                return;
            if (filename == "GHLIMGConverter.zip")
            {
                isDownloading = true;
                StartCoroutine(Waitfordownload(0));
                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                webClient.QueryString.Add("file", Application.persistentDataPath + $"\\{filename}"); // To identify the file
                webClient.DownloadFileAsync(new Uri($"{url}"), Application.persistentDataPath + $"\\{filename}");
            }
            else if (filename == "PowerVRSDKSetup-4.0.exe" || filename == "PowerVRSDKSetup-4.0.run-x64")
            {
                isDownloading = true;
                StartCoroutine(Waitfordownload(1));
                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedPVR);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                webClient.QueryString.Add("file", Application.persistentDataPath + $"\\{filename}"); // To identify the file
                webClient.DownloadFileAsync(new Uri($"{url}"), Application.persistentDataPath + $"\\{filename}");
            }
            else if (filename == "fartools.zip")
            {
                isDownloading = true;
                StartCoroutine(Waitfordownload(2));
                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedFartool);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                webClient.QueryString.Add("file", Application.persistentDataPath + $"\\{filename}"); // To identify the file
                webClient.DownloadFileAsync(new Uri($"{url}"), Application.persistentDataPath + $"\\{filename}");
            }
        }
    }

    private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required
        Debug.Log("[GemChanger] Download Recevedbyte: " + e.BytesReceived + " Download totalbyte: " + e.TotalBytesToReceive);
        // use these variables if needed
        load.GetComponent<GUI_MessageBox>().message = $"{T.getText("NET_COM_DOWNLOADING_EXTERNAL")}\n\n{T.getText("STR_DOWNLOADED")} {e.ProgressPercentage}%";
        load.GetComponent<GUI_MessageBox>().addmessage();
    }

    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        isDownloading = false;
        Debug.Log("[GemChanger] Download completed!");

        extractFile("GHLIMGConverter.zip");
    }

    private void CompletedFartool(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        isDownloading = false;
        Debug.Log("[GemChanger] Download completed!");

        extractFile("fartools.zip");
    }

    private void CompletedPVR(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        isDownloading = false;
        Debug.Log("[GemChanger] Download completed!");
    }

    private void extractFile(string filename)
    {
        if (!Directory.Exists(Application.persistentDataPath + "\\External_Tools"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "\\External_Tools");
        }
        ZipFile.ExtractToDirectory(Application.persistentDataPath + $"\\{filename}", Application.persistentDataPath + "\\External_Tools");
        File.Delete(Application.persistentDataPath + $"\\{filename}");
        if (filename == "GHLIMGConverter.zip")
        {
            Step2();
        }
        else if (filename == "fartools.zip")
        {
            main();
        }
    }

    private string PVRTexToolInstall()
    {
        var ini = new IniFile(Application.persistentDataPath + "\\External_Tools\\GHLIMGConverter-master\\config.ini");
        string a = ini.IniReadValue("path", "PVRTexToolCLI");
        if (a.Contains("PVRTexToolCLI.exe"))
        {
            Debug.Log("[GemChanger] User has PVRTextToolCLIInstalled");
            return "INI";
        }
        else
        {
            return "NO";
        }
    }

    private void Step2()
    {
        GameObject a = Instantiate(messageBox);
        a.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_EXT_PT1");
        a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_EXT_PT1_DES");
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(WaitForUserToInstall);
    }

    public void WaitForUserToInstall()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            btnDownload_Click("https://cdn.imgtec.com/sdk/PowerVRSDKSetup-4.0.exe", "PowerVRSDKSetup-4.0.exe");
        }
        else if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor)
        {
            btnDownload_Click("https://cdn.imgtec.com/sdk/PowerVRSDKSetup-4.0.run-x64", "PowerVRSDKSetup-4.0.run-x64");
        }
    }

    private IEnumerator Waitfordownload(int type)
    {
        load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("STR_DOWNLOAD");
        load.GetComponent<GUI_MessageBox>().message = T.getText("NET_COM_DOWNLOADING_EXTERNAL");
        yield return new WaitUntil(() => isDownloading == false);
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        yield return new WaitForSeconds(1f);
        if (type == 1)
        {
            Application.OpenURL(Application.persistentDataPath);
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_EXT_PT2");
            a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_EXT_PT2_DES");
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(OpenFileExplore);
        }
    }

    public void OpenFileExplore()
    {
        if (File.Exists("C:\\Imagination Technologies\\PowerVR_Graphics\\PowerVR_Tools\\PVRTexTool\\CLI\\Windows_x86_64\\PVRTexToolCLI.exe"))
        {
            var ini = new IniFile(Application.persistentDataPath + "\\External_Tools\\GHLIMGConverter-master\\config.ini");
            string a = ini.IniReadValue("path", "PVRTexToolCLI");
            a = " C:\\Imagination Technologies\\PowerVR_Graphics\\PowerVR_Tools\\PVRTexTool\\CLI\\Windows_x86_64\\PVRTexToolCLI.exe";
            ini.IniWriteValue("path", "PVRTexToolCLI", a);
            GameObject b = Instantiate(messageBox);
            b.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_EXT_TOOL_FOUND");
            b.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_EXT_TOOL_FOUND_DES");
            b.GetComponent<GUI_MessageBox>().button.onClick.AddListener(getFarTools);
        }
        else if (File.Exists("C:\\Imagination\\PowerVR_Graphics\\PowerVR_Tools\\PVRTexTool\\CLI\\Windows_x86_64\\PVRTexToolCLI.exe"))
        {
            var ini = new IniFile(Application.persistentDataPath + "\\External_Tools\\GHLIMGConverter-master\\config.ini");
            string a = ini.IniReadValue("path", "PVRTexToolCLI");
            a = " C:\\Imagination\\PowerVR_Graphics\\PowerVR_Tools\\PVRTexTool\\CLI\\Windows_x86_64\\PVRTexToolCLI.exe";
            ini.IniWriteValue("path", "PVRTexToolCLI", a);
            GameObject b = Instantiate(messageBox);
            b.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_EXT_TOOL_FOUND");
            b.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_EXT_TOOL_FOUND_DES");
            b.GetComponent<GUI_MessageBox>().button.onClick.AddListener(getFarTools);
        }
        else
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Locate PVRTexToolCLI", "C://", "exe", false);
            foreach (var path in paths)
            {
                if (path.Contains("PVRTexToolCLI"))
                {
                    var ini = new IniFile(Application.persistentDataPath + "\\External_Tools\\GHLIMGConverter-master\\config.ini");
                    string a = ini.IniReadValue("path", "PVRTexToolCLI");
                    a = path;
                    ini.IniWriteValue("path", "PVRTexToolCLI", " " + a);
                    GameObject c = Instantiate(messageBox);
                    c.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_EXT_TOOL_FOUND");
                    c.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_EXT_TOOL_FOUND_DES");
                    c.GetComponent<GUI_MessageBox>().button.onClick.AddListener(getFarTools);
                }
                else
                {
                    GameObject a = Instantiate(messageBox);
                    a.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_EXT_TOOL_REQ");
                    a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_EXT_TOOL_REQ_DES");
                    a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(OpenFileExplore);
                }
            }
        }
    }

    public void getFarTools()
    {
        btnDownload_Click("https://codeload.github.com/SUOlivia/FARtools/zip/refs/heads/master", "fartools.zip");
    }

    public void main()
    {
        black.GetComponent<Animation>().Play("FadeFromBlack");
    }

    public void ChangeLeftHandedImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(T.getText("GEM_CREATOR_SEL_LEFT_HND"), "", "png", false);
        foreach (var path in paths)
        {
            var loader = new WWW(path);
            if (!CheckUploadedSize(loader))
            {
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = T.getText("STR_INVALID_IMG");
                a.GetComponent<GUI_MessageBox>().message = T.getText("STR_IMG_TOOLTIP");
                return;
            }
            ModeToggleSwitch.isOn = true;
            if (noteType == NoteType.Normal)
            {
                gemLeft = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemLeft.png", true);
            }
            else if (noteType == NoteType.HeroPowerCollect)
            {
                gemLeftHeroCollect = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemLeftHeroPowerCollect.png", true);
            }
            else if (noteType == NoteType.HeroPowerActive)
            {
                gemLeftHeroActive = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemLeftHeroPowerActive.png", true);
            }
            else if (noteType == NoteType.Hopo)
            {
                gemLeftHopo = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemLeftHopo.png", true);
            }
            else if (noteType == NoteType.Streak)
            {
                gemLeftStreak = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemLeftStreak.png", true);
            }
            UpdateGem();
        }
    }

    public void ChangeOpenStrumImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(T.getText("GEM_CREATOR_SEL_OPEN"), "", "png", false);
        foreach (var path in paths)
        {
            var loader = new WWW(path);
            if (!CheckUploadedSize(loader))
            {
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = T.getText("STR_INVALID_IMG");
                a.GetComponent<GUI_MessageBox>().message = T.getText("STR_IMG_TOOLTIP");
                return;
            }
            if (noteType == NoteType.Normal)
            {
                openGem = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\OpenGem.png", true);
            }
            else if (noteType == NoteType.HeroPowerCollect)
            {
                openGemHeroCollect = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\OpenGemHeroPowerCollect.png", true);
            }
            else if (noteType == NoteType.HeroPowerActive)
            {
                openGemHeroActive = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\OpenGemHeroPowerActive.png", true);
            }
            else if (noteType == NoteType.Streak)
            {
                openGemStreak = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\OpenGemStreak.png", true);
            }
            UpdateGem();
        }
    }

    public void ChangeGemBoxImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(T.getText("GEM_CREATOR_SEL_BOX"), "", "png", false);
        foreach (var path in paths)
        {
            var loader = new WWW(path);
            if (!CheckUploadedSize(loader))
            {
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = T.getText("STR_INVALID_IMG");
                a.GetComponent<GUI_MessageBox>().message = T.getText("STR_IMG_TOOLTIP");
                return;
            }

            gemBox = loader.texture;
            gemBoxToggle.isOn = true;
            if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
            }
            File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemBox.png", true);

            UpdateGem();
        }
    }

    public void ChangeTrailImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(T.getText("GEM_CREATOR_SEL_TRAIL"), "", "png", false);
        foreach (var path in paths)
        {
            var loader = new WWW(path);
            if (!CheckUploadedSizeTrail(loader))
            {
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = T.getText("STR_INVALID_IMG");
                a.GetComponent<GUI_MessageBox>().message = T.getText("STR_IMG_TOOLTIP_64");
                return;
            }
            //loader.texture.alphaIsTransparency = false; // this is a lie
            if (noteType == NoteType.Normal)
            {
                trail = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\Trail.png", true);
            }
            else if (noteType == NoteType.HeroPowerActive)
            {
                trailHero = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\TrailHero.png", true);
            }
            UpdateGem();
        }
    }

    public void ChangeRightHandedImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(T.getText("GEM_CREATOR_SEL_RIGHT_HND"), "", "png", false);
        foreach (var path in paths)
        {
            var loader = new WWW(path);
            if (!CheckUploadedSize(loader))
            {
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = T.getText("STR_INVALID_IMG");
                a.GetComponent<GUI_MessageBox>().message = T.getText("STR_IMG_TOOLTIP");
                return;
            }

            if (noteType == NoteType.Normal)
            {
                gemRight = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemRight.png", true);
            }
            else if (noteType == NoteType.HeroPowerCollect)
            {
                gemRightHeroCollect = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemRightHeroPowerCollect.png", true);
            }
            else if (noteType == NoteType.HeroPowerActive)
            {
                gemRightHeroActive = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemRightHeroPowerActive.png", true);
            }
            else if (noteType == NoteType.Hopo)
            {
                gemRightHopo = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemRightHopo.png", true);
            }
            else if (noteType == NoteType.Streak)
            {
                gemRightSreak = loader.texture;
                if (!Directory.Exists(Application.persistentDataPath + @"/GemMaker/Texture"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + @"/GemMaker/Texture");
                }
                File.Copy(path, Application.persistentDataPath + "\\GemMaker\\Texture\\GemRightStreak.png", true);
            }
            UpdateGem();
            ModeToggleSwitch.isOn = false;
        }
    }

    public void DropDownGemType(int num)
    {
        noteType = (NoteType)num;
        UpdateGem();
    }

    private void UpdateGem()
    {
        OpenGem.GetComponent<Renderer>().material.mainTexture = openGem;
        GemBox.GetComponent<Renderer>().material.mainTexture = gemBox;
        if (gemBoxToggle.isOn)
        {
            GemBox.SetActive(true);
        }
        else
        {
            GemBox.SetActive(false);
        }
        if (noteType == NoteType.Normal)
        {
            foreach (GameObject trailOBJ in Trails)
            {
                trailOBJ.SetActive(true);
                trailOBJ.GetComponent<Renderer>().material.mainTexture = trail;
            }
            trailsButton.interactable = true;
            OpenGem.SetActive(true);
            OpenGemOther.SetActive(false);
            openstrumbutton.interactable = true;
            OpenGem.GetComponent<Renderer>().material.mainTexture = openGem;
            if (ModeToggleSwitch.isOn == false)
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemRight;
            }
            else
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemLeft;
            }
        }
        else if (noteType == NoteType.HeroPowerCollect)
        {
            foreach (GameObject trailOBJ in Trails)
            {
                trailOBJ.SetActive(false);
            }
            trailsButton.interactable = false;
            OpenGem.SetActive(false);
            OpenGemOther.SetActive(true);
            openstrumbutton.interactable = true;
            OpenGemOther.GetComponent<Renderer>().material.mainTexture = openGemHeroCollect;
            if (ModeToggleSwitch.isOn == false)
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemRightHeroCollect;
            }
            else
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemLeftHeroCollect;
            }
        }
        else if (noteType == NoteType.HeroPowerActive)
        {
            foreach (GameObject trailOBJ in Trails)
            {
                trailOBJ.SetActive(true);
                trailOBJ.GetComponent<Renderer>().material.mainTexture = trailHero;
            }
            trailsButton.interactable = true;
            OpenGem.SetActive(false);
            OpenGemOther.SetActive(true);
            openstrumbutton.interactable = true;
            OpenGemOther.GetComponent<Renderer>().material.mainTexture = openGemHeroActive;
            if (ModeToggleSwitch.isOn == false)
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemRightHeroActive;
            }
            else
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemLeftHeroActive;
            }
        }
        else if (noteType == NoteType.Hopo)
        {
            foreach (GameObject trailOBJ in Trails)
            {
                trailOBJ.SetActive(false);
            }
            trailsButton.interactable = false;
            OpenGem.SetActive(false);
            OpenGemOther.SetActive(false);
            openstrumbutton.interactable = false;
            if (ModeToggleSwitch.isOn == false)
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemRightHopo;
            }
            else
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemLeftHopo;
            }
        }
        else if (noteType == NoteType.Streak)
        {
            foreach (GameObject trailOBJ in Trails)
            {
                trailOBJ.SetActive(false);
            }
            trailsButton.interactable = false;
            openstrumbutton.interactable = true;
            OpenGem.SetActive(false);
            OpenGemOther.SetActive(true);
            OpenGemOther.GetComponent<Renderer>().material.mainTexture = openGemStreak;
            if (ModeToggleSwitch.isOn == false)
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemRightSreak;
            }
            else
            {
                Gem.GetComponent<Renderer>().material.mainTexture = gemLeftStreak;
            }
        }
    }

    private bool CheckUploadedSize(WWW text)
    {
        if (text.texture.width == TextureWidth && text.texture.height == TextureHeight)
        {
            return true;
        }
        else return false;
    }

    private bool CheckUploadedSizeTrail(WWW text)
    {
        if (text.texture.width == 64 && text.texture.height == 64)
        {
            return true;
        }
        else return false;
    }


    public void ToggleSwitch(bool mode)
    {
        UpdateGem();
    }

    public void templates()
    {
        Application.OpenURL($"{Application.streamingAssetsPath}\\GEM_MAKER\\Textures");
    }

    public void finish()
    {
        if (userData.instance.LocalFilePath == "TEST")
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR RPCS3 FOLDER", "RPSC3 FOLDER", false);
            foreach (var path in paths)
            {
                if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
                {
                    Debug.Log($"[GemChanger] {path} Is valid");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"[GemChanger] {userData.instance.LocalFilePath} Is valid");
                    done();
                }
                else
                {
                    GameObject t = Instantiate(messageBox);
                    userData.instance.LocalFilePath = "TEST";
                    t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
                    t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
                }
            }
        }
        else if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
        {
            StartCoroutine(done());
        }
        else
        {
            GameObject t = Instantiate(messageBox);
            userData.instance.LocalFilePath = "TEST";
            t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
            t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
        }
    }

    public IEnumerator done()
    {
        load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("STR_CONVERTING");
        load.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_CONVERTING_TEX");

        string RighGemImg = Application.persistentDataPath + $"/GemMaker/Texture/GemRight";
        string LeftGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeft";

        string LeftHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHeroPowerActive";
        string RightHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHeroPowerActive";

        string LeftHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHeroPowerCollect";
        string RightHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHeroPowerCollect";

        string LeftStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftStreak";
        string RightStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightStreak";

        string LeftHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHopo";
        string RightHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHopo";

        string OpenGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGem";
        string OpenGemHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemHeroPowerCollect";
        string OpenGemHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemHeroPowerActive";
        string OpenGemStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemStreak";
        string GemBoxIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemBox";

        string trailIMG = Application.persistentDataPath + $"/GemMaker/Texture/Trail";
        string trailHeroIMG = Application.persistentDataPath + $"/GemMaker/Texture/TrailHero";

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
            Debug.LogError("[HopoPatch] USER HASN'T SELECTED A GAME REGION");
        }

        if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/PRIMARY/TEXTURES/"))
        {
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/PRIMARY/TEXTURES/");
        }
        if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/"))
        {
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/");
        }
        List<string> gembatchfiles = new List<string>();
        if (userData.instance.platform == userData.Platform.Rpcs3 || userData.instance.platform == userData.Platform.PS3 || userWantToConvert == true)
        {
            gembatchfiles.Add(BatchFileMaker(RighGemImg, RighGemImg, "GemRight", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftGemIMG, LeftGemIMG, "GemLeft", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftHeroActiveIMG, LeftHeroActiveIMG, "GemLeftHeroPowerActive", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(RightHeroActiveIMG, RightHeroActiveIMG, "GemRightHeroPowerActive", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftHeroCollectIMG, LeftHeroCollectIMG, "GemLeftHeroPowerCollect", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(RightHeroCollectIMG, RightHeroCollectIMG, "GemRightHeroPowerCollect", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftStreakIMG, LeftStreakIMG, "GemLeftStreak", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(RightStreakIMG, RightStreakIMG, "GemRightStreak", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftHopoIMG, LeftHopoIMG, "GemLeftHopo", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(RightHopoIMG, RightHopoIMG, "GemRightHopo", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(OpenGemIMG, OpenGemIMG, "OpenGem", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(OpenGemHeroCollectIMG, OpenGemHeroCollectIMG, "OpenGemHeroPowerCollect", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(OpenGemHeroActiveIMG, OpenGemHeroActiveIMG, "OpenGemHeroPowerActive", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(OpenGemStreakIMG, OpenGemStreakIMG, "OpenGemStreak", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(GemBoxIMG, GemBoxIMG, "GemBox", "BC3", 6, 256, 256, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(trailIMG, trailIMG, "Trail", "BC3", 4, 64, 64, "ps3"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(trailHeroIMG, trailHeroIMG, "TrailHero", "BC3", 4, 64, 64, "ps3"));
            yield return new WaitForEndOfFrame();
        }
        //ios
        if (userData.instance.platform == userData.Platform.Ios || userData.instance.platform == userData.Platform.TVOS || userWantToConvert == true)
        {
            gembatchfiles.Add(BatchFileMaker(RighGemImg, RighGemImg, "GemRight", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftGemIMG, LeftGemIMG, "GemLeft", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftHeroActiveIMG, LeftHeroActiveIMG, "GemLeftHeroPowerActive", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(RightHeroActiveIMG, RightHeroActiveIMG, "GemRightHeroPowerActive", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftHeroCollectIMG, LeftHeroCollectIMG, "GemLeftHeroPowerCollect", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(RightHeroCollectIMG, RightHeroCollectIMG, "GemRightHeroPowerCollect", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftStreakIMG, LeftStreakIMG, "GemLeftStreak", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(RightStreakIMG, RightStreakIMG, "GemRightStreak", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(LeftHopoIMG, LeftHopoIMG, "GemLeftHopo", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(RightHopoIMG, RightHopoIMG, "GemRightHopo", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(OpenGemIMG, OpenGemIMG, "OpenGem", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(OpenGemHeroCollectIMG, OpenGemHeroCollectIMG, "OpenGemHeroPowerCollect", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(OpenGemHeroActiveIMG, OpenGemHeroActiveIMG, "OpenGemHeroPowerActive", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(OpenGemStreakIMG, OpenGemStreakIMG, "OpenGemStreak", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(GemBoxIMG, GemBoxIMG, "GemBox", "BC1", 6, 512, 512, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(trailIMG, trailIMG, "Trail", "BC1", 4, 128, 128, "ios"));
            yield return new WaitForEndOfFrame();
            gembatchfiles.Add(BatchFileMaker(trailHeroIMG, trailHeroIMG, "TrailHero", "BC1", 4, 128, 128, "ios"));
        }
        int filesnum = gembatchfiles.Count;
        int filesFailed = 0;
        foreach (var file in gembatchfiles)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                if (file != null)
                {
                    runbatfile(file);
                    Debug.Log("[GemChanger] Running batfile: " + file);
                }
                else
                {
                    filesFailed++;
                    Debug.Log("[GemChanger] Batfile faild to located");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(file + "    " + e);
                filesFailed++;
                Debug.Log("[GemChanger] Batfile faild create gem IMG");
            }
        }
        if (filesFailed >= filesnum)
        {
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = T.getText("STR_CONVERT_FAIL");
            a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_CONVERTING_FAIL");
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
            yield return null;
        }
        if (!userWantToConvert)
        {
            load.GetComponent<GUI_MessageBox>().CloseAnim();
            yield return new WaitForSeconds(1);
            StartCoroutine(AddToGHL());
        }
        else if (userWantToConvert)
        {
            MakeFarFile(true);
            yield return new WaitForSeconds(3);
            load.GetComponent<GUI_MessageBox>().CloseAnim();
            yield return new WaitForSeconds(1);
            Textbox = Instantiate(messageBoxWithText);
            Textbox.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_CONVERT_DONE");
            Textbox.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_CONVERT_DONE_DES");
            Textbox.GetComponent<GemChanger_CreateTexturePack_MessageBox>().host = this;
            userWantToConvert = false;
        }
    }

    private void runbatfile(string batfile)
    {
        System.Diagnostics.ProcessStartInfo batinfo = new System.Diagnostics.ProcessStartInfo();
        batinfo.FileName = ($"{batfile}");
        batinfo.WorkingDirectory = Path.GetDirectoryName($"{batfile}");
        System.Diagnostics.Process bat = System.Diagnostics.Process.Start(batinfo);
        bat.WaitForExit();
        File.Delete(batfile);
        Debug.Log("[GemChanger] bat file done");
    }

    private IEnumerator AddToGHL()
    {
        load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("STR_EXPORTING");
        load.GetComponent<GUI_MessageBox>().message = T.getText("GEM_IMPORTER_EXPORTING");
        yield return new WaitForSeconds(8);

        //checking if img files exist
        //if(!File.Exists($"{Application.persistentDataPath}/GemMaker/Texture/GemRight.IMG") | !File.Exists($"{Application.persistentDataPath}/GemMaker/Texture/GemLeft.IMG"))
        //{
        //    GameObject a = Instantiate(messageBox);
        //    a.GetComponent<GUI_MessageBox>().title = "Failed To Convert Texture";
        //    a.GetComponent<GUI_MessageBox>().message = $"Somthing went wrong while we try to convert the texture to Guitar Hero Live Texture\n\nPlease try again\n\nError Code: PYfailed";
        //    a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        //    yield break;
        //}
        //yield return new WaitUntil(() => DoneConverting == true);
        //setting platform

        var platform = "";

        //create ios files if ios is selected
        if (userData.instance.platform == userData.Platform.Ios || userData.instance.platform == userData.Platform.TVOS)
        {
            //ios/tvos stuff
            MakeFarFile(false);
            platform = "ios";
        }
        else if (userData.instance.platform == userData.Platform.WiiU)
        {
            platform = "wiiu";
        }
        else if (userData.instance.platform == userData.Platform.Rpcs3 || userData.instance.platform == userData.Platform.PS3)
        {
            platform = "ps3";
            //files names
            string RighGemImg = Application.persistentDataPath + $"/GemMaker/Texture/GemRight_" + platform;
            string LeftGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeft_" + platform;

            string LeftHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHeroPowerActive_" + platform;
            string RightHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHeroPowerActive_" + platform;

            string LeftHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHeroPowerCollect_" + platform;
            string RightHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHeroPowerCollect_" + platform;

            string LeftStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftStreak_" + platform;
            string RightStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightStreak_" + platform;

            string LeftHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHopo_" + platform;
            string RightHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHopo_" + platform;

            string OpenGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGem_" + platform;
            string OpenGemHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemHeroPowerCollect_" + platform;
            string OpenGemHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemHeroPowerActive_" + platform;
            string OpenGemStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemStreak_" + platform;
            string GemBoxIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemBox_" + platform;

            string trailIMG = Application.persistentDataPath + $"/GemMaker/Texture/Trail_" + platform;
            string trailHeroIMG = Application.persistentDataPath + $"/GemMaker/Texture/TrailHero_" + platform;

            //for updating from older version of the tool
            string version = "";
            if (userData.instance.gameVersion == userData.Version.PAL || userData.instance.gameVersion == userData.Version.Lite)
            {
                version = "BLES02180";
            }
            else if (userData.instance.gameVersion == userData.Version.USA)
            {
                version = "BLUS31556";
            }
            else
            {
                Debug.LogError("[HopoPatch] USER HASN'T SELECTED A GAME REGION");
            }
            //all highways of GHL
            int num = 0;
            int assets = Highways.Count + OnDiskHighways.Count + 2;
            //removing the old gems if placed
            Debug.Log("[GemChanger] Removing old gems");

            if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS"))
            {
                Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS", true);
            }
            string cache = $"{T.getText("GEM_IMPORTER_EXPORTING")}\n\n{T.getText("STR_CONVERTED")} ";

            //trails
            //CHECKING IF Folder EXIST
            if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/"))
            {
                Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/");
            }
            if (File.Exists($"{trailHeroIMG}.img"))
            {
                File.Copy($"{trailHeroIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/TRAIL_CYAN.IMG", true);
            }
            num++;
            if (File.Exists($"{trailIMG}.img"))
            {
                File.Copy($"{trailIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}//USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/TRAIL_GREY.IMG", true);
            }
            num++;

            //Highway exceptions
            //for some reason some highways have the right foldername but have the wrong file name
            string[] highwaysblacklist = { "RETROTRIANGLESA" };
            string[] highwaysblacklistMain = { "RETROTRIANGLES" };

            foreach (string highway in Highways)
            {
                //highways with weird files names
                string highwaya = highway;
                for (int i = 0; i < highwaysblacklist.Length; i++)
                {
                    if (highwaysblacklistMain[i] == highway)
                    {
                        highwaya = highwaysblacklist[i];
                    }
                }

                yield return new WaitForEndOfFrame();
                load.GetComponent<GUI_MessageBox>().message = $"{cache}{num}/{assets}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                num++;
                //CHECKING IF Folder EXIST
                if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/"))
                {
                    Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/");
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.img"))
                {
                    File.Copy($"{RighGemImg}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEM_COL_SRGB_00.1001.IMG", true);
                    if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEM_COL_SRGB_00.1001.IMG"))
                    {
                        Debug.Log($"[GemChangerImporter] WE COPY GEM_{highway}_GEM_COL_SRGB_00.1001.IMG TO RPCS3");
                    }
                }

                if (File.Exists($"{LeftGemIMG}.img"))
                {
                    File.Copy($"{LeftGemIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroActiveIMG}.img"))
                {
                    File.Copy($"{LeftHeroActiveIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROACTIVELEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHeroActiveIMG}.img"))
                {
                    File.Copy($"{RightHeroActiveIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroCollectIMG}.img"))
                {
                    File.Copy($"{LeftHeroCollectIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROCOLLECTLEFT_COL_SRGB_00.1001.IMG", true);
                }
                if (File.Exists($"{RightHeroCollectIMG}.img"))
                {
                    File.Copy($"{RightHeroCollectIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftStreakIMG}.img"))
                {
                    File.Copy($"{LeftStreakIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMSTREAKLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightStreakIMG}.img"))
                {
                    File.Copy($"{RightStreakIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHopoIMG}.img"))
                {
                    File.Copy($"{LeftHopoIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHOPOLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHopoIMG}.img"))
                {
                    File.Copy($"{RightHopoIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHOPO_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemIMG}.img"))
                {
                    File.Copy($"{OpenGemIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUM_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroCollectIMG}.img"))
                {
                    File.Copy($"{OpenGemHeroCollectIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroActiveIMG}.img"))
                {
                    File.Copy($"{OpenGemHeroActiveIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemStreakIMG}.img"))
                {
                    File.Copy($"{OpenGemStreakIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{GemBoxIMG}.img"))
                {
                    File.Copy($"{GemBoxIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/CATCHER_{highwaya}_BOX_COL_SRGB_00.1001.IMG", true);
                }
            }

            foreach (string highway in OnDiskHighways)
            {
                yield return new WaitForEndOfFrame();
                load.GetComponent<GUI_MessageBox>().message = $"{cache}{num}/{assets}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                num++;

                //CHECKING IF Folder EXIST
                if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/"))
                {
                    Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/");
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.img"))
                {
                    File.Copy($"{RighGemImg}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEM_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftGemIMG}.img"))
                {
                    File.Copy($"{LeftGemIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroActiveIMG}.img"))
                {
                    File.Copy($"{LeftHeroActiveIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHEROACTIVELEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHeroActiveIMG}.img"))
                {
                    File.Copy($"{RightHeroActiveIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroCollectIMG}.img"))
                {
                    File.Copy($"{LeftHeroCollectIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHEROCOLLECTLEFT_COL_SRGB_00.1001.IMG", true);
                }
                if (File.Exists($"{RightHeroCollectIMG}.img"))
                {
                    File.Copy($"{RightHeroCollectIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftStreakIMG}.img"))
                {
                    File.Copy($"{LeftStreakIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMSTREAKLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightStreakIMG}.img"))
                {
                    File.Copy($"{RightStreakIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHopoIMG}.img"))
                {
                    File.Copy($"{LeftHopoIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHOPOLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHopoIMG}.img"))
                {
                    File.Copy($"{RightHopoIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHOPO_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemIMG}.img"))
                {
                    File.Copy($"{OpenGemIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_STRUM_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroCollectIMG}.img"))
                {
                    File.Copy($"{OpenGemHeroCollectIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_STRUMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroActiveIMG}.img"))
                {
                    File.Copy($"{OpenGemHeroActiveIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_STRUMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemStreakIMG}.img"))
                {
                    File.Copy($"{OpenGemStreakIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_STRUMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{GemBoxIMG}.img"))
                {
                    File.Copy($"{GemBoxIMG}.img", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/CATCHER_{highway}_BOX_COL_SRGB_00.1001.IMG", true);
                }

                //clean up
                Directory.GetFiles(Application.persistentDataPath + $"/GemMaker/Texture/", "*.*")
                         .Where(item => item.EndsWith(".png") || item.EndsWith(".img"))
                         .ToList()
                         .ForEach(item => File.Delete(item));

                load.GetComponent<GUI_MessageBox>().CloseAnim();
                yield return new WaitForSeconds(1);
                //finish message
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = T.getText("STR_EXPORTDONE");
                a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_IMPORTER_EXPORT_DONE");
                a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
                Debug.LogWarning("[GemChanger] Finish exporting texture");
            }
        }
        yield return new WaitForEndOfFrame();
    }

    private void MakeFarFile(bool packoverride)
    {
        List<string> gembatchfiles = new List<string>();
        //copy our template files
        try
        {
            File.Copy($"{Application.streamingAssetsPath}/GEM_MAKER/ios/hwlprimary.far", $"{Application.persistentDataPath}/External_Tools/FARtools-master/hwlprimary.far", true);
            File.Copy($"{Application.streamingAssetsPath}/GEM_MAKER/ios/hudguitar_common.far", $"{Application.persistentDataPath}/External_Tools/FARtools-master/hudguitar_common.far", true);
        }
        catch (Exception e)
        {
            load.GetComponent<GUI_MessageBox>().CloseAnim();
            //finish message
            GameObject g = Instantiate(messageBox);
            g.GetComponent<GUI_MessageBox>().title = T.getText("STR_CONVERT_FAIL");
            g.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_CONVERTING_FAIL");
            g.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
            Debug.LogError("[Game Creator] ERROR FAILED TO FIND DEFAULT IOS FAR FILES PLEASE REINSTALL THE MOD TOOLS");
            return;
        }
        //setting up bat files for ios
        string platform = "ios";

        string RighGemImg = Application.persistentDataPath + $"/GemMaker/Texture/GemRight_" + platform + ".img";
        try
        {
            File.Copy(RighGemImg, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemRight.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemRight", "hwlprimary.far", "gem_primarydevicehigh_gem_col_srgb_00.1001", "GemRight"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find gem right img");
        }
        string LeftGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeft_" + platform + ".img";
        try
        {
            File.Copy(LeftGemIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemLeft.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemLeft", "hwlprimary.far", "gem_primarydevicehigh_gemleft_col_srgb_00.1001", "GemLeft"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find gem left img");
        }
        string LeftHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHeroPowerActive_" + platform + ".img";
        try
        {
            File.Copy(LeftHeroActiveIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemLeftHeroPowerActive.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemLeftHeroPowerActive", "hwlprimary.far", "gem_primarydevicehigh_gemheroactiveleft_col_srgb_00.1001", "GemLeftHeroPowerActive"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemLeftHeroPowerActive img");
        }
        string RightHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHeroPowerActive_" + platform + ".img";
        try
        {
            File.Copy(RightHeroActiveIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemRightHeroPowerActive.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemRightHeroPowerActive", "hwlprimary.far", "gem_primarydevicehigh_gemheroactive_col_srgb_00.1001", "GemRightHeroPowerActive"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemRightHeroPowerActive img");
        }
        string LeftHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHeroPowerCollect_" + platform + ".img";
        try
        {
            File.Copy(LeftHeroCollectIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemLeftHeroPowerCollect.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemLeftHeroPowerCollect", "hwlprimary.far", "gem_primarydevicehigh_gemherocollectleft_col_srgb_00.1001", "GemLeftHeroPowerCollect"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemLeftHeroPowerCollect img");
        }
        string RightHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHeroPowerCollect_" + platform + ".img";
        try
        {
            File.Copy(RightHeroCollectIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemRightHeroPowerCollect.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemRightHeroPowerCollect", "hwlprimary.far", "gem_primarydevicehigh_gemherocollect_col_srgb_00.1001", "GemRightHeroPowerCollect"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemRightHeroPowerCollect img");
        }
        string LeftStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftStreak_" + platform + ".img";
        try
        {
            File.Copy(LeftStreakIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemLeftStreak.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemLeftStreak", "hwlprimary.far", "gem_primarydevicehigh_gemstreakleft_col_srgb_00.1001", "GemLeftStreak"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemLeftStreak img");
        }
        string RightStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightStreak_" + platform + ".img";
        try
        {
            File.Copy(RightStreakIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemRightStreak.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemRightStreak", "hwlprimary.far", "gem_primarydevicehigh_gemstreak_col_srgb_00.1001", "GemRightStreak"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemRightStreak img");
        }
        string LeftHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHopo_" + platform + ".img";
        try
        {
            File.Copy(LeftHopoIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemLeftHopo.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemLeftHopo", "hwlprimary.far", "gem_primarydevicehigh_gemhopoleft_col_srgb_00.1001", "GemLeftHopo"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemLeftHopo img");
        }
        string RightHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHopo_" + platform + ".img";
        try
        {
            File.Copy(RightHopoIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemRightHopo.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemRightHopo", "hwlprimary.far", "gem_primarydevicehigh_gemhopo_col_srgb_00.1001", "GemRightHopo"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemRightHopo img");
        }
        string OpenGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGem_" + platform + ".img";
        try
        {
            File.Copy(OpenGemIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/OpenGem.img", true);
            gembatchfiles.Add(FarBatchFileMaker("OpenGem", "hwlprimary.far", "gem_primarydevicehigh_strum_col_srgb_00.1001", "OpenGem"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find OpenGem img");
        }
        string OpenGemHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemHeroPowerCollect_" + platform + ".img";
        try
        {
            File.Copy(OpenGemHeroCollectIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/OpenGemHeroPowerCollect.img", true);
            gembatchfiles.Add(FarBatchFileMaker("OpenGemHeroPowerCollect", "hwlprimary.far", "gem_primarydevicehigh_strumherocollect_col_srgb_00.1001", "OpenGemHeroPowerCollect"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find OpenGemHeroPowerCollect img");
        }
        string OpenGemHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemHeroPowerActive_" + platform + ".img";
        try
        {
            File.Copy(OpenGemHeroActiveIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/OpenGemHeroPowerActive.img", true);
            gembatchfiles.Add(FarBatchFileMaker("OpenGemHeroPowerActive", "hwlprimary.far", "gem_primarydevicehigh_strumheroactive_col_srgb_00.1001", "OpenGemHeroPowerActive"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find OpenGemHeroPowerActive img");
        }
        string OpenGemStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemStreak_" + platform + ".img";
        try
        {
            File.Copy(OpenGemStreakIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/OpenGemStreak.img", true);
            gembatchfiles.Add(FarBatchFileMaker("OpenGemStreak", "hwlprimary.far", "gem_primarydevicehigh_strumstreak_col_srgb_00.1001", "OpenGemStreak"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find OpenGemStreak img");
        }
        string GemBoxIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemBox_" + platform + ".img";
        try
        {
            File.Copy(GemBoxIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/GemBox.img", true);
            gembatchfiles.Add(FarBatchFileMaker("GemBox", "hwlprimary.far", "catcher_primarydevicehigh_box_col_srgb_00.1001", "GemBox"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find GemBox img");
        }
        string trailIMG = Application.persistentDataPath + $"/GemMaker/Texture/Trail_" + platform + ".img";
        try
        {
            File.Copy(trailIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/Trail.img", true);
            gembatchfiles.Add(FarBatchFileMaker("Trail", "hudguitar_common.far", "trail_grey", "Trail"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find Trail img");
        }
        string trailHeroIMG = Application.persistentDataPath + $"/GemMaker/Texture/TrailHero_" + platform + ".img";
        try
        {
            File.Copy(trailHeroIMG, $"{Application.persistentDataPath}/External_Tools/FARtools-master/TrailHero.img", true);
            gembatchfiles.Add(FarBatchFileMaker("TrailHero", "hudguitar_common.far", "trail_cyan", "TrailHero"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find TrailHero img");
        }
        try
        {
            File.Copy($"{Application.streamingAssetsPath}/GEM_MAKER/ios/confighud3x2_primarydevicehigh.xml", $"{Application.persistentDataPath}/External_Tools/FARtools-master/lightingfix.xml", true);
            gembatchfiles.Add(FarBatchFileMaker("lightingfix", "hwlprimary.far", "confighud3x2_primarydevicehigh", "lightingfix"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GemMaker] Failed to find lighting fix xml");
        }

        //clean up from the texture folder
        if (!packoverride)
        {
            Directory.GetFiles(Application.persistentDataPath + $"/GemMaker/Texture/", "*.*")
                     .Where(item => item.EndsWith(".png") || item.EndsWith(".img"))
                     .ToList()
                     .ForEach(item => File.Delete(item));
        }
        //running bat files so we can edit our far files
        foreach (var file in gembatchfiles)
        {
            try
            {
                if (file != null)
                {
                    runbatfile(file);
                    Debug.Log("[GemChanger] Running batfile: " + file);
                }
                else
                {
                    Debug.Log("[GemChanger] Batfile faild to located" + file);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"File {file} " + e.Message);
                Debug.Log("[GemChanger] Batfile faild add img to far");
            }
        }

        if (!packoverride)
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/GemMaker/iOS Device Gem Export"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/iOS Device Gem Export");
            }
            File.Copy($"{Application.persistentDataPath}/External_Tools/FARtools-master/hwlprimary.far", $"{Application.persistentDataPath}/GemMaker/iOS Device Gem Export/hwlprimary.far", true);
            File.Copy($"{Application.persistentDataPath}/External_Tools/FARtools-master/hudguitar_common.far", $"{Application.persistentDataPath}/GemMaker/iOS Device Gem Export/hudguitar_common.far", true);



            Debug.Log("[GemChanger] Done creating ios gems");
            load.GetComponent<GUI_MessageBox>().CloseAnim();
            //finish message
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = T.getText("STR_EXPORTDONE");
            a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_IMPORTER_EXPORT_DONE");
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);

            Application.OpenURL($"{Application.persistentDataPath}/GemMaker/iOS Device Gem Export");
            //clean up
            Directory.GetFiles(Application.persistentDataPath + $"/External_Tools/FARtools-master/", "*.*")
                     .Where(item => item.EndsWith(".far") || item.EndsWith(".img") || item.EndsWith(".xml"))
                     .ToList()
                     .ForEach(item => File.Delete(item));
        }
        else
        {
            //clean up
            Directory.GetFiles(Application.persistentDataPath + $"/External_Tools/FARtools-master/", "*.*")
                     .Where(item => item.EndsWith(".img") || item.EndsWith(".xml"))
                     .ToList()
                     .ForEach(item => File.Delete(item));
        }
        Debug.LogWarning("[GemChanger] Finish exporting texture");
    }

    public string FarBatchFileMaker(string batname, string farFile, string fileInFar, string replaceFile)
    {
        //finding python exe
        string[] pyhtonsver = { "Python310", "Python311", "Python39", "Python312", "Python31", "Python32", "Python33", "Python34", "Python35", "Python36", "Python37", "Python38" };
        string str = "python";
        bool foundpy = false;
        Debug.Log("[GemChanger] We are going to find python preinstall");
        foreach (string py in pyhtonsver)
        {
            if (File.Exists(Environment.ExpandEnvironmentVariables($"%localappdata%/Programs/Python/{py}/python.exe")))
            {
                Debug.Log("[GemChanger] We auto found python " + $"%localappdata%/Programs/Python/{py}/python.exe");
                str = Environment.ExpandEnvironmentVariables($"%localappdata%/Programs/Python/{py}/python.exe");
                foundpy = true;
                break;
            }
        }
        if (!foundpy)
        {
            Debug.LogError("[GemChanger] We didn't auto find python using fail safe");
        }
        //checking if the file exisit
        if (File.Exists($"{Application.persistentDataPath}/External_Tools/FARtools-master/{batname}.bat"))
        {
            File.Delete($"{Application.persistentDataPath}/External_Tools/FARtools-master/{batname}.bat");
        }
        //ios
        string[] lines = null;
        if (farFile == "hudguitar_common.far")
        {
            Debug.Log("[Gemchanger] hudguitar_common trail");
            lines = new string[]
            {
                "@echo off",$"FARtools.py -rp --FAR" + $@" ""{farFile}"" " + "-p" + $@" ""art\hud\textures\{fileInFar}.img"" " +  "-f" + $@" ""{replaceFile}.img"" "
            };
        }
        else
        {
            if (fileInFar == "confighud3x2_primarydevicehigh")
            {
                Debug.Log("[Gemchanger] XML passer");
                lines = new string[]
                {
                    "@echo off",$"FARtools.py -rp --FAR" + $@" ""{farFile}"" " + "-p" + $@" ""configs\hud\guitar\hudtypes\{fileInFar}.xml"" " +  "-f" + $@" ""{replaceFile}.xml"" "
                };
            }
            else
            {
                lines = new string[]
                {
                    "@echo off",$"FARtools.py -rp --FAR" + $@" ""{farFile}"" " + "-p" + $@" ""art\mayaprojects\stagefright\scenes\model\highways\disk\primarydevicehigh\textures\{fileInFar}.img"" " +  "-f" + $@" ""{replaceFile}.img"" "
                };
            }
        }
        File.WriteAllLinesAsync($"{Application.persistentDataPath}/External_Tools/FARtools-master/{batname}.bat", lines);

        //checking if the file exisit
        if (File.Exists($"{Application.persistentDataPath}/External_Tools/FARtools-master/{batname}.bat"))
        {
            return $"{Application.persistentDataPath}/External_Tools/FARtools-master/{batname}.bat";
        }
        else
        {
            Debug.LogError("[GemChanger] Failed to create bat file: " + batname + ".bat");
            return null;
        }
    }

    public string BatchFileMaker(string inputFile, string outputFile, string batname, string format, int mipmap, int width, int height, string platform)
    {
        //check if input file exist
        if (!File.Exists(inputFile + ".png"))
        {
            Debug.Log("[GemChanger] Input file doesn't exist: " + inputFile);
            return null;
        }
        //finding python exe
        string[] pyhtonsver = { "Python310", "Python311", "Python39", "Python312", "Python31", "Python32", "Python33", "Python34", "Python35", "Python36", "Python37", "Python38" };
        string str = "python";
        bool foundpy = false;
        Debug.Log("[GemChanger] We are going to find python preinstall");
        foreach (string py in pyhtonsver)
        {
            if (File.Exists(Environment.ExpandEnvironmentVariables($"%localappdata%/Programs/Python/{py}/python.exe")))
            {
                Debug.Log("[GemChanger] We auto found python " + $"%localappdata%/Programs/Python/{py}/python.exe");
                str = Environment.ExpandEnvironmentVariables($"%localappdata%/Programs/Python/{py}/python.exe");
                foundpy = true;
                break;
            }
        }
        if (!foundpy)
        {
            Debug.LogError("[GemChanger] We didn't auto find python using fail safe");
        }
        //checking if the file exisit
        if (File.Exists($"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}_{platform}.bat"))
        {
            File.Delete($"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}_{platform}.bat");
        }

        string[] lines =
        {
            "@echo off", $"python ghl_img_converter.py  %1 convert"  + $@" ""{inputFile}.png"" " + $@"--output ""{outputFile}_{platform}.img"" --platform {platform} --width {width} --height {height} --format {format} --mipmap {mipmap}"
        };

        File.WriteAllLinesAsync($"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}_{platform}.bat", lines);

        //checking if the file exisit
        if (File.Exists($"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}_{platform}.bat"))
        {
            return $"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}_{platform}.bat";
        }
        else
        {
            Debug.LogError("[GemChanger] Failed to create bat file: " + $"{batname}_{platform}"+ ".bat");
            return null;
        }
    }

    public void ConvertToPackDetails()
    {
        userWantToConvert = true;
        StartCoroutine(done());
    }

    public void premakeZipFile(string packName, string creator)
    {
        //copy IMG to a folder that has this file sturcter
        // PackName
        // -/Details.txt //details about the pack, name and creator,modtool version
        // -/CoverImg.png //unity grab a screenshot and save that as an image and will be shown when exporting
        // - /Texutre/THE_TEXTURE.IMG
        //filetype packname.stickpack

        //setting Texture to the default pos
        noteType = NoteType.Normal;
        ModeToggleSwitch.isOn = false;
        gemBoxToggle.isOn = false;
        UpdateGem();

        if (!Directory.Exists($"{Application.persistentDataPath}/GemMaker/Exported Pack"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Exported Pack");
        }
        Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Packs/{packName}");
        Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/ps3");
        Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/ios");
        Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/wiiu");
        //dissable UI
        UI.SetActive(false);

        //settings the gems to default state

        ScreenShotHandler.TakeScreenshot_static($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/Cover", 1280, 720);
        UI.SetActive(true);
        load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_CREATE_PACK");
        load.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_CREATE_PACK_DES");

        //credit file
        string[] lines =
        {
            $"Packname:{packName}",$"Creator:{creator}",$"ModtoolVersion:{Application.version}"
        };
        File.WriteAllLinesAsync($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/Credit.dat", lines);

        Debug.Log("[GemChanger] finish creating Credit and cover image");
        StartCoroutine(CreateZip($"{Application.persistentDataPath}/GemMaker/Packs/{packName}", packName));
    }

    private IEnumerator CreateZip(string dir, string packName)
    {
        //ios
        try
        {
            File.Copy($"{Application.persistentDataPath}/External_Tools/FARtools-master/hwlprimary.far", $"{dir}/ios/hwlprimary.far", true);
            File.Copy($"{Application.persistentDataPath}/External_Tools/FARtools-master/hudguitar_common.far", $"{dir}/ios/hudguitar_common.far", true);
            File.Delete($"{Application.persistentDataPath}/External_Tools/FARtools-master/hwlprimary.far");
            File.Delete($"{Application.persistentDataPath}/External_Tools/FARtools-master/hudguitar_common.far");
        }
        catch(Exception e)
        {
            Debug.LogError("[GemChanger] Fail to copy ios far files");
        }


        string[] platform = { "ps3", "wiiu" };
        foreach (string plat in platform)
        {
            string RighGemImg = Application.persistentDataPath + $"/GemMaker/Texture/GemRight_{plat}";
            string LeftGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeft_{plat}";

            string LeftHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHeroPowerActive_{plat}";
            string RightHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHeroPowerActive_{plat}";

            string LeftHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHeroPowerCollect_{plat}";
            string RightHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHeroPowerCollect_{plat}";

            string LeftStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftStreak_{plat}";
            string RightStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightStreak_{plat}";

            string LeftHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemLeftHopo_{plat}";
            string RightHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemRightHopo_{plat}";

            string OpenGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGem_{plat}";
            string OpenGemHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemHeroPowerCollect_{plat}";
            string OpenGemHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemHeroPowerActive_{plat}";
            string OpenGemStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/OpenGemStreak_{plat}";
            string GemBoxIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemBox_{plat}";

            string trailIMG = Application.persistentDataPath + $"/GemMaker/Texture/Trail_{plat}";
            string trailHeroIMG = Application.persistentDataPath + $"/GemMaker/Texture/TrailHero_{plat}";

            yield return new WaitForEndOfFrame();
            if (File.Exists($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/Cover.png"))
            {
                File.Copy($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/Cover.png", $"{Application.persistentDataPath}/GemMaker/Exported Pack/{packName}_cover.png", true);
            }

            yield return new WaitForEndOfFrame();
            if (File.Exists($"{trailIMG}.img"))
            {
                File.Copy($"{trailIMG}.img", $"{dir}/{plat}/Trail.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{trailHeroIMG}.img"))
            {
                File.Copy($"{trailHeroIMG}.img", $"{dir}/{plat}/TrailHero.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{GemBoxIMG}.img"))
            {
                File.Copy($"{GemBoxIMG}.img", $"{dir}/{plat}/GemBox.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{RighGemImg}.img"))
            {
                File.Copy($"{RighGemImg}.img", $"{dir}/{plat}/GemRight.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{LeftGemIMG}.img"))
            {
                File.Copy($"{LeftGemIMG}.img", $"{dir}/{plat}/GemLeft.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{LeftHeroActiveIMG}.img"))
            {
                File.Copy($"{LeftHeroActiveIMG}.img", $"{dir}/{plat}/GemLeftHeroPowerActive.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{RightHeroActiveIMG}.img"))
            {
                File.Copy($"{RightHeroActiveIMG}.img", $"{dir}/{plat}/GemRightHeroPowerActive.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{LeftHeroCollectIMG}.img"))
            {
                File.Copy($"{LeftHeroCollectIMG}.img", $"{dir}/{plat}/GemLeftHeroPowerCollect.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{RightHeroCollectIMG}.img"))
            {
                File.Copy($"{RightHeroCollectIMG}.img", $"{dir}/{plat}/GemRightHeroPowerCollect.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{LeftStreakIMG}.img"))
            {
                File.Copy($"{LeftStreakIMG}.img", $"{dir}/{plat}/GemLeftStreak.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{RightStreakIMG}.img"))
            {
                File.Copy($"{RightStreakIMG}.img", $"{dir}/{plat}/GemRightStreak.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{LeftHopoIMG}.img"))
            {
                File.Copy($"{LeftHopoIMG}.img", $"{dir}/{plat}/GemLeftHopo.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{RightHopoIMG}.img"))
            {
                File.Copy($"{RightHopoIMG}.img", $"{dir}/{plat}/GemRightHopo.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{OpenGemIMG}.img"))
            {
                File.Copy($"{OpenGemIMG}.img", $"{dir}/{plat}/OpenGem.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{OpenGemHeroCollectIMG}.img"))
            {
                File.Copy($"{OpenGemHeroCollectIMG}.img", $"{dir}/{plat}/OpenGemHeroPowerCollect.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{OpenGemHeroActiveIMG}.img"))
            {
                File.Copy($"{OpenGemHeroActiveIMG}.img", $"{dir}/{plat}/OpenGemHeroPowerActive.img", true);
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{OpenGemStreakIMG}.img"))
            {
                File.Copy($"{OpenGemStreakIMG}.img", $"{dir}/{plat}/OpenGemStreak.img", true);
            }
        }
        //clean up
        Directory.GetFiles(Application.persistentDataPath + $"/GemMaker/Texture/", "*.*")
                 .Where(item => item.EndsWith(".png") || item.EndsWith(".img"))
                 .ToList()
                 .ForEach(item => File.Delete(item));

        string startPath = $@"{Application.persistentDataPath}\GemMaker\Packs\{packName}";
        string zipPath = $@"{Application.persistentDataPath}\GemMaker\Exported Pack\{packName}.stickpack";

        //checking if existing one exist
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        ZipFile.CreateFromDirectory(startPath, zipPath);
        Debug.Log("[GemChanger] Finish creating gem pack");

        //deleting temp file
        Directory.Delete($"{Application.persistentDataPath}/GemMaker/Packs", true);
        load.GetComponent<GUI_MessageBox>().CloseAnim();

        GameObject a = Instantiate(messageBox);
        a.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_CREATED_PACK");
        a.GetComponent<GUI_MessageBox>().message = $"{T.getText("GEM_CREATOR_CREATED_PACK_DES1")} {packName} {T.getText("GEM_CREATOR_CREATED_PACK_DES2")}";
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(opengempackdir);
    }

    public void opengempackdir()
    {
        Application.OpenURL($@"{Application.persistentDataPath}\GemMaker\Exported Pack\");
        GameObject a = Instantiate(messageBox2);
        a.GetComponent<GUI_MessageBox>().title = T.getText("GEM_CREATOR_SHARE");
        a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_CREATOR_SHARE_DES");
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(OpenGemPackChannel);
    }

    public void OpenGemPackChannel()
    {
        Application.OpenURL("https://discord.com/channels/758530768640802826/1061536695624925185");
        // this link shouldn't be hard coded in, should be dynamic
    }

    public void pythonNotices()
    {
        StartGemChanger();
    }
}