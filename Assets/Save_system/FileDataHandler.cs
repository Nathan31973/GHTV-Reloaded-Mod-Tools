using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string DataDirPath = "";
    private string DataFileName = "";

    private bool useEncrpytion = false;
    private readonly string encrpytionCodeword = "GHTVRELOADEDTOOLMADEBYNATHAN";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncrpytion)
    {
        DataDirPath = dataDirPath;
        DataFileName = dataFileName;
        this.useEncrpytion = useEncrpytion;
    }

    public GameData Load()
    {
        //using path.combine
        string fullpath = Path.Combine(DataDirPath, DataFileName);
        GameData loadedData = null;
        if(File.Exists(fullpath))
        {
            try
            {
                //load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullpath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (useEncrpytion)
                {
                    dataToLoad = EncrpytDecrypt(dataToLoad);
                }

                //deserialize the data from Json back to Game data
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                
            }
        }
        return loadedData;
    }
    public void Save(GameData data)
    {
        //using path.combine
        string fullpath = Path.Combine(DataDirPath, DataFileName);
        try
        {
            //create the directory the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

            //serialze the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(data, true);
            
            if(useEncrpytion)
            {
                dataToStore = EncrpytDecrypt(dataToStore);
            }

            //write the serialized data to the file
            using (FileStream stream = new FileStream(fullpath, FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
           

        }
    }

    //basic encpytion
    private string EncrpytDecrypt(string data)
    {
        string modifiedData = "";
        for(int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encrpytionCodeword[i % encrpytionCodeword.Length]);
        }
        return modifiedData;
    }
}
