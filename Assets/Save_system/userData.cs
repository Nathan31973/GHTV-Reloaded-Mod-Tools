using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userData : MonoBehaviour, IDataPersistence
{
    public static userData instance { get; private set; }
    public string LocalFilePath;
    public bool hasDownloadedDefaultAtLeastOnce;
    public long hyperspeedLastDL;
    public bool hasDoneFirstTimeSetup;
    public bool AutoGameUpdate;
    public Version gameVersion;
    public string saveVersion;
    public bool ToolIntro;
    public Platform platform;

    [Header("WiiU details")]
    public WiiUVersion wiiUVersion;
    public string wiiULastUSB;
    public bool wiiUFTP;
    public string wiiuFtpIp;
    public string wiiuFtpPassword;

    [Header("ios / TV details")]
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

    private void Awake()
    {

        //check if a instance already exisit
        if (instance != null)
        {
            Debug.LogError("[userData] PlayerSave is alread loaded on " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void LoadData(GameData data)
    {
        gameVersion = (Version)data.gameVersion;
        AutoGameUpdate = data.AutoGameUpdate;
        hasDoneFirstTimeSetup = data.hasDoneFirstTimeSetup;
        LocalFilePath = data.LocalFilePath;
        hasDownloadedDefaultAtLeastOnce = data.hasDownloadedDefaultAtLeastOnce;
        hyperspeedLastDL = data.hyperspeedLastDL;
        saveVersion = data.saveVersion;
        ToolIntro = data.ToolIntro;
        platform = (Platform)data.platform;
        wiiUVersion = (WiiUVersion)data.wiiUVersion;
        wiiUFTP = data.wiiUFTP;
        wiiuFtpIp = data.wiiuFtpIp;
        wiiuFtpPassword = data.wiiuFtpPassword;
        wiiULastUSB = data.wiiULastUSB;

        SSHiosIP = data.SSHiosIP;
        SSHiosPassword = data.SSHiosPassword;
        gameTvosUUID = data.gameTvosUUID; //app UUID so we know where it located on TVOS
        gameIosUUID = data.gameIosUUID;  //app UUID so we know where it located on ios

    }
    public void SaveData(ref GameData data)
    {
        data.platform = (GameData.Platform)platform;
        data.gameVersion = (GameData.Version)gameVersion;
        data.hasDoneFirstTimeSetup = hasDoneFirstTimeSetup;
        data.LocalFilePath = LocalFilePath;
        data.hasDownloadedDefaultAtLeastOnce = hasDownloadedDefaultAtLeastOnce;
        data.hyperspeedLastDL = hyperspeedLastDL;
        data.AutoGameUpdate = AutoGameUpdate;
        data.saveVersion = saveVersion;
        data.ToolIntro = ToolIntro;
        data.wiiUVersion = (GameData.WiiUVersion)wiiUVersion;
        data.wiiuFtpPassword = wiiuFtpPassword;
        data.wiiuFtpIp = wiiuFtpIp;
        data.wiiUFTP = wiiUFTP;
        data.wiiULastUSB = wiiULastUSB;
        data.SSHiosIP = SSHiosIP;
        data.SSHiosPassword = SSHiosPassword;
        data.gameTvosUUID = gameTvosUUID; //app UUID so we know where it located on TVOS
        data.gameIosUUID = gameIosUUID;  //app UUID so we know where it located on ios
    }
}
