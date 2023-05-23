
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
    public Platform platform;
    public WiiUVersion wiiUVersion;
    public bool wiiUFTP;
    public string wiiuFtpIp;
    public string wiiuFtpPassword;
    public string wiiULastUSB;
    public string SSHiosIP;
    public string SSHiosPassword;
    public string gameTvosUUID; //app UUID so we know where it located on TVOS
    public string gameIosUUID;  //app UUID so we know where it located on ios
    public enum Platform
    {
        Rpcs3,
        WiiU,
        PS3,
        Ios,
        TVOS
    }
    public enum Version
    {
        None,
        PAL,
        USA,
        Lite
    }
    public enum WiiUVersion
    {
        None,
        PAL,
        USA,
    }
    public GameData()
    {
        wiiUVersion = WiiUVersion.None;
        platform = Platform.Rpcs3;
        gameVersion = Version.None;
        LocalFilePath = $"TEST";
        hasDownloadedDefaultAtLeastOnce = false;
        hyperspeedLastDL = 0;
        hasDoneFirstTimeSetup = false;
        AutoGameUpdate = false;
        saveVersion = "V1.3.2";
        ToolIntro = false; //bool to auto run the setup level

        wiiUFTP = false;
        wiiuFtpIp = "0.0.0.0";
        wiiuFtpPassword = "admin";
        wiiULastUSB = "TEST";
        SSHiosIP = "0.0.0.0";
        SSHiosPassword = "alpine"; //default ios root password
        gameTvosUUID = "TEST"; //app UUID so we know where it located on TVOS
        gameIosUUID = "TEST";  //app UUID so we know where it located on ios
}
}
