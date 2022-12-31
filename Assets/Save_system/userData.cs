using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userData : MonoBehaviour, IDataPersistence
{
    public static userData instance { get; private set; }
    public string LocalFilePath;
    public bool hasDownloadedDefaultAtLeastOnce;
    public long hyperspeedLastDL;

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
        this.LocalFilePath = data.LocalFilePath;
        this.hasDownloadedDefaultAtLeastOnce = data.hasDownloadedDefaultAtLeastOnce;
        this.hyperspeedLastDL = data.hyperspeedLastDL;
    }
    public void SaveData(ref GameData data)
    {
        data.LocalFilePath = this.LocalFilePath;
        data.hasDownloadedDefaultAtLeastOnce = this.hasDownloadedDefaultAtLeastOnce;
        data.hyperspeedLastDL = this.hyperspeedLastDL;
    }
}
