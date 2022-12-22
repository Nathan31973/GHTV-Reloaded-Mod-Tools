using Octokit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gitHubManager : MonoBehaviour
{
    private GitHubClient client = new GitHubClient(new ProductHeaderValue("GHTV-Reloaded-Mod-Tools"));
    public static gitHubManager instance { get; private set; }
    public GameObject Messagebox;

    private void Awake()
    {

        //check if a instance already exisit
        if (instance != null)
        {
            Debug.LogError("GitHubManger is alread loaded on " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    // Start is called before the first frame update
    public async void GetVersion()
    {
        var releases = client.Repository.Release.GetAll("nathan31973", "GHTV-Reloaded-Mod-Tools");
        var latest = (await releases)[0];
        var test =(
            "The latest release is tagged at {0} and is named {1}",
            latest.TagName,
            latest.Name);
        var version = "V" + UnityEngine.Application.version;
        if (!version.Contains(" - Beta"))
        {
            if (version != latest.TagName)
            {
                Debug.LogWarning("[System] App is out of date, Please Update the app");
                GameObject a = Instantiate(Messagebox);
                GUI_MessageBox b = a.GetComponent<GUI_MessageBox>();
                b.title = "Outdated App";
                b.message = $"A new version of the GHTV Reloaded: Mod Tools is available\n\nLatest Version: {latest.TagName}\nApp Version: V{UnityEngine.Application.version}";
                b.button.onClick.AddListener(OpenLatestVersion);
            }
            else
            {
                Debug.LogWarning("[System] App is up to date");
            }
        }
    }
    public void OpenLatestVersion()
    {
        UnityEngine.Application.OpenURL("https://github.com/Nathan31973/GHTV-Reloaded-Mod-Tools/releases/latest");
    }

}
