using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string LocalFilePath;
    public bool hasDownloadedDefaultAtLeastOnce;
    public GameData()
    {
        this.LocalFilePath = $"TEST";
        this.hasDownloadedDefaultAtLeastOnce = false;
    }
}
