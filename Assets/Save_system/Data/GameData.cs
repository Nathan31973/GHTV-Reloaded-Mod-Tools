
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string LocalFilePath;
    public bool hasDownloadedDefaultAtLeastOnce;
    public long hyperspeedLastDL;
    public bool hasDoneFirstTimeSetup;
    public bool AutoGameUpdate;
    public Version gameVersion;
    public string saveVersion;
    public bool ToolIntro;
    public enum Version
    {
        None,
        PAL,
        USA,
        Lite
    }
    public GameData()
    {
        this.gameVersion = Version.None;
        this.LocalFilePath = $"TEST";
        this.hasDownloadedDefaultAtLeastOnce = false;
        this.hyperspeedLastDL = 0;
        this.hasDoneFirstTimeSetup = false;
        this.AutoGameUpdate = false;
        this.saveVersion = "V1.3";
        this.ToolIntro = false; //bool to auto run the setup level
    }
}
