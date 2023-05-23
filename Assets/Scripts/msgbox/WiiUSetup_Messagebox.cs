using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WiiUSetup_Messagebox : MonoBehaviour
{
    public GameObject MessageBox;
    [HideInInspector]public GemChanger_Importer Master;
    public Translater T;
    [HideInInspector]public string pack;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void USB()
    {
        
        //make the user find the wiiU usb
        var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR WIIU USB", "WIIU USB", false);
        foreach (var path in paths)
        {
            if (Directory.Exists($"{path}/usr/title/0005000e/101bc600") | Directory.Exists($"{path}/usr/title/0005000e/101ba400"))
            {
               
                if(Directory.Exists($"{path}/usr/title/0005000e/101bc600"))
                {
                    userData.instance.wiiUVersion = userData.WiiUVersion.PAL;
                }
                else if(Directory.Exists($"{path}/usr/title/0005000e/101ba400"))
                {
                    userData.instance.wiiUVersion = userData.WiiUVersion.USA;
                }
                userData.instance.wiiULastUSB = path;
                Debug.Log($"[GemChangerImporter] {path} Is valid");
                Master.ExportWiiU(pack);
            }
            else
            {
                GameObject t = Instantiate(MessageBox);
                userData.instance.wiiULastUSB = "TEST";
                t.GetComponent<GUI_MessageBox>().title = T.getText("ERROR_INVAL_PATH");
                t.GetComponent<GUI_MessageBox>().message = T.getText("STR_WIIU_WRONG_FOLDER");
                gameObject.GetComponent<GUI_MessageBox>().CloseAnim();
            }
        }
    }
}
