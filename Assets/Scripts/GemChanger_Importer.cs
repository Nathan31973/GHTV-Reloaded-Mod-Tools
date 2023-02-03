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

    // Start is called before the first frame update
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

    private void refreshUI()
    {
        if (!refreshing)
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
                    StartCoroutine(Extract($"{Application.persistentDataPath}/GemMaker/Resource Pack/{packname}.stickpack"));
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
            Debug.Log("[GemChangerImporter]  - exporting " + packname);
            StartCoroutine(Extract($"{Application.persistentDataPath}/GemMaker/Resource Pack/{packname}.stickpack"));
        }
        else
        {
            GameObject t = Instantiate(MessageBox);
            userData.instance.LocalFilePath = "TEST";
            t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
            t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
        }
    }

    private IEnumerator Extract(string path)
    {
        load = Instantiate(LoadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("STR_EXPORTING");
        load.GetComponent<GUI_MessageBox>().message = T.getText("GEM_IMPORTER_EXPORTING");
        yield return new WaitForEndOfFrame();
        if (Directory.Exists(Application.persistentDataPath + $"/GemMaker/Texture/"))
        {
            Directory.Delete(Application.persistentDataPath + $"/GemMaker/Texture/", true);
        }
        using (ZipArchive archive = ZipFile.OpenRead(path))
        {
            foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(".IMG")))
            {
                yield return new WaitForEndOfFrame();
                if (!Directory.Exists(Application.persistentDataPath + $"/GemMaker/Texture/"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + $"/GemMaker/Texture/");
                }
                entry.ExtractToFile(Path.Combine(Application.persistentDataPath + $"/GemMaker/Texture/", entry.FullName));
            }
        }
        StartCoroutine(AddToGHL());
    }

    private IEnumerator AddToGHL()
    {
        yield return new WaitForSeconds(8);
        bool hasver = false;
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
        string GemBoxIMG = Application.persistentDataPath + $"/GemMaker/Texture/GemBox";

        string trailIMG = Application.persistentDataPath + $"/GemMaker/Texture/Trail";
        string trailHeroIMG = Application.persistentDataPath + $"/GemMaker/Texture/TrailHero";
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
                for(int i = 0; i < highwaysblacklist.Length; i++)
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
            if (File.Exists($"{RighGemImg}.IMG"))
            {
                File.Delete(RighGemImg + ".IMG");
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{trailIMG}.IMG"))
            {
                File.Delete(trailIMG + ".IMG");
            }
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{trailHeroIMG}.IMG"))
            {
                File.Delete(trailHeroIMG + ".IMG");
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
            yield return new WaitForEndOfFrame();
            if (File.Exists($"{GemBoxIMG}.IMG"))
            {
                File.Delete(GemBoxIMG + ".IMG");
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
}