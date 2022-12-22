using Ini;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class GemChanger : MonoBehaviour
{
    private WebClient webClient = null;
    public GameObject UI;
    public GameObject messageBox;
    public GameObject loadingBox;
    public GameObject messageBoxWithText;
    public GameData GemcreatorMenu;
    public GameObject fadeToBlack;
    private GameObject load;
    public Button openstrumbutton;
    private GameObject Textbox;

    [Header("UI")]
    public Toggle ModeToggleSwitch;

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

    public bool isDownloading = false;
    public List<string> Highways;
    private bool DoneConverting = false;
    private bool userWantToConvert = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //clean up temp files
            if (File.Exists(Application.persistentDataPath + "\\GemMaker\\Texture\\GemRight.png"))
            {
                File.Delete(Application.persistentDataPath + "\\GemMaker\\Texture\\GemRight.png");
            }
            if (File.Exists(Application.persistentDataPath + "\\GemMaker\\Texture\\GemLeft.png"))
            {
                File.Delete(Application.persistentDataPath + "\\GemMaker\\Texture\\GemLeft.png");
            }
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
            Debug.Log("Error. Platform isn't supported");
            Debug.Log(Application.platform);
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = "ERROR";
            a.GetComponent<GUI_MessageBox>().message = $"Gem Changer - Create isn't supported for your current OS\n\nGem Changer - Create is currently support Windows";
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
            Debug.Log("Error. Check internet connection!");
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = "No Active Internet Connection";
            a.GetComponent<GUI_MessageBox>().message = $"No internet connect exist\n\nMake sure you are connected to the internet and try again.";
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
            return;
        }
        else
        {
            Debug.Log("We have internet");

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
        }
    }

    private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required
        Debug.Log("Download Recevedbyte: " + e.BytesReceived);
        Debug.Log("Download totalbyte: " + e.TotalBytesToReceive);
        // use these variables if needed
        load.GetComponent<GUI_MessageBox>().message = $"Please wait while we download external files\n\n Dowloaded: {e.ProgressPercentage}%";
        load.GetComponent<GUI_MessageBox>().addmessage();
    }

    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        isDownloading = false;
        Debug.Log("Download completed!");

        extractFile("GHLIMGConverter.zip");
    }

    private void CompletedPVR(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        isDownloading = false;
        Debug.Log("Download completed!");
    }

    private void extractFile(string filename)
    {
        if (!Directory.Exists(Application.persistentDataPath + "\\External_Tools"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "\\External_Tools");
        }
        ZipFile.ExtractToDirectory(Application.persistentDataPath + $"\\{filename}", Application.persistentDataPath + "\\External_Tools");
        File.Delete(Application.persistentDataPath + $"\\{filename}");
        Step2();
    }

    private string PVRTexToolInstall()
    {
        var ini = new IniFile(Application.persistentDataPath + "\\External_Tools\\GHLIMGConverter-master\\config.ini");
        string a = ini.IniReadValue("path", "PVRTexToolCLI");
        if (a.Contains("PVRTexToolCLI.exe"))
        {
            Debug.Log("User has PVRTextToolCLIInstalled");
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
        a.GetComponent<GUI_MessageBox>().title = "External Tool Require - PART 1";
        a.GetComponent<GUI_MessageBox>().message = $"We need you to install an external tool to allow us to save your custom gems to Guitar Hero Live.\n\nClick Continue Download once downloaded install the program\n\nIf this is already install you can skip this step";
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
        load.GetComponent<GUI_MessageBox>().title = "Downloading";
        load.GetComponent<GUI_MessageBox>().message = "Please wait while we download external files";
        yield return new WaitUntil(() => isDownloading == false);
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        yield return new WaitForSeconds(1f);
        if (type == 1)
        {
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = "External Tool Require - PART 2";
            a.GetComponent<GUI_MessageBox>().message = $"Once the install has complete\n\nClick Continue to locate the PVRTextToolCLI.exe\n\nIf you have installed it to the default path we will auto located.";
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
            b.GetComponent<GUI_MessageBox>().title = "External Tool successfully located";
            b.GetComponent<GUI_MessageBox>().message = $"We manage to auto located the external tool on your device!!!";
            b.GetComponent<GUI_MessageBox>().button.onClick.AddListener(main);
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
                    c.GetComponent<GUI_MessageBox>().title = "External Tool successfully located";
                    c.GetComponent<GUI_MessageBox>().message = $"We manage to located the external tool!!!";
                    c.GetComponent<GUI_MessageBox>().button.onClick.AddListener(main);
                }
                else
                {
                    GameObject a = Instantiate(messageBox);
                    a.GetComponent<GUI_MessageBox>().title = "External Tool Require";
                    a.GetComponent<GUI_MessageBox>().message = $"We need you to install an external tool to allow us to save your custom gems to Guitar Hero Live.\n\nClick Continue to open the download page\n\nMAKE SURE YOUR INSTALL PVR V4.0";
                    a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(OpenFileExplore);
                }
            }
        }
    }

    public void main()
    {
        black.GetComponent<Animation>().Play("FadeFromBlack");
    }

    public void ChangeLeftHandedImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select your Left handed gem texture", "", "png", false);
        foreach (var path in paths)
        {
            var loader = new WWW(path);
            if (!CheckUploadedSize(loader))
            {
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = "Invalided Image";
                a.GetComponent<GUI_MessageBox>().message = $"Make Sure your uploaded image is 512X512 \n\nPlease try again";
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
        var paths = StandaloneFileBrowser.OpenFilePanel("Select your open gem texture", "", "png", false);
        foreach (var path in paths)
        {
            var loader = new WWW(path);
            if (!CheckUploadedSize(loader))
            {
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = "Invalided Image";
                a.GetComponent<GUI_MessageBox>().message = $"Make Sure your uploaded image is 256X256 \n\nPlease try again";
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

    public void ChangeRightHandedImage()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select your Right hadned gem texture", "", "png", false);
        foreach (var path in paths)
        {
            var loader = new WWW(path);
            if (!CheckUploadedSize(loader))
            {
                GameObject a = Instantiate(messageBox);
                a.GetComponent<GUI_MessageBox>().title = "Invalided Image";
                a.GetComponent<GUI_MessageBox>().message = $"Make Sure your uploaded image is 256X256 \n\nPlease try again";
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
        if (noteType == NoteType.Normal)
        {
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

    public void ResetGem()
    {
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
        //clean up
        if (File.Exists($"{RighGemImg}.png"))
        {
            File.Delete(RighGemImg + ".png");

        }
        if (File.Exists($"{LeftGemIMG}.png"))
        {
            File.Delete(LeftGemIMG + ".png");
        }
        if (File.Exists($"{LeftHeroActiveIMG}.png"))
        {
            File.Delete(LeftHeroActiveIMG + ".png");
        }
        if (File.Exists($"{RightHeroActiveIMG}.png"))
        {
            File.Delete(RightHeroActiveIMG + ".png");
        }
        if (File.Exists($"{LeftHeroCollectIMG}.png"))
        {
            File.Delete(LeftHeroCollectIMG + ".png");
        }
        if (File.Exists($"{RightHeroCollectIMG}.png"))
        {
            File.Delete(RightHeroCollectIMG + ".png");
        }
        if (File.Exists($"{LeftStreakIMG}.png"))
        {
            File.Delete(LeftStreakIMG + ".png");
        }
        if (File.Exists($"{RightStreakIMG}.png"))
        {
            File.Delete(RightStreakIMG + ".png");
        }
        if (File.Exists($"{LeftHopoIMG}.png"))
        {
            File.Delete(LeftHopoIMG + ".png");
        }
        if (File.Exists($"{RightHopoIMG}.png"))
        {
            File.Delete(RightHopoIMG + ".png");
        }
        if (File.Exists($"{OpenGemIMG}.png"))
        {
            File.Delete(OpenGemIMG + ".png");
        }
        if (File.Exists($"{OpenGemHeroCollectIMG}.png"))
        {
            File.Delete(OpenGemHeroCollectIMG + ".png");
        }
        if (File.Exists($"{OpenGemHeroActiveIMG}.png"))
        {
            File.Delete(OpenGemHeroActiveIMG + ".png");
        }
        if (File.Exists($"{OpenGemStreakIMG}.png"))
        {
            File.Delete(OpenGemStreakIMG + ".png");
        }

        resetGemToDefault();
        ModeToggleSwitch.isOn = false;
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
                    Debug.Log($"{path} Is valid");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"{userData.instance.LocalFilePath} Is valid");
                    done();
                }
                else
                {
                    GameObject t = Instantiate(messageBox);
                    userData.instance.LocalFilePath = "TEST";
                    t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
                    t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
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
            t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
            t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
        }
    }

    public IEnumerator done()
    {
        load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = "Converting";
        load.GetComponent<GUI_MessageBox>().message = "Please wait while convert your textures.";

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

        if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/PRIMARY/TEXTURES/"))
        {
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/PRIMARY/TEXTURES/");
        }
        if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/PRIMARY/TEXTURES/"))
        {
            Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/PRIMARY/TEXTURES/");
        }
        List<string> gembatchfiles = new List<string>();
        gembatchfiles.Add(BatchFileMaker(RighGemImg, RighGemImg, "GemRight", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(LeftGemIMG, LeftGemIMG, "GemLeft", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(LeftHeroActiveIMG, LeftHeroActiveIMG, "GemLeftHeroPowerActive", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(RightHeroActiveIMG, RightHeroActiveIMG, "GemRightHeroPowerActive", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(LeftHeroCollectIMG, LeftHeroCollectIMG, "GemLeftHeroPowerCollect", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(RightHeroCollectIMG, RightHeroCollectIMG, "GemRightHeroPowerCollect", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(LeftStreakIMG, LeftStreakIMG, "GemLeftStreak", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(RightStreakIMG, RightStreakIMG, "GemRightStreak", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(LeftHopoIMG, LeftHopoIMG, "GemLeftHopo", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(RightHopoIMG, RightHopoIMG, "GemRightHopo", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(OpenGemIMG, OpenGemIMG, "OpenGem", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(OpenGemHeroCollectIMG, OpenGemHeroCollectIMG, "OpenGemHeroPowerCollect", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(OpenGemHeroActiveIMG, OpenGemHeroActiveIMG, "OpenGemHeroPowerActive", 6, 256, 256));
        yield return new WaitForEndOfFrame();
        gembatchfiles.Add(BatchFileMaker(OpenGemStreakIMG, OpenGemStreakIMG, "OpenGemStreak", 6, 256, 256));
        yield return new WaitForEndOfFrame();
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
                    Debug.Log("[GemChanger]Running batfile: " + file);
                }
                else
                {
                    filesFailed++;
                    Debug.Log("[GemChanger]Batfile faild to located");
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                filesFailed++;
                Debug.Log("[GemChanger]Batfile faild create gem IMG");
            }
        }
        if(filesFailed >= filesnum)
        {
            GameObject a = Instantiate(messageBox);
            a.GetComponent<GUI_MessageBox>().title = "Convert Failed";
            a.GetComponent<GUI_MessageBox>().message = $"Failed to Convert your textures\n\nCheck if you have Python 3.1 installed";
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
            yield return null;
        }
        if (!userWantToConvert)
        {
            load.GetComponent<GUI_MessageBox>().CloseAnim();
            yield return new WaitForSeconds(1);
            StartCoroutine(AddToGHL());
        }
        else if(userWantToConvert)
        {
            load.GetComponent<GUI_MessageBox>().CloseAnim();
            yield return new WaitForSeconds(1);
            Textbox = Instantiate(messageBoxWithText);
            Textbox.GetComponent<GUI_MessageBox>().title = "Create Gem Pack";
            Textbox.GetComponent<GUI_MessageBox>().message = $"Before we can create your pack, We need some details";
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
        Debug.Log("bat file done");
    }

    private IEnumerator AddToGHL()
    {
        load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = "Exporting";
        load.GetComponent<GUI_MessageBox>().message = "Please wait while Export your textures.";
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
        //loop though the list of loc so it apply to GHTV and GHL gems

        List<string> GameVersion = new List<string>();
        GameVersion.Add("BLES02180");
        GameVersion.Add("BLUS31556");
        //all highways of GHL
        int num = 0;

        //removing the old gems if placed
        Debug.Log("[Gem Changer] Removing old gems");
        foreach (string version in GameVersion)
        {
            if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS"))
            {
                Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS", true);
            }
        }

        foreach (string highway in Highways)
        {
            yield return new WaitForEndOfFrame();
            load.GetComponent<GUI_MessageBox>().message = $"Please wait while Export your textures to Guitar Hero Live\n\nConverted: {num}/{Highways.Count}";
            load.GetComponent<GUI_MessageBox>().addmessage();
            num++;
            foreach (string version in GameVersion)
            {
                //CHECKING IF Folder EXIST
                if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/"))
                {
                    Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/");
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.IMG"))
                {
                    File.Copy($"{RighGemImg}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEM_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftGemIMG}.IMG"))
                {
                    File.Copy($"{LeftGemIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{LeftHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHEROACTIVELEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{RightHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{LeftHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHEROCOLLECTLEFT_COL_SRGB_00.1001.IMG", true);
                }
                if (File.Exists($"{RightHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{RightHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftStreakIMG}.IMG"))
                {
                    File.Copy($"{LeftStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMSTREAKLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightStreakIMG}.IMG"))
                {
                    File.Copy($"{RightStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHopoIMG}.IMG"))
                {
                    File.Copy($"{LeftHopoIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHOPOLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHopoIMG}.IMG"))
                {
                    File.Copy($"{RightHopoIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEMHOPO_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemIMG}.IMG"))
                {
                    File.Copy($"{OpenGemIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_STRUM_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{OpenGemHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_STRUMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{OpenGemHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_STRUMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemStreakIMG}.IMG"))
                {
                    File.Copy($"{OpenGemStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_STRUMSTREAK_COL_SRGB_00.1001.IMG", true);
                }
            }


        }

        yield return new WaitForEndOfFrame();
        //clean up
        if (File.Exists($"{RighGemImg}.IMG"))
        {
            File.Delete(RighGemImg + ".png");
            File.Delete(RighGemImg + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftGemIMG}.IMG"))
        {
            File.Delete(LeftGemIMG + ".png");
            File.Delete(LeftGemIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHeroActiveIMG}.IMG"))
        {
            File.Delete(LeftHeroActiveIMG + ".png");
            File.Delete(LeftHeroActiveIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHeroActiveIMG}.IMG"))
        {
            File.Delete(RightHeroActiveIMG + ".png");
            File.Delete(RightHeroActiveIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHeroCollectIMG}.IMG"))
        {
            File.Delete(LeftHeroCollectIMG + ".png");
            File.Delete(LeftHeroCollectIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHeroCollectIMG}.IMG"))
        {
            File.Delete(RightHeroCollectIMG + ".png");
            File.Delete(RightHeroCollectIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftStreakIMG}.IMG"))
        {
            File.Delete(LeftStreakIMG + ".png");
            File.Delete(LeftStreakIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightStreakIMG}.IMG"))
        {
            
            File.Delete(RightStreakIMG + ".png");
            File.Delete(RightStreakIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHopoIMG}.IMG"))
        {
            File.Delete(LeftHopoIMG + ".png");
            File.Delete(LeftHopoIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHopoIMG}.IMG"))
        {
            File.Delete(RightHopoIMG + ".png");
            File.Delete(RightHopoIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemIMG}.IMG"))
        {
            File.Delete(OpenGemIMG + ".png");
            File.Delete(OpenGemIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemHeroCollectIMG}.IMG"))
        {
            File.Delete(OpenGemHeroCollectIMG + ".png");
            File.Delete(OpenGemHeroCollectIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemHeroActiveIMG}.IMG"))
        {
            File.Delete(OpenGemHeroActiveIMG + ".png");
            File.Delete(OpenGemHeroActiveIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemStreakIMG}.IMG"))
        {
            File.Delete(OpenGemStreakIMG + ".png");
            File.Delete(OpenGemStreakIMG + ".IMG");
        }


        load.GetComponent<GUI_MessageBox>().CloseAnim();
        yield return new WaitForSeconds(1);
        //finish message
        GameObject a = Instantiate(messageBox);
        a.GetComponent<GUI_MessageBox>().title = "Exported Complete";
        a.GetComponent<GUI_MessageBox>().message = $"Custom Gems have been exported to Guitar Hero live.\n\nThe Next time you load the default highway or play GHL song the changes will be apply.";
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        Debug.LogWarning("[Gem Changer]Finish exporting texture");
    }

    public string BatchFileMaker(string inputFile, string outputFile, string batname, int mipmap, int width, int height)
    {
        //check if input file exist
        if (!File.Exists(inputFile + ".png"))
        {
            Debug.Log("[Gem Changer]Input file doesn't exist: " + inputFile);
            return null;
        }
        //checking if the file exisit
        if (File.Exists($"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}.bat"))
        {
            File.Delete($"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}.bat");
        }
        string[] lines =
        {
            "@echo off", $"python ghl_img_converter.py  %1 convert"  + $@" ""{inputFile}.png"" " + $@"--output ""{outputFile}.IMG"" --platform ps3 --width {width} --height {height} --format BC1 --mipmap {mipmap}"
        };
        File.WriteAllLinesAsync($"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}.bat", lines);

        //checking if the file exisit
        if (File.Exists($"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}.bat"))
        {
            return $"{Application.persistentDataPath}/External_Tools/GHLIMGConverter-master/{batname}.bat";
        }
        else
        {
            Debug.Log("[Gem Changer]Failed to create bat file: " + batname + ".bat");
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

        //setting Texture to the default one

        noteType = NoteType.Normal;
        UpdateGem();
        if(!Directory.Exists($"{Application.persistentDataPath}/GemMaker/Exported Pack"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Exported Pack");
        }
        Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Packs/{packName}");
        //dissable UI
        UI.SetActive(false);
        ScreenShotHandler.TakeScreenshot_static($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/Cover", 1280, 720);
        UI.SetActive(true);
        load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = "Creating Gem Pack";
        load.GetComponent<GUI_MessageBox>().message = "Please wait while we create your gem pack.";

       

        //credit file
        string[] lines =
        {
            $"Packname:{packName}",$"Creator:{creator}",$"ModtoolVersion:{Application.version}"
        };
        File.WriteAllLinesAsync($"{Application.persistentDataPath}/GemMaker/Packs/{packName}/Credit.dat", lines);

        Debug.Log("[GemChanger]finish creating Credit and cover image");
        StartCoroutine(CreateZip($"{Application.persistentDataPath}/GemMaker/Packs/{packName}", packName));
    }

    private IEnumerator CreateZip(string dir, string packName)
    {

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


        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RighGemImg}.IMG"))
        {
            File.Copy($"{RighGemImg}.IMG", $"{dir}/GemRight.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftGemIMG}.IMG"))
        {
            File.Copy($"{LeftGemIMG}.IMG", $"{dir}/GemLeft.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHeroActiveIMG}.IMG"))
        {
            File.Copy($"{LeftHeroActiveIMG}.IMG", $"{dir}/GemLeftHeroPowerActive.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHeroActiveIMG}.IMG"))
        {
            File.Copy($"{RightHeroActiveIMG}.IMG", $"{dir}/GemRightHeroPowerActive.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHeroCollectIMG}.IMG"))
        {
            File.Copy($"{LeftHeroCollectIMG}.IMG", $"{dir}/GemLeftHeroPowerCollect.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHeroCollectIMG}.IMG"))
        {
            File.Copy($"{RightHeroCollectIMG}.IMG", $"{dir}/GemRightHeroPowerCollect.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftStreakIMG}.IMG"))
        {
            File.Copy($"{LeftStreakIMG}.IMG", $"{dir}/GemLeftStreak.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightStreakIMG}.IMG"))
        {
            File.Copy($"{RightStreakIMG}.IMG", $"{dir}/GemRightStreak.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHopoIMG}.IMG"))
        {
            File.Copy($"{LeftHopoIMG}.IMG", $"{dir}/GemLeftHopo.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHopoIMG}.IMG"))
        {
            File.Copy($"{RightHopoIMG}.IMG", $"{dir}/GemRightHopo.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemIMG}.IMG"))
        {
            File.Copy($"{OpenGemIMG}.IMG", $"{dir}/OpenGem.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemHeroCollectIMG}.IMG"))
        {
            File.Copy($"{OpenGemHeroCollectIMG}.IMG", $"{dir}/OpenGemHeroPowerCollect.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemHeroActiveIMG}.IMG"))
        {
            File.Copy($"{OpenGemHeroActiveIMG}.IMG", $"{dir}/OpenGemHeroPowerActive.IMG", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemStreakIMG}.IMG"))
        {
            File.Copy($"{OpenGemStreakIMG}.IMG", $"{dir}/OpenGemStreak.IMG", true);
        }



        //clean up
        if (File.Exists($"{RighGemImg}.IMG"))
        {
            File.Delete(RighGemImg + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftGemIMG}.IMG"))
        {
            File.Delete(LeftGemIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHeroActiveIMG}.IMG"))
        {
            File.Delete(LeftHeroActiveIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHeroActiveIMG}.IMG"))
        {
            File.Delete(RightHeroActiveIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHeroCollectIMG}.IMG"))
        {
            File.Delete(LeftHeroCollectIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHeroCollectIMG}.IMG"))
        {
            File.Delete(RightHeroCollectIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftStreakIMG}.IMG"))
        {
            File.Delete(LeftStreakIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightStreakIMG}.IMG"))
        {
            File.Delete(RightStreakIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{LeftHopoIMG}.IMG"))
        {
            File.Delete(LeftHopoIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{RightHopoIMG}.IMG"))
        {
            File.Delete(RightHopoIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemIMG}.IMG"))
        {
            File.Delete(OpenGemIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemHeroCollectIMG}.IMG"))
        {
            File.Delete(OpenGemHeroCollectIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemHeroActiveIMG}.IMG"))
        {
            File.Delete(OpenGemHeroActiveIMG + ".IMG");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{OpenGemStreakIMG}.IMG"))
        {
            File.Delete(OpenGemStreakIMG + ".IMG");
        }




        string startPath = $@"{Application.persistentDataPath}\GemMaker\Packs\{packName}";
        string zipPath = $@"{Application.persistentDataPath}\GemMaker\Exported Pack\{packName}.stickpack";
        
        //checking if existing one exist
        if(File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        ZipFile.CreateFromDirectory(startPath, zipPath);
        Debug.Log("[GemChanger]Finish creating gem pack");

        //deleting temp file
        Directory.Delete($"{Application.persistentDataPath}/GemMaker/Packs", true);
        load.GetComponent<GUI_MessageBox>().CloseAnim();

        GameObject a = Instantiate(messageBox);
        a.GetComponent<GUI_MessageBox>().title = "Gem Pack Created";
        a.GetComponent<GUI_MessageBox>().message = $"Your Gem pack {packName} has been created";
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(opengempackdir);
    }

    public void opengempackdir()
    {
        Application.OpenURL($@"{Application.persistentDataPath}\GemMaker\Exported Pack\");
    }
    public void pythonNotices()
    {
        StartGemChanger();
    }
}