using Renci.SshNet;
using Renci.SshNet.Common;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GemChanger_Importer : MonoBehaviour
{
    public GemChanger main;
    public string[] resourcePacks;
    public GameObject PackPrefab;
    public GameObject MessageBox;
    public GameObject MessageBox2;
    private GameObject load;
    public GameObject LoadingBox;
    public GameObject fadeToBlack;
    public GameObject blocker;
    public List<GameObject> PackObj = new List<GameObject>();

    public GameObject ScrollViewContent;
    private WebClient webClient = null;
    private bool isDownloading;

    private GameObject nopackmessage;
    public bool refreshing = false;
    private Translater T;

    public GameObject wiiuconfig;
    private GameObject wiiUConfigOBJ;

    [Header("Gem Pack Importer Config")]
    public float refreshCooldown = 1.2f;

    private bool cooldowncheck = false;
    private string localAppdata;

    // Start is called before the first frame update
    private void Start()
    {
        localAppdata = Application.persistentDataPath;
    }

    public void GemChangerImporterStart()
    {
        T = Translater.instance;
        DiscordController.instance.UpdateDiscordActivity("Gem Changer", "Importing a Gem Pack", "gem_changer", "Gem Changer");
        if (!Directory.Exists($"{Application.persistentDataPath}/GemMaker/Resource Pack"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Resource Pack");
        }
        if (!userData.instance.hasDownloadedDefaultAtLeastOnce)
        {
            GameObject a = Instantiate(MessageBox2);
            GUI_MessageBox b = a.GetComponent<GUI_MessageBox>();
            b.title = T.getText("GEM_IMPORTER_DOWNLOAD_PACK");
            b.message = T.getText("GEM_IMPORTER_DOWNLOAD_PACK_DES");
            b.button.onClick.AddListener(DownloadPacks);
        }
        else
        {
            refreshUI();
        }
    }

    public void DownloadPacks()
    {
        Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/defaultpack/");
        userData.instance.hasDownloadedDefaultAtLeastOnce = true;
        btnDownload_Click("https://github.com/Nathan31973/GHTV-Reloaded-Mods-Tools-Assets/raw/main/default.stickpack", "OfficialGemPack.stickpack");
    }

    public void MANUALDOWNLOADPACK()
    {
        GameObject a = Instantiate(MessageBox2);
        GUI_MessageBox b = a.GetComponent<GUI_MessageBox>();
        b.title = T.getText("GEM_IMPORTER_DOWNLOAD_PACK");
        b.message = T.getText("GEM_IMPORTER_DOWNLOAD_PACK_DES");
        b.button.onClick.AddListener(DownloadPacks);
    }

    private void btnDownload_Click(string url, string filename)
    {
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[GemChangerImporter] Error. Check internet connection!");
            GameObject a = Instantiate(MessageBox);
            a.GetComponent<GUI_MessageBox>().title = "No Active Internet Connection";
            a.GetComponent<GUI_MessageBox>().message = $"No internet connect exist\n\nMake sure you are connected to the internet and try again.";
            return;
        }
        else
        {
            Debug.Log("[GemChangerImporter] We have internet");
            load = Instantiate(LoadingBox);
            load.GetComponent<GUI_MessageBox>().title = "Downloading";
            load.GetComponent<GUI_MessageBox>().message = "Please wait while we download Official GHTV Reloaded Gem Packs";
            // Is file downloading yet?
            if (webClient != null)
            {
                return;
            }
            if (!Directory.Exists(Application.persistentDataPath + $"/GemMaker/defaultpack"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + $"/GemMaker/defaultpack");
            }

            //checking if we don't have an old one
            if (File.Exists(Application.persistentDataPath + $"/GemMaker/defaultpack/OfficialGemPack.stickpack"))
            {
                File.Delete(Application.persistentDataPath + $"/GemMaker/defaultpack/OfficialGemPack.stickpack");
            }

            isDownloading = true;
            webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            webClient.QueryString.Add("file", Application.persistentDataPath + $"/GemMaker/defaultpack/{filename}"); // To identify the file
            webClient.DownloadFileAsync(new Uri($"{url}"), Application.persistentDataPath + $"/GemMaker/defaultpack/{filename}");
        }
    }

    private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required
        Debug.Log("[GemChangerImporter] Download Recevedbyte: " + e.BytesReceived + " Download totalbyte: " + e.TotalBytesToReceive);
        // use these variables if needed
        load.GetComponent<GUI_MessageBox>().message = $"{T.getText("GEM_IMPORTER_DOWNLOADING")}\n\n{T.getText("STR_DOWNLOADED")} {e.ProgressPercentage}%";
        load.GetComponent<GUI_MessageBox>().addmessage();
    }

    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        isDownloading = false;
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        Debug.Log("[GemChangerImporter] Download completed!");
        StartCoroutine(extractFile(Application.persistentDataPath + $"/GemMaker/defaultpack/OfficialGemPack.stickpack"));
    }

    private IEnumerator extractFile(string path)
    {
        load = Instantiate(LoadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("STR_IMPORTING");
        load.GetComponent<GUI_MessageBox>().message = T.getText("GEM_IMPORTER_IMPORTING_DES");
        using (ZipArchive archive = ZipFile.OpenRead(path))
        {
            foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(".stickpack")))
            {
                yield return new WaitForEndOfFrame();
                if (!Directory.Exists($"{Application.persistentDataPath}/GemMaker/Resource Pack/"))
                {
                    Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Resource Pack/");
                }
                entry.ExtractToFile(Path.Combine($"{Application.persistentDataPath}/GemMaker/Resource Pack/", entry.FullName), true);
                Debug.Log($"[GemChangerImporter] Extracting {Path.Combine($"{Application.persistentDataPath}/GemMaker/Resource Pack/", entry.FullName)}");
            }
        }
        Directory.Delete(Application.persistentDataPath + $"/GemMaker/defaultpack", true);
        load.GetComponent<GUI_MessageBox>().CloseAnim();
    }

    private IEnumerator cooldownRefreshcheck()
    {
        cooldowncheck = true;
        yield return new WaitForSeconds(refreshCooldown);
        cooldowncheck = false;
    }

    private void refreshUI()
    {
        if (!refreshing && !cooldowncheck)
        {
            try
            {
                resourcePacks = Directory.GetFiles($"{Application.persistentDataPath}/GemMaker/Resource Pack", "*.stickpack");
                if (resourcePacks.Length > 0)
                {
                    StartCoroutine(GrabingKeyFilesFromPack());
                }
                else
                {
                    if (nopackmessage != null)
                    {
                        Debug.Log("[GemChangerImporter] No Gem Pack found");
                        nopackmessage = Instantiate(MessageBox);
                        GUI_MessageBox b = nopackmessage.GetComponent<GUI_MessageBox>();
                        b.title = T.getText("GEM_IMPORTER_NOPACK");
                        b.message = T.getText("GEM_IMPORTER_NOPACK_DES");
                        b.button.onClick.AddListener(OpenResourcePackFolder);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Could not find a part of the path"))
                {
                    Debug.Log("[GemChangerImporter] Can't find Path");
                }
            }
        }
    }

    public void OpenResourcePackFolder()
    {
        Application.OpenURL($"{Application.persistentDataPath}/GemMaker/Resource Pack");
    }

    private void OnApplicationFocus(bool focus)
    {
        refreshUI();
    }

    private IEnumerator GrabingKeyFilesFromPack()
    {
        if (!refreshing)
        {
            refreshing = true;
            blocker.SetActive(true);
            //nuke the cache folder
            if (Directory.Exists($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache"))
            {
                Debug.LogWarning("[GemChangerImporter] Clearing Resource Pack Cache");
                Directory.Delete($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache", true);
            }
            yield return new WaitForEndOfFrame();
            //clearing out existing UI
            if (PackObj != null)
            {
                foreach (GameObject g in PackObj.ToArray())
                {
                    yield return new WaitForEndOfFrame();
                    GameObject a = g;
                    PackObj.Remove(g);
                    Destroy(a);
                }
                PackObj.Clear();
            }

            string creator = "";
            string version = "";
            foreach (string pack in resourcePacks)
            {
                yield return new WaitForEndOfFrame();
                using (ZipArchive archive = ZipFile.OpenRead(pack))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains("Cover.png")))
                    {
                        if (!Directory.Exists($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}"))
                        {
                            Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}");
                        }
                        entry.ExtractToFile(Path.Combine($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}", entry.FullName));
                    }
                    foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains("Credit.dat")))
                    {
                        if (!Directory.Exists($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}"))
                        {
                            Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}");
                        }
                        entry.ExtractToFile(Path.Combine($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}", entry.FullName));
                        string getcreator = GetLine($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}/Credit.dat", 2);
                        string[] tempsplit = getcreator.Split(":");
                        creator = tempsplit[1];

                        string getver = GetLine($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}/Credit.dat", 3);
                        string[] tempsplit2 = getver.Split(":");
                        version = tempsplit2[1];
                    }

                    GameObject a = Instantiate(PackPrefab);
                    a.transform.SetParent(ScrollViewContent.transform, false);
                    a.GetComponent<ExportPackCoverGameObject>().host = this;
                    a.GetComponent<ExportPackCoverGameObject>().packName = Path.GetFileNameWithoutExtension(pack);
                    a.GetComponent<ExportPackCoverGameObject>().Creator = creator;
                    a.GetComponent<ExportPackCoverGameObject>().version = version;
                    a.GetComponent<ExportPackCoverGameObject>().PackIcon = GetImg($"{Application.persistentDataPath}/GemMaker/Resource Pack/cache/{Path.GetFileNameWithoutExtension(pack)}/Cover");
                    PackObj.Add(a);
                }
            }
            blocker.SetActive(false);
            refreshing = false;
            StartCoroutine(cooldownRefreshcheck());
        }
    }

    private string GetLine(string fileName, int line)
    {
        using (var sr = new StreamReader(fileName))
        {
            for (int i = 1; i < line; i++)
                sr.ReadLine();
            return sr.ReadLine();
        }
    }

    private Texture2D GetImg(string path)
    {
        Texture2D thisTexture;
        byte[] bytes;
        thisTexture = new Texture2D(240, 135);
        bytes = File.ReadAllBytes(path + ".png");
        thisTexture.LoadImage(bytes);
        thisTexture.name = "Cover";
        return thisTexture;
    }

    public void ExportPack(string packname)
    {
        //rpcs3
        if (userData.instance.platform == userData.Platform.Rpcs3)
        {
            ExportRPCS3(packname);
        }
        else if (userData.instance.platform == userData.Platform.Ios || userData.instance.platform == userData.Platform.TVOS)
        {
            //IOS export
            CheckSSHDetails(packname);
        }
        else if (userData.instance.platform == userData.Platform.WiiU)
        {
            //Wii U export
            ExportWiiU(packname);
        }
        else if (userData.instance.platform == userData.Platform.PS3)
        {
            //Real PS3
        }
        else
        {
            GameObject t = Instantiate(MessageBox);
            userData.instance.LocalFilePath = "TEST";
            t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
            t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
        }
    }

    private void CheckSSHDetails(string packname)
    {
        if (userData.instance.SSHiosIP != null && userData.instance.SSHiosPassword != null)
        {
            Thread myTest = new System.Threading.Thread(delegate ()
            {
                Debug.LogWarning("[Gem Importer] - Threading SSH");
                //attemp ssh
                var connectionInfo = new ConnectionInfo($"{userData.instance.SSHiosIP}", "root", new PasswordAuthenticationMethod("root", userData.instance.SSHiosPassword));
                try
                {
                    using (var client = new SftpClient(connectionInfo))
                    {
                        client.Connect();
                        bool test = false;
                        //check if client save location exist
                        if (userData.instance.gameIosUUID != null)
                        {
                            try
                            {
                                client.ChangeDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app");
                                Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} is GHL install");
                                iosthingy($"{localAppdata}/GemMaker/Resource Pack/{packname}.stickpack");
                                test = true;
                            }
                            catch (SftpPathNotFoundException)
                            {
                                Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} isn't GHL");
                                userData.instance.gameIosUUID = "Test";
                            }
                        }
                        if (!test)
                        {
                            var files = client.ListDirectory("/private/var/containers/Bundle/Application");
                            foreach (var file in files)
                            {
                                try
                                {
                                    client.ChangeDirectory($"/private/var/containers/Bundle/Application/{file.Name}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app");
                                    userData.instance.gameIosUUID = file.Name;
                                    Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {file.Name} is GHL install");
                                    iosthingy($"{localAppdata}/GemMaker/Resource Pack/{packname}.stickpack");
                                }
                                catch (SftpPathNotFoundException)
                                {
                                    Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {file.Name} isn't GHL");
                                }
                            }
                        }
                        client.Disconnect();
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "Permission denied (password).")
                    {
                        Debug.LogError("[Gem Importer] - Bad Password");
                        //message saying invaid password
                    }
                    else if (e.Message.Contains("Could not resolve host"))
                    {
                        Debug.LogError("[Gem Importer] - Failed to connect");
                        //failed to connect message
                    }
                    else if (e.Message.Contains("A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond."))
                    {
                        Debug.LogError("[Gem Importer] - Connection Time Out");
                    }
                    Debug.Log(e.Message);
                }
            });
            myTest.Start();
            
        }
        else
        {
            //get the user to enter there device id
            //get the button the enter button to come here
        }
    }
    public void iosthingy(string path)
    {
        //load = Instantiate(LoadingBox);
        //load.GetComponent<GUI_MessageBox>().title = T.getText("STR_EXPORTING");
        //load.GetComponent<GUI_MessageBox>().message = T.getText("GEM_IMPORTER_EXPORTING");
        if (Directory.Exists(localAppdata + $"/GemMaker/Texture/"))
        {
            Directory.Delete(localAppdata + $"/GemMaker/Texture/", true);
            Directory.CreateDirectory(localAppdata + $"/GemMaker/Texture/");
        }
        ZipFile.ExtractToDirectory(path, localAppdata + $"/GemMaker/Texture/");

        AddToGHLIos();
        
    }
    public void ExportWiiU(string packname)
    {
        if (userData.instance.wiiUVersion == userData.WiiUVersion.None || !Directory.Exists(userData.instance.wiiULastUSB))
        {            //spawn pop up msg
                     //give packname
                     //get them to setup usb or ftp
            if (wiiUConfigOBJ == null)
            {
                wiiUConfigOBJ = Instantiate(wiiuconfig);
                wiiUConfigOBJ.GetComponent<WiiUSetup_Messagebox>().pack = packname;
            }
            else
            {
                Debug.LogWarning("GemChangerImporter] Your trying to Instantiate WiiU config UI. Only one can be active");
            }
        }
        else
        {
            Debug.Log("[GemChangerImporter] WIIU - exporting " + packname);
            StartCoroutine(Extract($"{Application.persistentDataPath}/GemMaker/Resource Pack/{packname}.stickpack", "WIIU"));
        }
    }

    private void ExportRPCS3(string packname)
    {
        if (userData.instance.LocalFilePath == "TEST")
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR RPCS3 FOLDER", "RPSC3 FOLDER", false);
            foreach (var path in paths)
            {
                if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
                {
                    Debug.Log($"[GemChangerImporter] {path} Is valid");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"[GemChangerImporter] {userData.instance.LocalFilePath} Is valid");
                    Debug.Log("[GemChangerImporter]  - exporting " + packname);
                    StartCoroutine(Extract($"{Application.persistentDataPath}/GemMaker/Resource Pack/{packname}.stickpack", "PS3"));
                }
                else
                {
                    GameObject t = Instantiate(MessageBox);
                    userData.instance.LocalFilePath = "TEST";
                    t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
                    t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
                }
            }
        }
        else if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
        {
            Debug.Log("[GemChangerImporter] RPCS3 - exporting " + packname);
            StartCoroutine(Extract($"{Application.persistentDataPath}/GemMaker/Resource Pack/{packname}.stickpack", "PS3"));
        }
    }

    private IEnumerator Extract(string path, string platform)
    {
        load = Instantiate(LoadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("STR_EXPORTING");
        load.GetComponent<GUI_MessageBox>().message = T.getText("GEM_IMPORTER_EXPORTING");
        yield return new WaitForEndOfFrame();
        if (Directory.Exists(Application.persistentDataPath + $"/GemMaker/Texture/"))
        {
            Directory.Delete(Application.persistentDataPath + $"/GemMaker/Texture/", true);
            Directory.CreateDirectory(Application.persistentDataPath + $"/GemMaker/Texture/");
        }
        ZipFile.ExtractToDirectory(path, Application.persistentDataPath + $"/GemMaker/Texture/");
        if (platform.ToLower() == "ps3")
        {
            StartCoroutine(AddToGHLRPCS3(platform.ToLower()));
        }
        if (platform.ToLower() == "wiiu")
        {
            WiiUcopyPromt();
        }
        if(platform.ToLower() == "ios" || platform.ToLower() == "tvos")
        {
            AddToGHLIos();
        }
    }

    private void WiiUcopyPromt()
    {
        //spawn msg box asking what method to copy the files by
    }

    private async Task AddtoGHLWiiU(bool ftp)
    {
        await Task.Delay(1);
        //general stuff
        string platform = "wiiu";
        string version = "none";
        if (userData.instance.wiiUVersion == userData.WiiUVersion.PAL)
        {
            version = "101bc600";
        }
        else if (userData.instance.wiiUVersion == userData.WiiUVersion.USA)
        {
            version = "101ba400";
        }
        else
        {
            Debug.LogError("[GemChangerImporter] - unknow wiiU version");
        }
        string RighGemImg = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRight";
        string LeftGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeft";

        string LeftHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeftHeroPowerActive";
        string RightHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRightHeroPowerActive";

        string LeftHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeftHeroPowerCollect";
        string RightHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRightHeroPowerCollect";

        string LeftStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeftStreak";
        string RightStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRightStreak";

        string LeftHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeftHopo";
        string RightHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRightHopo";

        string OpenGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/OpenGem";
        string OpenGemHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/OpenGemHeroPowerCollect";
        string OpenGemHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/OpenGemHeroPowerActive";
        string OpenGemStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/OpenGemStreak";
        string GemBoxIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemBox";

        string trailIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/Trail";
        string trailHeroIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/TrailHero";

        if (ftp)
        {
            //ftp crab

            //finding
            string gamelocated = "";
            if (await DoesFtpDirectoryExist($"storage_mlc/usr/title/000500e/{version}"))
            {
                gamelocated = "storage_mlc";
            }
            else if (await DoesFtpDirectoryExist($"storage_usb/usr/title/000500e/{version}"))
            {
                gamelocated = "storage_usb";
            }

            //all highways of GHL
            int num = 0;

            //removing the old gems if placed

            if (await DoesFtpDirectoryExist($"{gamelocated}/usr/title/0005000e/{version}/content/Update" + "/Override" + "/ART/MAYAPROJECTS".ToLower()))
            {
                await DeleteFolder($"{gamelocated}/usr/title/0005000e/{version}/content/Update" + "/Override" + "/ART/MAYAPROJECTS".ToLower());
                await CreateFolder($"{gamelocated}/usr/title/0005000e/{version}/content/Update" + "/Override" + "/ART/HUD/TEXTURES/".ToLower());
            }
            int highwayscount = main.OnDiskHighways.Count + main.Highways.Count + 2;

            //trails
            if (File.Exists($"{trailHeroIMG}.IMG"))
            {
                await UploadFile($"{trailHeroIMG}.IMG", $"{gamelocated}/usr/title/0005000e/{version}/content/Update" + "/Override" + "/ART/HUD/TEXTURES/TRAIL_CYAN.IMG".ToLower());
            }
            num++;
            if (File.Exists($"{trailIMG}.IMG"))
            {
                await UploadFile($"{trailIMG}.IMG", $"{gamelocated}/usr/title/0005000e/{version}/content/Update" + "/Override" + "/ART/HUD/TEXTURES/TRAIL_GREY.IMG".ToLower());
            }
            num++;

            //Highway exceptions
            //for some reason some highways have the right foldername but have the wrong file name
            string[] highwaysblacklist = { "RETROTRIANGLESA" };
            string[] highwaysblacklistMain = { "RETROTRIANGLES" };

            foreach (string highway in main.Highways)
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

                load.GetComponent<GUI_MessageBox>().message = $"Please wait while Export your textures\n\nConverted: {num}/{highwayscount}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                num++;

                //CHECKING IF Folder EXIST
                if (!await DoesFtpDirectoryExist($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/".ToLower()))
                {
                    await CreateFolder($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/".ToLower());
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.IMG"))
                {
                    await UploadFile($"{RighGemImg}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEM_COL_SRGB_00.1001.IMG".ToLower());
                }

                if (File.Exists($"{LeftGemIMG}.IMG"))
                {
                    File.Copy($"{LeftGemIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{LeftHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROACTIVELEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{RightHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{LeftHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROCOLLECTLEFT_COL_SRGB_00.1001.IMG", true);
                }
                if (File.Exists($"{RightHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{RightHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftStreakIMG}.IMG"))
                {
                    File.Copy($"{LeftStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMSTREAKLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightStreakIMG}.IMG"))
                {
                    File.Copy($"{RightStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHopoIMG}.IMG"))
                {
                    File.Copy($"{LeftHopoIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHOPOLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHopoIMG}.IMG"))
                {
                    File.Copy($"{RightHopoIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHOPO_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemIMG}.IMG"))
                {
                    File.Copy($"{OpenGemIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUM_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{OpenGemHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{OpenGemHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemStreakIMG}.IMG"))
                {
                    File.Copy($"{OpenGemStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{GemBoxIMG}.IMG"))
                {
                    File.Copy($"{GemBoxIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/CATCHER_{highwaya}_BOX_COL_SRGB_00.1001.IMG", true);
                }
            }

            foreach (string highway in main.OnDiskHighways)
            {
                load.GetComponent<GUI_MessageBox>().message = $"Please wait while Export your textures\n\nConverted: {num}/{highwayscount}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                num++;

                //CHECKING IF Folder EXIST
                if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/"))
                {
                    Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/");
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.IMG"))
                {
                    File.Copy($"{RighGemImg}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEM_COL_SRGB_00.1001.IMG", true);
                    if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEM_COL_SRGB_00.1001.IMG"))
                    {
                        Debug.Log($"[GemChangerImporter] WE COPY GEM_{highway}_GEM_COL_SRGB_00.1001.IMG TO RPCS3");
                    }
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

                if (File.Exists($"{GemBoxIMG}.IMG"))
                {
                    File.Copy($"{GemBoxIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/CATCHER_{highway}_BOX_COL_SRGB_00.1001.IMG", true);
                }
            }
        }
        else
        {
            //all highways of GHL
            int num = 0;

            //removing the old gems if placed

            if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS"))
            {
                Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS", true);
            }
            int highwayscount = main.OnDiskHighways.Count + main.Highways.Count + 2;

            //trails
            //CHECKING IF Folder EXIST
            if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/"))
            {
                Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/");
            }
            if (File.Exists($"{trailHeroIMG}.IMG"))
            {
                File.Copy($"{trailHeroIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/TRAIL_CYAN.IMG", true);
                if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/TRAIL_CYAN.IMG"))
                {
                    Debug.Log("[GemChangerImporter] WE COPY TRAIL_CYAN TO RPCS3");
                }
            }
            num++;
            if (File.Exists($"{trailIMG}.IMG"))
            {
                File.Copy($"{trailIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}//USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/TRAIL_GREY.IMG", true);
            }
            num++;

            //Highway exceptions
            //for some reason some highways have the right foldername but have the wrong file name
            string[] highwaysblacklist = { "RETROTRIANGLESA" };
            string[] highwaysblacklistMain = { "RETROTRIANGLES" };

            foreach (string highway in main.Highways)
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

                load.GetComponent<GUI_MessageBox>().message = $"Please wait while Export your textures\n\nConverted: {num}/{highwayscount}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                num++;

                //CHECKING IF Folder EXIST
                if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/"))
                {
                    Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/");
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.IMG"))
                {
                    File.Copy($"{RighGemImg}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEM_COL_SRGB_00.1001.IMG", true);
                    if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEM_COL_SRGB_00.1001.IMG"))
                    {
                        Debug.Log($"[GemChangerImporter] WE COPY GEM_{highway}_GEM_COL_SRGB_00.1001.IMG TO RPCS3");
                    }
                }

                if (File.Exists($"{LeftGemIMG}.IMG"))
                {
                    File.Copy($"{LeftGemIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{LeftHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROACTIVELEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{RightHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{LeftHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROCOLLECTLEFT_COL_SRGB_00.1001.IMG", true);
                }
                if (File.Exists($"{RightHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{RightHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftStreakIMG}.IMG"))
                {
                    File.Copy($"{LeftStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMSTREAKLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightStreakIMG}.IMG"))
                {
                    File.Copy($"{RightStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHopoIMG}.IMG"))
                {
                    File.Copy($"{LeftHopoIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHOPOLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHopoIMG}.IMG"))
                {
                    File.Copy($"{RightHopoIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHOPO_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemIMG}.IMG"))
                {
                    File.Copy($"{OpenGemIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUM_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{OpenGemHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{OpenGemHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemStreakIMG}.IMG"))
                {
                    File.Copy($"{OpenGemStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{GemBoxIMG}.IMG"))
                {
                    File.Copy($"{GemBoxIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/CATCHER_{highwaya}_BOX_COL_SRGB_00.1001.IMG", true);
                }
            }

            foreach (string highway in main.OnDiskHighways)
            {
                load.GetComponent<GUI_MessageBox>().message = $"Please wait while Export your textures\n\nConverted: {num}/{highwayscount}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                num++;

                //CHECKING IF Folder EXIST
                if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/"))
                {
                    Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/");
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.IMG"))
                {
                    File.Copy($"{RighGemImg}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEM_COL_SRGB_00.1001.IMG", true);
                    if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEM_COL_SRGB_00.1001.IMG"))
                    {
                        Debug.Log($"[GemChangerImporter] WE COPY GEM_{highway}_GEM_COL_SRGB_00.1001.IMG TO RPCS3");
                    }
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

                if (File.Exists($"{GemBoxIMG}.IMG"))
                {
                    File.Copy($"{GemBoxIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/CATCHER_{highway}_BOX_COL_SRGB_00.1001.IMG", true);
                }
            }

            //clean up
            if (Directory.Exists(Application.persistentDataPath + $"/GemMaker/Texture/"))
            {
                Directory.Delete(Application.persistentDataPath + $"/GemMaker/Texture/", true);
            }
        }
    }

    private void AddToGHLIos()
    {
        //ssh stuff
        Thread myTest = new System.Threading.Thread(delegate ()
        {
            Debug.LogWarning("[Gem Importer] - Threading SSH");
            //attemp ssh
            var connectionInfo = new ConnectionInfo($"{userData.instance.SSHiosIP}", "root", new PasswordAuthenticationMethod("root", userData.instance.SSHiosPassword));
            try
            {
                var client = new SftpClient(connectionInfo);

                client.Connect();
                //general stuff
                string platform = "ios";
                bool trails = false;
                bool highwayb = false;
                //checking if path is GHL
                try
                {
                    client.ChangeDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app");
                }
                catch (SftpPathNotFoundException)
                {
                    Debug.LogError($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} isn't GHL install Bailing");
                    throw new Exception("IOS GHL installed Not Found");
                }
                //check if a backup folder exist
                try
                {
                    client.ChangeDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways/_backup");
                    ////checking if there files in there
                    var highways2 = client.ListDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways/_backup");
                    int count = 0;
                    foreach (var highway in highways2)
                    {
                        count++;
                    }
                    if (count <= 13)
                    {
                        //backup failed tell the player they should reinstall the game if they want to restore the highways
                        //create new backup
                        highwayb = false;
                        Debug.LogError($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Backup is missing content. If you want OG gems back you have to reinstall the game");

                        //dumb fix for deleting dir
                        foreach (var file in highways2)
                        {
                            if ((file.Name != ".") && (file.Name != ".."))
                            {
                                if (file.IsDirectory)
                                {
                                    client.DeleteDirectory(file.FullName);
                                }
                                else
                                {
                                    client.DeleteFile(file.FullName);
                                }
                            }
                        }
                        client.DeleteDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways/_backup");
                    }
                    else
                    {
                        highwayb = true;
                    }

                    //check if backup of hudguitar_common.far
                    client.ChangeDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/_backup");
                    var fars = client.ListDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/_backup");
                    foreach (var far in fars)
                    {
                        if (far.Name == "hudguitar_common.far")
                        {
                            trails = true;
                            Debug.Log("Trails true");
                            break;
                        }
                    }
                    if (!trails)
                    {
                        Debug.LogError($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Backup is missing content. If you want OG trails back you have to reinstall the game");
                        //dumb fix for deleting dir
                        foreach (var file in fars)
                        {
                            if ((file.Name != ".") && (file.Name != ".."))
                            {
                                if (file.IsDirectory)
                                {
                                    client.DeleteDirectory(file.FullName);
                                }
                                else
                                {
                                    client.DeleteFile(file.FullName);
                                }
                            }
                        }
                        client.DeleteDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/_backup");
                    }
                    if (!trails || !highwayb)
                    {
                        throw new SftpPathNotFoundException();
                    }
                }
                //if doesn't exist make one and copy the highways
                catch (SftpPathNotFoundException)
                {
                    //backup the hudguitar_common.far
                    if (!highwayb)
                    {
                        client.ChangeDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways");
                        Debug.LogError($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Doesn't have highway backup folder \n Creating backup");
                        client.CreateDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways/_backup");
                        var highways = client.ListDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways");
                        foreach (var far in highways)
                        {
                            if (far.Name.Contains(".far"))
                            {
                                Debug.LogWarning(far.Name);
                                var fsIn = client.OpenRead($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways/{far.Name}");
                                var fsOut = client.OpenWrite($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways/_backup/{far.Name}");

                                int data;
                                while ((data = fsIn.ReadByte()) != -1)
                                    fsOut.WriteByte((byte)data);

                                fsOut.Flush();
                                fsIn.Close();
                                fsOut.Close();
                            }
                        }
                    }
                }
                if (!trails)
                {
                    client.ChangeDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far");
                    Debug.LogError($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Doesn't have Trails backup folder \n Creating backup");
                    client.CreateDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/_backup");
                    var highways = client.ListDirectory($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far");
                    foreach (var far in highways)
                    {
                        if (far.Name.Contains(".far"))
                        {
                            Debug.LogWarning(far.Name);
                            var fsIn = client.OpenRead($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/{far.Name}");
                            var fsOut = client.OpenWrite($"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/_backup/{far.Name}");

                            int data;
                            while ((data = fsIn.ReadByte()) != -1)
                                fsOut.WriteByte((byte)data);

                            fsOut.Flush();
                            fsIn.Close();
                            fsOut.Close();
                        }
                    }
                    Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Backup complete");
                }

                //copy highway
                //ghtv
                if (File.Exists(localAppdata + $"/GemMaker/Texture/{platform}/hwlprimary.far"))
                {
                    Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Uploading hwlprimary");
                    var fileStream = new FileStream(localAppdata + $"/GemMaker/Texture/{platform}/hwlprimary.far", FileMode.Open);
                    client.UploadFile(fileStream, $"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/highways/hwlprimary.far");
                    Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Done hwlprimary");
                }
                if (File.Exists(localAppdata + $"/GemMaker/Texture/{platform}/hudguitar_common.far"))
                {
                    Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Uploading hudguitar_common");
                    var fileStream = new FileStream(localAppdata + $"/GemMaker/Texture/{platform}/hudguitar_common.far", FileMode.Open);
                    client.UploadFile(fileStream, $"/private/var/containers/Bundle/Application/{userData.instance.gameIosUUID}/TitleUpdate_3.1_iOS_SF_2014924_Master_Developer_WW.app/far/hudguitar_common.far");
                    Debug.Log($"[Gem Importer] - {client.ConnectionInfo.Username} - {userData.instance.gameIosUUID} Done hudguitar_common");
                }

                //clean up
                if (Directory.Exists(localAppdata + $"/GemMaker/Texture/"))
                {
                    Directory.Delete(localAppdata + $"/GemMaker/Texture/", true);
                }
                client.Disconnect();
                client.Dispose();
                Debug.Log($"[Gem Importer] - ios Done");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        });
        myTest.Start();
    }

    private IEnumerator AddToGHLRPCS3(string platform)
    {
        yield return new WaitForSeconds(8);
        bool hasver = false;

        string RighGemImg = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRight";
        string LeftGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeft";

        string LeftHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeftHeroPowerActive";
        string RightHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRightHeroPowerActive";

        string LeftHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeftHeroPowerCollect";
        string RightHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRightHeroPowerCollect";

        string LeftStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeftStreak";
        string RightStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRightStreak";

        string LeftHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemLeftHopo";
        string RightHopoIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemRightHopo";

        string OpenGemIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/OpenGem";
        string OpenGemHeroCollectIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/OpenGemHeroPowerCollect";
        string OpenGemHeroActiveIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/OpenGemHeroPowerActive";
        string OpenGemStreakIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/OpenGemStreak";
        string GemBoxIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/GemBox";

        string trailIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/Trail";
        string trailHeroIMG = Application.persistentDataPath + $"/GemMaker/Texture/{platform}/TrailHero";
        //loop though the list of loc so it apply to GHTV and GHL gems

        //for updating from older version of the tool
        string version = "";
        if (userData.instance.gameVersion == userData.Version.PAL || userData.instance.gameVersion == userData.Version.Lite)
        {
            version = "BLES02180";
            hasver = true;
        }
        else if (userData.instance.gameVersion == userData.Version.USA)
        {
            version = "BLUS31556";
            hasver = true;
        }
        else
        {
            Debug.LogError("[HopoPatch] USER HASN'T SELECTED A GAME REGION");
        }
        if (hasver)
        {
            //all highways of GHL
            int num = 0;

            //removing the old gems if placed

            if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS"))
            {
                Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS", true);
            }
            int highwayscount = main.OnDiskHighways.Count + main.Highways.Count + 2;

            //trails
            //CHECKING IF Folder EXIST
            if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/"))
            {
                Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/");
            }
            if (File.Exists($"{trailHeroIMG}.IMG"))
            {
                File.Copy($"{trailHeroIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/TRAIL_CYAN.IMG", true);
                if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/TRAIL_CYAN.IMG"))
                {
                    Debug.Log("[GemChangerImporter] WE COPY TRAIL_CYAN TO RPCS3");
                }
            }
            num++;
            if (File.Exists($"{trailIMG}.IMG"))
            {
                File.Copy($"{trailIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}//USRDIR/UPDATE/OVERRIDE/ART/HUD/TEXTURES/TRAIL_GREY.IMG", true);
            }
            num++;

            //Highway exceptions
            //for some reason some highways have the right foldername but have the wrong file name
            string[] highwaysblacklist = { "RETROTRIANGLESA" };
            string[] highwaysblacklistMain = { "RETROTRIANGLES" };

            foreach (string highway in main.Highways)
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
                load.GetComponent<GUI_MessageBox>().message = $"Please wait while Export your textures\n\nConverted: {num}/{highwayscount}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                num++;

                //CHECKING IF Folder EXIST
                if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/"))
                {
                    Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/");
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.IMG"))
                {
                    File.Copy($"{RighGemImg}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEM_COL_SRGB_00.1001.IMG", true);
                    if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEM_COL_SRGB_00.1001.IMG"))
                    {
                        Debug.Log($"[GemChangerImporter] WE COPY GEM_{highway}_GEM_COL_SRGB_00.1001.IMG TO RPCS3");
                    }
                }

                if (File.Exists($"{LeftGemIMG}.IMG"))
                {
                    File.Copy($"{LeftGemIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{LeftHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROACTIVELEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{RightHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{LeftHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROCOLLECTLEFT_COL_SRGB_00.1001.IMG", true);
                }
                if (File.Exists($"{RightHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{RightHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftStreakIMG}.IMG"))
                {
                    File.Copy($"{LeftStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMSTREAKLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightStreakIMG}.IMG"))
                {
                    File.Copy($"{RightStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{LeftHopoIMG}.IMG"))
                {
                    File.Copy($"{LeftHopoIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHOPOLEFT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{RightHopoIMG}.IMG"))
                {
                    File.Copy($"{RightHopoIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_GEMHOPO_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemIMG}.IMG"))
                {
                    File.Copy($"{OpenGemIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUM_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroCollectIMG}.IMG"))
                {
                    File.Copy($"{OpenGemHeroCollectIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMHEROCOLLECT_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemHeroActiveIMG}.IMG"))
                {
                    File.Copy($"{OpenGemHeroActiveIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMHEROACTIVE_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{OpenGemStreakIMG}.IMG"))
                {
                    File.Copy($"{OpenGemStreakIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/GEM_{highwaya}_STRUMSTREAK_COL_SRGB_00.1001.IMG", true);
                }

                if (File.Exists($"{GemBoxIMG}.IMG"))
                {
                    File.Copy($"{GemBoxIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DEFAULT/{highway}/TEXTURES/CATCHER_{highwaya}_BOX_COL_SRGB_00.1001.IMG", true);
                }
            }

            foreach (string highway in main.OnDiskHighways)
            {
                yield return new WaitForEndOfFrame();
                load.GetComponent<GUI_MessageBox>().message = $"Please wait while Export your textures\n\nConverted: {num}/{highwayscount}";
                load.GetComponent<GUI_MessageBox>().addmessage();
                num++;

                //CHECKING IF Folder EXIST
                if (!Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/"))
                {
                    Directory.CreateDirectory($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/");
                }

                //checking if IMG exist from the bat file. this allow users to only update certain textures
                if (File.Exists($"{RighGemImg}.IMG"))
                {
                    File.Copy($"{RighGemImg}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEM_COL_SRGB_00.1001.IMG", true);
                    if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/GEM_{highway}_GEM_COL_SRGB_00.1001.IMG"))
                    {
                        Debug.Log($"[GemChangerImporter] WE COPY GEM_{highway}_GEM_COL_SRGB_00.1001.IMG TO RPCS3");
                    }
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

                if (File.Exists($"{GemBoxIMG}.IMG"))
                {
                    File.Copy($"{GemBoxIMG}.IMG", $"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS/STAGEFRIGHT/SCENES/MODEL/HIGHWAYS/DISK/{highway}/TEXTURES/CATCHER_{highway}_BOX_COL_SRGB_00.1001.IMG", true);
                }
            }

            yield return new WaitForEndOfFrame();
            //clean up
            if (Directory.Exists(Application.persistentDataPath + $"/GemMaker/Texture/"))
            {
                Directory.Delete(Application.persistentDataPath + $"/GemMaker/Texture/", true);
            }

            load.GetComponent<GUI_MessageBox>().CloseAnim();
            yield return new WaitForSeconds(1);
            //finish message
            GameObject a = Instantiate(MessageBox);
            a.GetComponent<GUI_MessageBox>().title = T.getText("STR_EXPORTDONE");
            a.GetComponent<GUI_MessageBox>().message = T.getText("GEM_IMPORTER_EXPORT_DONE");
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
            Debug.LogWarning("[GemChangerImporter] Finish exporting texture");
        }
    }

    public void ReturnToMainMenu()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

    public void FindMoreGemPacks()
    {
        Application.OpenURL("https://discord.com/channels/758530768640802826/1061536695624925185");
        // this link shouldn't be hard coded in, should be dynamic
    }

    public async Task<bool> CreateFolder(string path)
    {
        bool IsCreated = true;
        try
        {
            WebRequest request = WebRequest.Create(userData.instance.wiiuFtpIp + path);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential("admin", "admin");
            using (var resp = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine(resp.StatusCode);
            }
        }
        catch (Exception ex)
        {
            IsCreated = false;
        }
        return IsCreated;
    }

    public async Task<bool> DeleteFolder(string path)
    {
        bool IsCreated = true;
        try
        {
            WebRequest request = WebRequest.Create(userData.instance.wiiuFtpIp + path);
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;
            request.Credentials = new NetworkCredential("admin", "admin");
            using (var resp = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine(resp.StatusCode);
            }
        }
        catch (Exception ex)
        {
            IsCreated = false;
        }
        return IsCreated;
    }

    public async Task<bool> DoesFtpDirectoryExist(string dirPath)
    {
        bool isexist = false;

        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(dirPath);
            request.Credentials = new NetworkCredential("admin", "admin");
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                isexist = true;
            }
        }
        catch (WebException ex)
        {
            if (ex.Response != null)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
                }
            }
        }
        return isexist;
    }

    public async Task UploadFile(string LocalFile, string OnlineFile)
    {
        OnlineFile = "ftp://212.102.43.88/" + OnlineFile;

        try
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential("admin", "admin");
                client.UploadFileAsync(new Uri("ftp://" + OnlineFile), WebRequestMethods.Ftp.UploadFile, LocalFile);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Admin Tool] FTP UPLOAD ERROR " + e.Message);
        }
    }
}