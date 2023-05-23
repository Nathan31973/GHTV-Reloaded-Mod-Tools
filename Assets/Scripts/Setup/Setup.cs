using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.RepresentationModel;

public class Setup : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Settings")]
    public bool GHLite = false;


    [Header("UI Elem")]
    public GameObject WelcomeScreen;

    [Header("UI Elem - Stages")]
    public List<GameObject> Stage1UI;

    public List<GameObject> Stage2UI;
    public List<GameObject> Stage3UI;
    public List<GameObject> Stage4UI;
    public GameObject doneUI;

    [Header("Messageboxes")]
    public GameObject MessageBox;
    public GameObject MessageBox2;
    public GameObject LoadingBox;
    private GameObject load;

    [Header("misc")]
    public int stage = 0;

    public int step = 0;
    public int maxStep = 0;
    public int part = 0;

    [Header("Step config")]
    public int stage0MaxStep = 0;

    public int stage1MaxStep = 5;
    public int stage2MaxStep = 2;
    public int stage3MaxStep = 2;
    public int stage4MaxStep = 3;

    public bool legPlayer = false;


    [Header("Extra")]
    public string username ="";

    [HideInInspector]public downloadPSNTitleUpdates psn;
    [HideInInspector]public Stage3_UI stage3_UIscript;
    public GameObject DownloadPrefab;
    public GameObject fadeToBlack;

    [Header("device")]
    public string device;
    private Translater T;
    private enum gameRegion
    {
        None,
        PAL,
        USA,
        Lite
    }

    private void Start()
    {
        T = Translater.instance;
        //checking if the welcome is display on screen
        DiscordController.instance.UpdateDiscordActivity("Setup", "Setting up GHTV: Reloaded for the first time", "mainmenu", "Main Menu");
        resetSetup();
    }

    // Update is called once per frame
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Setup");
        }
    }

    public void resetSetup()
    {
        step = 0;
        stage = 0;
        maxStep = stage0MaxStep;
        foreach (GameObject go in Stage1UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage2UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage3UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage4UI)
        {
            go.SetActive(false);
        }
        doneUI.SetActive(false);
        WelcomeScreen.SetActive(true);
        Debug.LogWarning("[SETUP] RESET SETUP.");
    }

    private void CompleteStep(int st, int mx)
    {
        //stage 0
        if (st == stage && step == mx)
        {
            Debug.LogWarning($"[SETUP] Stage:{stage} complete");
            stage++;
            step = 0;
            
            maxStep = mx;
        }
        else if (stage == st)
        {
            step++;
            Debug.LogWarning($"[SETUP] Step:{step} complete");
        }
        updateUI();
    }

    private void updateUI()
    {
        foreach (GameObject go in Stage1UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage2UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage3UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage4UI)
        {
            go.SetActive(false);
        }
        doneUI.SetActive(false);
        WelcomeScreen.SetActive(false);

        if (stage == 0)
        {
            WelcomeScreen.SetActive(true);
        }
        else if (stage == 1)
        {
            Stage1UI[step].SetActive(true);
        }
        else if (stage == 2)
        {
            Stage2UI[step].SetActive(true);
        }
        else if (stage == 3)
        {
            Stage3UI[step].SetActive(true);
        }
        else if (stage == 4)
        {
            Stage4UI[step].SetActive(true);
        }
        else if(stage == 5)
        {
            doneUI.SetActive(true);
        }    

    }

    //until
    private string getGameRegion()
    {
        if(userData.instance.gameVersion == userData.Version.PAL || userData.instance.gameVersion == userData.Version.Lite)
        {
            return "BLES02180";
        }
        else if (userData.instance.gameVersion == userData.Version.USA)
        {
            return "BLUS31556";
        }
        else
        {
            Debug.LogError("[SETUP] UNKNOW GAME VERSION");
            return null;
        }
    }
    private bool hasRPCS3Folder()
    {
        bool pathhasrpcs3 = false;

        var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR RPCS3 FOLDER", "RPSC3 FOLDER", false);
        foreach (var path in paths)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsEditor)
            {
                var files = Directory.GetFiles(path, "rpcs3" + ".*");
                if (files.Length > 0)
                {
                    pathhasrpcs3 = true;
                    Debug.Log($"[SETUP] rpcs3 is in this dir");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"[SETUP] {userData.instance.LocalFilePath} Is valid");
                    // at least one matching file exists
                    // file name is files[0]
                }
                else
                {
                    Debug.Log("[SETUP] rpcs3 isn't in this dir");
                }
            }
            else
            {
                
                //just find the dev_hdd0
                if (Directory.Exists($"{path}/dev_hdd0"))
                {
                    pathhasrpcs3 = true;
                    Debug.Log($"[SETUP] rpcs3 is in this dir");
                    userData.instance.LocalFilePath = path;
                    Debug.Log($"[SETUP] {userData.instance.LocalFilePath} Is valid");
                }
            }
        }

        if (!pathhasrpcs3)
        {
            GameObject t = Instantiate(MessageBox);
            userData.instance.LocalFilePath = "TEST";
            t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
            t.GetComponent<GUI_MessageBox>().message = T.getText("STR_RPCS3_RIGHT_FOLD");
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(HASRPCS3);
            return false;
        }
        else
        {
            return true;
        }
    }

    private void openURL(string url)
    {
        if (url.Contains("https://") || url.Contains("stickgaming.net"))
        {
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogWarning("[SETUP] URL isn't HTTPS, blocking");
        }
    }

    //welcome
    public void GETSTARTED()
    {
        CompleteStep(0, stage0MaxStep);
    }

    //stage 1

    //stage 1-1
    public void FIRSTTIMEPLAYING()
    {
        //install RPCS3
        CompleteStep(1, stage1MaxStep);
    }

    public void HASPLAYED()
    {
        legPlayer = true;
        HASRPCS3();
    }

    //stage 1-2
    public void HASRPCS3()
    {
        stage = 1;
        step = 0;
        CompleteStep(1, stage1MaxStep);
        bool rpcs3set = hasRPCS3Folder();
        if (rpcs3set)
        {
            if (legPlayer)
            {
                bool prequire = true;
                //check if game is installed
                USERHASGHL(false);
                //check if game is up to date

                //check if there deamonwareconfig doesn't contain the defaults settings

                if (prequire)
                {
                    stage = 2;
                    step = 1;
                    CompleteStep(2, stage2MaxStep);
                }
            }
            else
            {
                CompleteStep(1, stage1MaxStep);
                //ask if they want to install GHL
                if (GHLite)
                {
                    GameObject t = Instantiate(MessageBox2);
                    t.GetComponent<GUI_MessageBox>().title = T.getText("STR_DOWNLOAD_LITE_TITLE");
                    t.GetComponent<GUI_MessageBox>().message = T.getText("STR_DOWNLOAD_LITE_TITLE_DES");
                    t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(downloadGHTVLite);
                }
            }
            //stage completed
        }
    }

    public void downloadGHTVLite()
    {
        GameObject a = Instantiate(DownloadPrefab);
        DownloadMod DL = a.GetComponent<DownloadMod>();
        DL.btnDownload_Click(2, "http://download.ghtv.game.stickgaming.net/", $"{Application.persistentDataPath}/GuitarHeroLive-TVEdition.zip");
        StartCoroutine(WaitTillLiteInstall(DL));
    }

    private IEnumerator WaitTillLiteInstall(DownloadMod DL)
    {
        yield return new WaitUntil(() => DL.installLiteComplete == true);
        USERHASGHL(true);
        yield return new WaitForSeconds(0.1f);
        CheckIfUpdateHasBeenApplyed();
    }

    public void DOESNTHAVERPCS3()
    {
        openURL("https://rpcs3.net/download");
        GameObject t = Instantiate(MessageBox);
        t.GetComponent<GUI_MessageBox>().title = T.getText("SETUP_STR_LOCATED_RPCS3");
        t.GetComponent<GUI_MessageBox>().message = T.getText("SETUP_STR_LOCATED_RPCS3_DES");
        t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(HASRPCS3);
    }

    //stage 1-3
    public void USERHASGHL(bool over)
    {
        bool hasGHL = findGHL(over);
        if(hasGHL)
        {
            CompleteStep(1, stage1MaxStep);
        }
        else
        {
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = T.getText("SETUP_STR_NO_DISC");
            t.GetComponent<GUI_MessageBox>().message = T.getText("SETUP_STR_NO_DISC_DES");
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(HASRPCS3);
        }
    }

    private bool findGHL(bool Override)
    {
        if (Override)
        {
            var files = Directory.GetFiles($"{userData.instance.LocalFilePath}/games/Guitar Hero Live [BLES0218]/PS3_GAME/", "Lite" + ".*");
            if (files.Length > 0)
            {
                userData.instance.gameVersion = userData.Version.Lite;
                Debug.Log("[SETUP] Game Version Lite");
                return true;
            }
        }

        using (var reader = new StreamReader($"{userData.instance.LocalFilePath}/games.yml"))
        {
            // Load the stream
            var yaml = new YamlStream();
            yaml.Load(reader);
            // the rest
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            foreach (var entry in mapping.Children)
            {
                Debug.Log("[SETUP] GAMES.YML KEYS " +((YamlScalarNode)entry.Key).Value);
                if (((YamlScalarNode)entry.Key).Value.Contains("BLES02180"))
                {
                    if(Directory.Exists($"{userData.instance.LocalFilePath}/games/Guitar Hero Live [BLES0218]/PS3_GAME/"))
                    {
                        var files = Directory.GetFiles($"{userData.instance.LocalFilePath}/games/Guitar Hero Live [BLES0218]/PS3_GAME/", "Lite" + ".*");
                        if (files.Length > 0)
                        {
                            userData.instance.gameVersion = userData.Version.Lite;
                            Debug.Log("[SETUP] Game Version Lite");
                        }
                        else
                        {
                            userData.instance.gameVersion = userData.Version.PAL;
                            Debug.Log("[SETUP] Game Version PAL");
                        }

                    }
                    else
                    {
                        userData.instance.gameVersion = userData.Version.PAL;
                        Debug.Log("[SETUP] Game Version PAL");
                    }

                    return true;
                }
                if (((YamlScalarNode)entry.Key).Value.Contains("BLUS31556"))
                {
                    userData.instance.gameVersion = userData.Version.USA;
                    Debug.Log("[SETUP] Game Version USA");
                    return true;
                }

            }
            reader.Close();

        }
        return false;
    }

    //stage 1-4
    public void CheckIfUpdateHasBeenApplyed()
    {
        string region = getGameRegion(); //getting game region
        string[] split = null;
        bool gameUpToDate = false;
        if (region != null)
        {
            if (File.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/PARAM.SFO"))
            {
                string text = File.ReadAllText($"{userData.instance.LocalFilePath}/dev_hdd0/game/{region}/PARAM.SFO");
                split = text.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                foreach (string line in split)
                {
                    if (line.Contains("01.06") || line.Contains("01.07"))
                    {
                        Debug.LogWarning("[SETUP] Game is up to date(none modded update)");
                        gameUpToDate = true;
                    }
                    count++;
                }
            }
        }

        if(gameUpToDate)
        {
            CompleteStep(1, stage1MaxStep);
        }
        else
        {
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = T.getText("SETUP_STR_NO_FSG_UPDATE");
            t.GetComponent<GUI_MessageBox>().message = T.getText("SETUP_STR_NO_FSG_UPDATE_DES");
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(psn.opendownloadfolder);
        }
    }    

    //stage 1-5
    public void ConfigDone()
    {
        GameObject t = Instantiate(MessageBox2);
        t.GetComponent<GUI_MessageBox>().title = T.getText("STR_CONFIRM");
        t.GetComponent<GUI_MessageBox>().message = T.getText("SETUP_STR_CONFIRM_RPCS3_SETTING");
        t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ConfigConfirm);
    }
    public void ConfigConfirm()
    {
        CompleteStep(1, stage1MaxStep);
    }

    //stage 2

    //stage 2 - 1
    public void RPCNDONE()
    {
        string RPCNUserName = getRpcnUserName();
        if(RPCNUserName != null)
        {
            username = RPCNUserName;
            CompleteStep(2, stage2MaxStep);
        }
        else
        {
            GameObject t = Instantiate(MessageBox2);
            t.GetComponent<GUI_MessageBox>().title = T.getText("SETUP_STR_NONE_RPCN");
            t.GetComponent<GUI_MessageBox>().message = T.getText("SETUP_STR_NONE_RPCN_DES");
        }
    }
    private string getRpcnUserName()
    {
        string path = "";
        //check for rpcn.yml
        if (File.Exists($"{userData.instance.LocalFilePath}/config/rpcn.yml"))
        {
            path = $"{userData.instance.LocalFilePath}/config/rpcn.yml";
        }
        else if (File.Exists($"{userData.instance.LocalFilePath}/rpcn.yml"))
        {
            path = $"{userData.instance.LocalFilePath}/rpcn.yml";
        }
        else return null;

        try
        {
            using (var reader = new StreamReader(path))
            {
                // Load the stream
                var yaml = new YamlStream();
                yaml.Load(reader);
                // the rest
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                foreach (var entry in mapping.Children)
                {
                    print(((YamlScalarNode)entry.Key).Value);

                    if (((YamlScalarNode)entry.Key).Value.Contains("NPID"))
                    {
                        return (string)entry.Value;
                    }

                }

                reader.Close();

            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SETUP] Failed to find RPCN.yml \n {e.Message}");
            return null;
        }
        return null;

    }

    //setage 2 - 2
    public void CHECKCONFIGDEAMONWARE()
    {
        string config = getdemonwareconfig();
        Debug.Log("[SETUP] copy demonwareconfig.xml to the game");
        File.Copy(config, $"{userData.instance.LocalFilePath}/dev_hdd0/game/{getGameRegion()}/USRDIR/UPDATE/CONFIGS/CONFIGDEMONWARE.XML", true);
        CompleteStep(2, stage2MaxStep);
    }

    private string getdemonwareconfig()
    {
        string gotconfig = "NONE";

        var paths = StandaloneFileBrowser.OpenFilePanel("SELECT CONFIGDEMONWARE", "CONFIGDEMONWARE.XML", "XML", false);
        foreach(var path in paths)
        {
            
            if(path.ToLower().Contains("configdemonware.xml"))
            {
                gotconfig = path;
                Debug.Log("[SETUP] We Got config deamonware");
            }
        }

        if (gotconfig == "NONE")
        {
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = T.getText("STR_INVAIL_FILE");
            t.GetComponent<GUI_MessageBox>().message = T.getText("SETUP_STR_RELOADED_ACCOUNT_TIP");
            t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(CHECKCONFIGDEAMONWARE);
            return null;
        }
        else
        {
            return gotconfig;
        }
    }

    //stage 3 - 1
    public IEnumerator GETGHTVRELOADED()
    {
        //replace url with DL.updateURL;
        string url = "https://cdn.discordapp.com/attachments/1051918508159672330/1063447943178367067/GHTVReloadedUpdate.zip"; //placeholder
        stage3_UIscript.UpdateText(T.getText("NET_COM_PREP"));
        yield return new WaitForSeconds(2);
        GameObject a = Instantiate(DownloadPrefab);
        DownloadMod DL = a.GetComponent<DownloadMod>();
        DL.dissablePrompt = true;
        stage3_UIscript.UpdateText(T.getText("NET_COM_FETCH_GIT_DETAILS"));
        DL.btnDownload_Click(0, "https://raw.githubusercontent.com/Nathan31973/GHTV-Reloaded-Mods-Tools-Assets/main/GHL_PS3_LatestVersion.dat", Application.persistentDataPath + "/GHL_PS3_Version.dat");
        while(DL.finishfetchupdateDetails != "Game need to update")
        {
            yield return new WaitForEndOfFrame();
        }
        stage3_UIscript.UpdateText(T.getText("NET_COM_DOWNLOAD_START_GHTVR"));
        yield return new WaitForSeconds(2);
        DL.btnDownload_Click(1, DL.updateURL, Application.persistentDataPath + "/GHTV_Reloaded_update.zip");
        while(!DL.updateComplete)
        {
            stage3_UIscript.UpdateText(DL.message);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(2);
        stage3_UIscript.UpdateText(T.getText("NET_COM_DOWNLOAD_DONE"));
        yield return new WaitForSeconds(4);
        //controller setup 
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            CompleteStep(3, stage3MaxStep);
        }
        else
        {
            GoToFinish();
        }
    }

    //stage 3 - 2
    public void GoToStage4()
    {
        CompleteStep(3, stage3MaxStep);
    }

    public void GoToFinish()
    {
        foreach (GameObject go in Stage1UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage2UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage3UI)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in Stage4UI)
        {
            go.SetActive(false);
        }
        doneUI.SetActive(true);
        WelcomeScreen.SetActive(false);
        Debug.LogWarning("[SETUP] Finish setup going to finsh page.");
    }

    //stage 4 - 1
    public void IOSGuitarButton()
    {
        device = "ios";
        step = 2;
        CompleteStep(4, stage4MaxStep);
    }
    public void ConsoleGuitarButton()
    {
        step = 0;
        CompleteStep(4, stage4MaxStep);
    }

    //stage 4 - 2
    public void GOTOPlatform(string input)
    {
        device = input;
        if(input == "xbox360")
        {
            step = 1;
            CompleteStep(4, stage4MaxStep);
            return;
        }
        else if (input == "xboxone")
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                GameObject t = Instantiate(MessageBox);
                t.GetComponent<GUI_MessageBox>().title = T.getText("SETUP_ERROR_NO_TOOL");
                t.GetComponent<GUI_MessageBox>().message = $"{T.getText("SETUP_ERROR_NO_TOOL_DES")} {device} {T.getText("STR_GUITAR_DONGLE")}";
                t.GetComponent<GUI_MessageBox>().button.onClick.AddListener(GoToFinish);
                return;
            }
        }
        else
        {
            step = 2;
            CompleteStep(4, stage4MaxStep);
            return;
        }

    }

    //stage 4 - 3
    public void Openlink()
    {
        if(device.ToLower() == "ps4")
        {
            openURL("https://github.com/Octave13/GHLPokeMachine");
        }
        else if (device.ToLower() == "ps3" || device.ToLower() == "ios")
        {
            openURL("https://github.com/ghlre/GHLtarUtility");
        }
        else if (device.ToLower() == "xboxone")
        {
            openURL("https://github.com/paroj/xpad");
        }
        
    }
    public void Stage4Step4()
    {
        if(device.ToLower() == "ps4")
        {
            step = 4;
            CompleteStep(4, stage4MaxStep);
        }
        else
        {
            step = 3;
            CompleteStep(4, stage4MaxStep);
        }
    }

    //done
    public void GOTOMAINMENU()
    {
        userData.instance.hasDoneFirstTimeSetup = true;
        DataPersistenceManager.instance.SaveGame();
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }
}