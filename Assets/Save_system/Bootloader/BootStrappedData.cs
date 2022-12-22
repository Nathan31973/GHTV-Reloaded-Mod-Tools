using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PerformBootstrp
{
    const string SceneName = "BOOTLOADER";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        //traverse the currently loaded scenes
        for(int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
        {
            var scene = SceneManager.GetSceneAt(sceneIndex);

            //early out if already loaded
            if(scene.name == SceneName)
            {
                return;
            }
        }
        //load the scene but it additive
        SceneManager.LoadScene(SceneName, LoadSceneMode.Additive);
    }

}

public class BootStrappedData : MonoBehaviour
{
    public static BootStrappedData Instance { get; private set; } = null;
    private void Awake()
    {

        //check if a instance already exisit
        if(Instance != null)
        {
            Debug.LogError("BootStrapped is alread loaded on " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        //stop the data from being lost when unloaded
        DontDestroyOnLoad(gameObject);
    }
}
