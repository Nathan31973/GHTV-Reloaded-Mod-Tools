using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
public class MainSetting : MonoBehaviour
{
    private GUI_MessageBox host;
    public GameObject loadingBox;
    public GameObject MessageBox;
    public TextMeshProUGUI version;
    public GameObject Credits;

    private void Start()
    {
        version.text = $"V{Application.version}";
        host = gameObject.GetComponent<GUI_MessageBox>();
    }

    public void Back()
    {
        host.CloseAnim();
    }
    public void SeleteRPCS3()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR RPCS3 FOLDER", "RPSC3 FOLDER", false);
        foreach (var path in paths)
        {
            if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
            {
                Debug.Log($"{path} Is valid");
                userData.instance.LocalFilePath = path;
            }
            else
            {
                GameObject t = Instantiate(MessageBox);
                t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
                t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
            }
        }
    }
    public void ClearCache()
    {
        StartCoroutine(Clearing());
    }
    private IEnumerator Clearing()
    {
        GameObject load = Instantiate(loadingBox);
        load.GetComponent<GUI_MessageBox>().title = "Deleting Cache Files";
        load.GetComponent<GUI_MessageBox>().message = "Please wait while remove cache files";
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
        if (Directory.Exists($"{Application.persistentDataPath}/GemMaker"))
        {
            Directory.Delete($"{Application.persistentDataPath}/GemMaker", true);
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
        load.GetComponent<GUI_MessageBox>().CloseAnim();
        yield return new WaitForSeconds(1);
    }
    public void DeleteSave()
    {
        File.Delete($"{Application.persistentDataPath}/save.game");
        Debug.Log(Application.platform);
        userData.instance.LocalFilePath = $"TEST";
        DataPersistenceManager.instance.freshStart();
    }
    public void OPENCREDITS()
    {
        Instantiate(Credits);
    }
}

