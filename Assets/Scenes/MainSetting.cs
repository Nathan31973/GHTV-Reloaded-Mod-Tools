using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net;
using System.ComponentModel;

public class MainSetting : MonoBehaviour
{
    private GUI_MessageBox host;
    public GameObject loadingBox;
    public GameObject MessageBox;
    public TextMeshProUGUI version;
    public GameObject Credits;

    private Translater T;
    private bool checking = false;

    [Header("UI ELEMETS")]
    public Toggle autoupdate;

    [Header("Platform related things")]
    public Button RPCS3Folder;
    public TMP_Dropdown platform;
    public GameObject TV_button;

    private void Start()
    {
        if(userData.instance.gameVersion != userData.Version.Lite)
        {
            TV_button.SetActive(false);
        }
        platform.value = (int)userData.instance.platform;
        autoupdate.isOn = userData.instance.AutoGameUpdate;
        T = Translater.instance;
        version.text = $"V{Application.version}";
        host = gameObject.GetComponent<GUI_MessageBox>();
    }

    public void Back()
    {
        host.CloseAnim();
    }
    public void SeleteRPCS3()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel(T.getText("MSG_SEL_RPCS3"), T.getText("MGS_RPCS3_FOLD"), false);
        foreach (var path in paths)
        {
            if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
            {
                Debug.Log($"[MainSettings] {path} Is valid");
                userData.instance.LocalFilePath = path;
            }
            else
            {
                GameObject t = Instantiate(MessageBox);
                t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
                t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
            }
        }
    }
    public void ClearCache()
    {
        StartCoroutine(Clearing());
    }
    public void ClearHyperSpeedCache()
    {
        if (Directory.Exists(Application.persistentDataPath + "/External_Tools/HyperSpeed"))
        {
            Directory.Delete(Application.persistentDataPath + "/External_Tools/HyperSpeed", true);
        }
        GameObject t = Instantiate(MessageBox);
        t.GetComponent<GUI_MessageBox>().title = T.getText("STR_HYPER_CACHE_CLEAR");
        t.GetComponent<GUI_MessageBox>().message = $"";
    }
    private IEnumerator Clearing()
    {
        GameObject load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("STR_DEL_CACHE_FILES");
        load.GetComponent<GUI_MessageBox>().message = T.getText("STR_WAIT_DEL_CACHE_FILES");
        if (Directory.Exists($"{Application.persistentDataPath}/External_Tools"))
        {
            Directory.Delete($"{Application.persistentDataPath}/External_Tools", true);
        }
        yield return new WaitForEndOfFrame();
        if (Directory.Exists($"{Application.persistentDataPath}/External_Tools")) 
        {
            Directory.Delete($"{Application.persistentDataPath}/External_Tools", true);
        }
        yield return new WaitForEndOfFrame();
        if (Directory.Exists($"{Application.persistentDataPath}/GemMaker/Exported Pack"))
        {
            Directory.Delete($"{Application.persistentDataPath}/GemMaker/Exported Pack", true);
        }
        yield return new WaitForEndOfFrame();
        if (Directory.Exists($"{Application.persistentDataPath}/GemMaker/Texture"))
        {
            Directory.Delete($"{Application.persistentDataPath}/GemMaker/Texture", true);
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{Application.persistentDataPath}/PowerVRSDKSetup-4.0.exe"))
        {
            File.Delete($"{Application.persistentDataPath}/PowerVRSDKSetup-4.0.exe");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{Application.persistentDataPath}/GHLIMGConverter.zip"))
        {
            File.Delete($"{Application.persistentDataPath}/GHLIMGConverter.zip");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{Application.persistentDataPath}/Hyperspeed.zip"))
        {
            File.Delete($"{Application.persistentDataPath}/Hyperspeed.zip");
        }
        yield return new WaitForEndOfFrame();
        if (File.Exists($"{Application.persistentDataPath}/OfficialGemPack.zip"))
        {
            File.Delete($"{Application.persistentDataPath}/OfficialGemPack.zip");
        }
        yield return new WaitForEndOfFrame();
        if (Directory.Exists($"{Application.persistentDataPath}/GuitarHeroLiveUpdates"))
        {
            Directory.Delete($"{Application.persistentDataPath}/GuitarHeroLiveUpdates", true);
        }
        yield return new WaitForEndOfFrame();
        if (Directory.Exists($"{Application.persistentDataPath}/RPCS3 Patch"))
        {
            Directory.Delete($"{Application.persistentDataPath}/RPCS3 Patch", true);
        }
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        yield return new WaitForSeconds(1);
    }
    public void DeleteSave()
    {
        File.Delete($"{Application.persistentDataPath}/save.game");
        Debug.Log(Application.platform);
        userData.instance.LocalFilePath = $"TEST";
        DataPersistenceManager.instance.freshStart();
        SceneManager.LoadScene("Setup");
    }
    public void OPENCREDITS()
    {
        Instantiate(Credits);
    }

    public void ToggleAutoUpdate(bool val)
    {
        userData.instance.AutoGameUpdate = val;
    }

    public void checkforupdate()
    {
        if (!checking)
        {
            StartCoroutine(DOCHECK());
        }
    }

    private IEnumerator DOCHECK()
    {
        GameObject load;
        MainMenu Mainmenu = GameObject.Find("Main Camera").GetComponent<MainMenu>();

        load = Instantiate(Mainmenu.LoadingBox);
        load.GetComponent<GUI_MessageBox>().title = T.getText("NET_CHECK_FOR_GAME_UPDATE");
        load.GetComponent<GUI_MessageBox>().message = T.getText("NET_CHECK_FOR_GAME_UPDATE_DES");
        yield return new WaitForSeconds(2);
        bool gameUpToDate = false;
        string[] lines = File.ReadAllLines(Application.persistentDataPath + "/GHL_PS3_Version.dat");
        string gameversion = lines[0];
        string clientver = "";
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
            Debug.LogError("[MAINMENU] USER HASN'T SELECTED A GAME REGION");

        }
        if (region != "")
        {
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
                load.GetComponent<GUI_MessageBox>().CloseAnim();
                Debug.Log("GHL need to update");
                //download GHL update from github
                GameObject a = Instantiate(Mainmenu.messagebox2);
                a.GetComponent<GUI_MessageBox>().title = T.getText("STR_GHLR_UPDATE");
                a.GetComponent<GUI_MessageBox>().message = $"{T.getText("STR_GHLR_FOUND")} \n\n{T.getText("STR_LATESTVER")}{gameversion}\n{T.getText("STR_GAMEVER")}{split[57]}";
                a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(Mainmenu.confirmdownload);
            }
            else
            {
                load.GetComponent<GUI_MessageBox>().CloseAnim();
                GameObject a = Instantiate(Mainmenu.MessageBox);
                a.GetComponent<GUI_MessageBox>().title = T.getText("NET_ERROR_GAME_UP_TODATE");
                a.GetComponent<GUI_MessageBox>().message = T.getText("NET_ERROR_GAME_UP_TODATE_DES");
            }
        }
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("[Pause] Quitting app");
    }

    public void PlatformSelect(int val)
    {
        if(val == 0)
        {
            RPCS3Folder.interactable = true;
            userData.instance.platform = userData.Platform.Rpcs3;
        }
        else if (val == 1)
        {
            RPCS3Folder.interactable = false;
            userData.instance.platform = userData.Platform.WiiU;
        }
        else if (val == 2)
        {
            RPCS3Folder.interactable = false;
            userData.instance.platform = userData.Platform.PS3;
        }
        else if (val == 3)
        {
            RPCS3Folder.interactable = false;
            userData.instance.platform = userData.Platform.Ios;
        }
        else if (val == 4)
        {
            RPCS3Folder.interactable = false;
            userData.instance.platform = userData.Platform.TVOS;
        }
    }
    public void TVFIX()
    {
        if(!Directory.Exists(Application.persistentDataPath + "/Savefix"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Savefix");
        }
        if(!File.Exists(Application.persistentDataPath + "/Savefix/GHLIVE.DAT"))
        {
            
            //download the file
            btnDownload_Click("https://github.com/Nathan31973/GHTV-Reloaded-Mods-Tools-Assets/raw/main/GHLIVE.DAT", Application.persistentDataPath + "/Savefix/GHLIVE.DAT");
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (Directory.Exists(userData.instance.LocalFilePath + $"/dev_hdd0/home/0000000{i}/savedata/BLES02180-GHLIVE"))
                {
                    File.Copy(Application.persistentDataPath + "/Savefix/GHLIVE.DAT", userData.instance.LocalFilePath + $"/dev_hdd0/home/0000000{i}/savedata/BLES02180-GHLIVE/GHLIVE.DAT", true);
                    GameObject t = Instantiate(MessageBox);
                    t.GetComponent<GUI_MessageBox>().title = T.getText("STR_SETTINGS_SAVECOPY");
                    t.GetComponent<GUI_MessageBox>().message = T.getText("STR_SETTINGS_SAVECOPY_MSG");
                    break;
                }
            }
        }
    }

    private void btnDownload_Click(string url, string filename)
    {
        WebClient webClient = null;
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[Settings] Error. Check internet connection!");
        }
        else
        {
            Debug.Log("[Settings] We have internet");
            // Is file downloading yet?
            if (webClient != null)
            {
                Debug.LogError("[Settings] Webclient in used");
                return;
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            webClient = new WebClient();

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver3);
                webClient.DownloadFileAsync(new Uri($"{url}"), filename);
            
        }
    }
    private void CompletedConver3(object sender, AsyncCompletedEventArgs e)
    {
        Debug.Log("[Settings] Download completed!");
        TVFIX();

    }

}

