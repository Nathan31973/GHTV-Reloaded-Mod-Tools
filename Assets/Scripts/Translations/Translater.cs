using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using UnityEngine;

public class Translater : MonoBehaviour
{
    public string lang;

    [Header("external translations files")]
    public bool hasdownloaded = false;
    public XDocument xmlFile = null;

    private bool hasgrab = false;
    private WebClient webClient = null;
    [SerializeField]private bool uselocal = false;
    public static Translater instance { get; private set; }
    private void Awake()
    {
        Debug.Log($"[SYSTEM] Platform is {Application.platform}");
        lang = Application.systemLanguage.ToString();
        //check if a instance already exisit
        if (instance != null)
        {
            Debug.LogError("Translater is alread loaded on " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        instance = this;
        if (!hasgrab)
        {
            string path = $"{Application.streamingAssetsPath}/TRANSLATIONS/{lang}.xml"; //default path


            //check if we have downloaded external translations files
            if(!Directory.Exists($"{Application.persistentDataPath}/translations"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/translations");
            }
            if (!uselocal)
            {
                btnDownload_Click($"https://raw.githubusercontent.com/Nathan31973/GHTV-Reloaded-Mods-Tools-Assets/main/TRANSLATIONS/{lang}.xml", $"{Application.persistentDataPath}/translations/{lang}.xml");
            }
            else
            {
                Debug.LogWarning("[Translater] ONLY USING LOCAL TRANSLATIONS FILES");
            }
            //Check if the lang file exist
            if (!File.Exists(path))
            {
                path = $"{Application.streamingAssetsPath}/TRANSLATIONS/English.xml"; //which to eng if other lang doesn't existed
            }
            Debug.LogWarning("[Translater] Using translation from " + path);
            //reading the file
            xmlFile = XDocument.Load(path);
            hasgrab = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("[Translate] System lang = " + Application.systemLanguage);  
    }

    public string getText(string key)
    {
        string translatedtext = GetTextFromXML(key);
        //read from xml


        if(translatedtext != "")
        {
            return translatedtext;
        }
        else
        {
            Debug.LogError($"[Translate] KEY: '{key}' String not found in translation files.");
            return key;
        }
    }


    private string GetTextFromXML(string key)
    {
        if (!hasgrab || hasdownloaded)
        {
            string path = $"{Application.streamingAssetsPath}/TRANSLATIONS/{lang}.xml"; //default path


            //check if we have downloaded external translations files
            if(hasdownloaded)
            {
                path = $"{Application.persistentDataPath}/translations/{lang}.xml";
            }

            //Check if our path exist else we switch to ENG as default
            if (!File.Exists(path))
            {
                Debug.Log($"[Translater] no {lang} translation exist, switch to English");
                path = $"{Application.streamingAssetsPath}/TRANSLATIONS/English.xml"; //which to eng if other lang doesn't existed
            }
            Debug.LogWarning("[Translater] Using translation from "+ path);
            //reading the file
            xmlFile = XDocument.Load(path);
            hasgrab = true;
            hasdownloaded = false;
        }
        var dif = from c in xmlFile.Elements("Translation").Elements("Translation")
                  select c;

        foreach (XElement c in dif)
        {
            if(c.Attribute("key").Value == key)
            {
                return c.Element("Language").Attribute("value").Value;
            }
        }
        return "";
    }
    private void btnDownload_Click(string url, string filename)
    {
        //checking if we have internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[Translater] Error. Check internet connection!");
            return;
        }
        else
        {
            Debug.Log("[Translater] We have internet");
            // Is file downloading yet?
            if (webClient != null)
            {
                return;
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            webClient = new WebClient();

            // Create a new HttpWebRequest Object to the mentioned URL.
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            myHttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";

            try
            {
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedConver);
                webClient.DownloadFileAsync(new Uri($"{url}"), filename);
            }
            catch (Exception e)
            {
                if (e.Message == "The remote server returned an error: (404) Not Found.")
                {
                    Debug.Log($"[Translater] Server doesn't have {lang} translation");
                }
            }



        }
    }
    private void CompletedConver(object sender, AsyncCompletedEventArgs e)
    {
        webClient = null;
        Debug.Log("[Translater] Download completed!");
        hasdownloaded = true;
    }

}

