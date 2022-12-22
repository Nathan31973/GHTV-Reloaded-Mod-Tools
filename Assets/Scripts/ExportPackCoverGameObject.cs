using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExportPackCoverGameObject : MonoBehaviour
{
    public string packName;
    public string Creator;
    public TextMeshProUGUI PackName;
    public RawImage PackRawImg;
    public Texture PackIcon;
    public Button button;
    public GemChanger_Importer host;
    public GameObject MessageBox;

    private void Start()
    {
        PackName.text = packName;
        button.onClick.AddListener(Click);
        if(PackIcon != null)
        {
            PackRawImg.texture = PackIcon;
        }
    }

    public void Click()
    {
        GameObject a = Instantiate(MessageBox);
        a.GetComponent<GUI_MessageBox>().title = $"Select {packName}";
        a.GetComponent<GUI_MessageBox>().message = $"Creator: {Creator}\n\nAre you Sure you want to import this pack?";
        a.GetComponent<MessageBox_GemChanger_Confrim_GUI>().yesButton.onClick.AddListener(Export);
        a.GetComponent<MessageBox_GemChanger_Confrim_GUI>().Cover.texture = PackRawImg.texture;
    }
    public void Export()
    {
        host.ExportPack(packName);
    }

}
