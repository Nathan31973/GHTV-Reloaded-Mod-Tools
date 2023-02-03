using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExportPackCoverGameObject : MonoBehaviour
{
    public string packName;
    public string Creator;
    public string version;
    public TextMeshProUGUI PackName;
    public RawImage PackRawImg;
    public Texture PackIcon;
    public Button button;
    public GemChanger_Importer host;
    public GameObject MessageBox;
    public GameObject MessageBox2;
    private Translater T;
    private void Start()
    {
        T = Translater.instance;
        PackName.text = packName;
        button.onClick.AddListener(Click);
        if(PackIcon != null)
        {
            PackRawImg.texture = PackIcon;
        }
    }

    private bool versioncheck()
    {
        string[] pastversion = { "1.3", "1.3.01", "1.3.02", "1.3.1" };
        foreach(string ver in pastversion)
        {
            if(version == ver)
            {
                return true;
            }
        }
        return false;
    }
    public void Click()
    {
        if(!versioncheck())
        {
            GameObject a = Instantiate(MessageBox2);
            a.GetComponent<GUI_MessageBox>().title = $"{T.getText("GEM_IMPORTER_PACKOUTDATED")}";
            a.GetComponent<GUI_MessageBox>().message = $"{packName} {T.getText("GEM_IMPORTER_PACKOUTDATED_DES")}";
            a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(Click2);
        }
        else
        {
            Click2();
        }

    }
    public void Click2()
    {
        GameObject a = Instantiate(MessageBox);
        a.GetComponent<GUI_MessageBox>().title = $"{T.getText("STR_SELECT")} {packName}";
        a.GetComponent<GUI_MessageBox>().message = $"{T.getText("STR_CREATOR")} {Creator}\n\n{T.getText("GEM_IMPORTER_CONFIRM")}";
        a.GetComponent<MessageBox_GemChanger_Confrim_GUI>().yesButton.onClick.AddListener(Export);
        a.GetComponent<MessageBox_GemChanger_Confrim_GUI>().Cover.texture = PackRawImg.texture;
    }
    public void Export()
    {
        host.ExportPack(packName);
    }

}
