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

    // Start is called before the first frame update
    public void GemChangerImporterStart()
    {
        DiscordController.instance.UpdateDiscordActivity("Gem Changer", "Importing a Gem Pack", "gem_changer", "Gem Changer");
        if (!Directory.Exists($"{Application.persistentDataPath}/GemMaker/Resource Pack"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Resource Pack");
        }
        if (!userData.instance.hasDownloadedDefaultAtLeastOnce)
        {
            GameObject a = Instantiate(MessageBox2);
            GUI_MessageBox b = a.GetComponent<GUI_MessageBox>();
            b.title = "Download Official Pack";
            b.message = "Would you like to download the Official GHTV Reloaded Gem Packs?";
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
        btnDownload_Click("https://github.com/Nathan31973/GHTV-Reloaded-Official-Gem-Packs/archive/refs/heads/main.zip", "OfficialGemPack.zip");
    }
    public void MANUALDOWNLOADPACK()
    {
        GameObject a = Instantiate(MessageBox2);
        GUI_MessageBox b = a.GetComponent<GUI_MessageBox>();
        b.title = "Download Official Pack";
        b.message = "Would you like to download the Official GHTV Reloaded Gem Packs?";
        b.button.onClick.AddListener(DownloadPacks);
    }

    private void btnDownload_Click(string url, string filename)
    {
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("Error. Check internet connection!");
            GameObject a = Instantiate(MessageBox);
            a.GetComponent<GUI_MessageBox>().title = "No Active Internet Connection";
            a.GetComponent<GUI_MessageBox>().message = $"No internet connect exist\n\nMake sure you are connected to the internet and try again.";
            return;
        }
        else
        {
            Debug.Log("We have internet");
            load = Instantiate(LoadingBox);
            load.GetComponent<GUI_MessageBox>().title = "Downloading";
            load.GetComponent<GUI_MessageBox>().message = "Please wait while we download Official GHTV Reloaded Gem Packs";
            // Is file downloading yet?
            if (webClient != null)
            {
                return;
            }

                isDownloading = true;
                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                webClient.QueryString.Add("file", Application.persistentDataPath + $"\\{filename}"); // To identify the file
                webClient.DownloadFileAsync(new Uri($"{url}"), Application.persistentDataPath + $"\\{filename}");

        }
    }

    private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        string fileProcessed = ((System.Net.WebClient)(sender)).QueryString["file"]; // Getting the local path if required
        Debug.Log("Download Recevedbyte: " + e.BytesReceived);
        Debug.Log("Download totalbyte: " + e.TotalBytesToReceive);
        // use these variables if needed
        load.GetComponent<GUI_MessageBox>().message = $"Please wait while we download Official GHTV Reloaded Gem Packs\n\n Dowloaded: {e.ProgressPercentage}%";
        load.GetComponent<GUI_MessageBox>().addmessage();
    }

    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        isDownloading = false;
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        Debug.Log("Download completed!");
        //checking if we don't have an old one
        if(File.Exists(Application.persistentDataPath + $"\\GemMaker\\defaultpack\\GHTV-Reloaded-Official-Gem-Packs-main\\default.stickpack"))
        {
            File.Delete(Application.persistentDataPath + $"\\GemMaker\\defaultpack\\GHTV-Reloaded-Official-Gem-Packs-main\\default.stickpack");
        }

        ZipFile.ExtractToDirectory(Application.persistentDataPath + $"\\OfficialGemPack.zip", Application.persistentDataPath + "\\GemMaker\\defaultpack");
        File.Delete(Application.persistentDataPath + $"\\OfficialGemPack.zip");
        StartCoroutine(extractFile(Application.persistentDataPath + $"\\GemMaker\\defaultpack\\GHTV-Reloaded-Official-Gem-Packs-main\\default.stickpack"));
    }
    private IEnumerator extractFile(string path)
    {
        load = Instantiate(LoadingBox);
        load.GetComponent<GUI_MessageBox>().title = "Importing";
        load.GetComponent<GUI_MessageBox>().message = "Please wait while we imported the Official GHTV Reloaded Gem Packs to your resource folder";
        using (ZipArchive archive = ZipFile.OpenRead(path))
        {

            foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(".stickpack")))
            {
                yield return new WaitForEndOfFrame();
                if (!Directory.Exists($"{Application.persistentDataPath}/GemMaker/Resource Pack/"))
                {
                    Directory.CreateDirectory($"{Application.persistentDataPath}/GemMaker/Resource Pack/");
                }
                entry.ExtractToFile(Path.Combine($"{Application.persistentDataPath}/GemMaker/Resource Pack/", entry.FullName),true);
            }
        }
        Directory.Delete(Application.persistentDataPath + $"\\GemMaker\\defaultpack", true);
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
                        Debug.Log("[Gem Changer - Importer] No Gem Pack found");
                        nopackmessage = Instantiate(MessageBox);
                        GUI_MessageBox b = nopackmessage.GetComponent<GUI_MessageBox>();
                        b.title = "No Gem Pack Found";
                        b.message = "You need to add gem pack into your 'resource pack' folder";
                        b.button.onClick.AddListener(OpenResourcePackFolder);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Could not find a part of the path"))
                {
                    Debug.Log("[Gem Changer - Importer] Can't find Path");

                }
            }
        }

    }
    public void OpenResourcePackFolder()
    {
        Application.OpenURL(@$"{Application.persistentDataPath}\GemMaker\Resource Pack");
        
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
                Debug.LogWarning("[GemChanger - Importer] Clearing Resource Pack Cache");
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
                    }

                    GameObject a = Instantiate(PackPrefab);
                    a.transform.SetParent(ScrollViewContent.transform, false);
                    a.GetComponent<ExportPackCoverGameObject>().host = this;
                    a.GetComponent<ExportPackCoverGameObject>().packName = Path.GetFileNameWithoutExtension(pack);
                    a.GetComponent<ExportPackCoverGameObject>().Creator = creator;
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
                    Debug.Log($"{path} Is valid");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"{userData.instance.LocalFilePath} Is valid");
                    Debug.Log("[GemChanger - Importer] - exporting " + packname);
                    StartCoroutine(Extract($"{Application.persistentDataPath}/GemMaker/Resource Pack/{packname}.stickpack"));
                }
                else
                {
                    GameObject t = Instantiate(MessageBox);
                    userData.instance.LocalFilePath = "TEST";
                    t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
                    t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
                }
            }
        }
        else if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
        {
            Debug.Log("[GemChanger - Importer] - exporting " + packname);
            StartCoroutine(Extract($"{Application.persistentDataPath}/GemMaker/Resource Pack/{packname}.stickpack"));
        }
        else
        {
            GameObject t = Instantiate(MessageBox);
            userData.instance.LocalFilePath = "TEST";
            t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
            t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
        }
    }

    private IEnumerator Extract(string path)
    {
        load = Instantiate(LoadingBox);
        load.GetComponent<GUI_MessageBox>().title = "Exporting";
        load.GetComponent<GUI_MessageBox>().message = "Please wait while Export your textures";
        yield return new WaitForEndOfFrame();
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
        foreach (string version in GameVersion)
        {
            if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS"))
            {
                Directory.Delete($"{userData.instance.LocalFilePath}/dev_hdd0/game/{version}/USRDIR/UPDATE/OVERRIDE/ART/MAYAPROJECTS", true);
            }
        }
        foreach (string highway in main.Highways)
        {
            yield return new WaitForEndOfFrame();
            load.GetComponent<GUI_MessageBox>().message = $"Please wait while Export your textures\n\nConverted: {num}/{main.Highways.Count}";
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


        load.GetComponent<GUI_MessageBox>().CloseAnim();
        yield return new WaitForSeconds(1);
        //finish message
        GameObject a = Instantiate(MessageBox);
        a.GetComponent<GUI_MessageBox>().title = "Exported Complete";
        a.GetComponent<GUI_MessageBox>().message = $"Custom Gems have been exported to Guitar Hero live.\n\nThe Next time you load the default highway or play GHL song the changes will be apply.";
        a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
        Debug.LogWarning("[Gem Changer - Importer] Finish exporting texture");
    }
    public void ReturnToMainMenu()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

}
