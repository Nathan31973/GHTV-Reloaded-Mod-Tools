using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GHTVAccountCreator : MonoBehaviour
{
    public TextMeshProUGUI Username;
    public Setup main;
    public int unlocktype = 0;
    public string command;
    // Start is called before the first frame update
    void Start()
    {
        Username.text = main.username;
        command = $"!reg {main.username} {unlocktype} Sony";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ClipClassicCommand()
    {
        unlocktype = 0;
        command = $"!reg {main.username} {unlocktype} Sony";
        command.CopyToClipboard();
    }
    public void ClipUnlockAllCommand()
    {
        unlocktype = 1;
        command = $"!reg {main.username} {unlocktype} Sony";
        command.CopyToClipboard();
    }
}
public static class ClipboardExtension
{
    /// <summary>
    /// Puts the string into the Clipboard.
    /// </summary>
    public static void CopyToClipboard(this string str)
    {
        GUIUtility.systemCopyBuffer = str;
    }
}
