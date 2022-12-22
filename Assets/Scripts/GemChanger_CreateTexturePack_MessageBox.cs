using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GemChanger_CreateTexturePack_MessageBox : MonoBehaviour
{
    public TMP_InputField texturePackName;
    public TMP_InputField creator;
    public GameObject MessageBox;
    public GemChanger host;

    public List<string> badWords = new List<string>();

    private void Start()
    {
        gameObject.GetComponent<GUI_MessageBox>().button.onClick.AddListener(Done);
    }
    public string getPackName()
    {
        return texturePackName.text;
    }
    public string getCreatorName()
    {
        return creator.text;
    }
    public void Done()
    {
        if(texturePackName.text == null || texturePackName.text == "" || creator.text == null || creator.text == "")
        {
            GameObject a = Instantiate(MessageBox);
            a.GetComponent<GUI_MessageBox>().title = "Bad Word Detective";
            a.GetComponent<GUI_MessageBox>().message = $"Sorry your not allow to use bad word in a pack name/creator";
            texturePackName.text = "";
            creator.text = "";
        }
        bool hasBadWord = false;
        foreach(string badWord in badWords)
        {
            if(texturePackName.text.ToLower().Contains(badWord))
            {
                hasBadWord = true;
            }
            if(creator.text.ToLower().Contains(badWord))
            {
                hasBadWord = true;
            }
        }
        if(hasBadWord)
        {
            GameObject a = Instantiate(MessageBox);
            a.GetComponent<GUI_MessageBox>().title = "Bad Word Detective";
            a.GetComponent<GUI_MessageBox>().message = $"Sorry your not allow to use bad word in a pack name/creator";
            texturePackName.text = "";
            creator.text = "";
        }
        else
        {
            host.premakeZipFile(getPackName(), getCreatorName());
        }
    }

}

