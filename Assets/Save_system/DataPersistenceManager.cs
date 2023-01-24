using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private string fileLocation;
    [SerializeField] private bool useEncrpytion;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public static DataPersistenceManager instance { get; private set; }

    private void Awake()
    {
        if(instance !=null)
        {
            Debug.LogError("Found more than 1 DataPersistenceManger in the scene. " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }
    public void LoadGame()
    {
        //load save data from data handler
        this.gameData = dataHandler.Load();

        //if no data is found we make a new save
        if(this.gameData == null)
        {
            Debug.LogWarning("No saveData found creating new save");
            NewGame();
        }
        //push the loaded data to all scripts that need it
        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
        Debug.LogWarning("LOADING SAVE");
    }

    public void SaveGame()
    {
        //pass the data to other scripts
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }
        Debug.LogWarning("Saving Game!!!!");
        //save the data to the data handler
        dataHandler.Save(gameData);
    }

    //change this for when the start button is press. for now it load on app bootup
    private void Start()
    {
        
        //if filelocation of the save file is null we use unity default location
        if(fileLocation == null || fileLocation == "")
        {
            fileLocation = Application.persistentDataPath;
        }
        this.dataHandler = new FileDataHandler(fileLocation, fileName, useEncrpytion);

        //find all objects that have IdataPersistence
        this.dataPersistenceObjects = FindAllDataPresistenceObjects();
        LoadGame();
    }

    public void freshStart()
    {
        //if filelocation of the save file is null we use unity default location
        if (fileLocation == null || fileLocation == "")
        {
            fileLocation = Application.persistentDataPath;
        }
        this.dataHandler = new FileDataHandler(fileLocation, fileName, useEncrpytion);

        //find all objects that have IdataPersistence
        this.dataPersistenceObjects = FindAllDataPresistenceObjects();
        LoadGame();
    }
    //auto save when quitting the app
    private void OnApplicationQuit()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "Setup")
        {
            SaveGame();
        }
    }
    private List<IDataPersistence> FindAllDataPresistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
