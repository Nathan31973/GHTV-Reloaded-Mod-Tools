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
    public enum Version
    {
        None,
        PAL,
        USA,
        Lite
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
        this.gameVersion = (Version)data.gameVersion;
        this.AutoGameUpdate = data.AutoGameUpdate;
        this.hasDoneFirstTimeSetup = data.hasDoneFirstTimeSetup;
        this.LocalFilePath = data.LocalFilePath;
        this.hasDownloadedDefaultAtLeastOnce = data.hasDownloadedDefaultAtLeastOnce;
        this.hyperspeedLastDL = data.hyperspeedLastDL;
        this.saveVersion = data.saveVersion;
        this.ToolIntro = data.ToolIntro;
    }
    public void SaveData(ref GameData data)
    {
        data.gameVersion = (GameData.Version)this.gameVersion;
        data.hasDoneFirstTimeSetup = this.hasDoneFirstTimeSetup;
        data.LocalFilePath = this.LocalFilePath;
        data.hasDownloadedDefaultAtLeastOnce = this.hasDownloadedDefaultAtLeastOnce;
        data.hyperspeedLastDL = this.hyperspeedLastDL;
        data.AutoGameUpdate = this.AutoGameUpdate;
        data.saveVersion = this.saveVersion;
        data.ToolIntro = this.ToolIntro;
    }
}
