using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Stage3_UI : MonoBehaviour
{
    public TextMeshProUGUI status;
    public Setup main;
    private Translater T;

    private void Start()
    {
        T = Translater.instance;
        try
        {
            main.stage3_UIscript = this;
            StartCoroutine(main.GETGHTVRELOADED());
        }
        catch
        {
            Debug.LogError($"[Stage3_UI] Failed to grab main. Make sure {gameObject.name} has SETUP referance");
        }
    }
    public void UpdateText(string str)
    {
        status.text = $"{T.getText("STR_LOADING_STATUS")} "+ str;
    }
}
