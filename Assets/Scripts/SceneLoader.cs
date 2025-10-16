using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string onMouseDownSceneLoad;
    //public bool preloadCanLoad;
    public GameObject loadingPanel;
    
    /// <summary>
    /// Loads the scene with the name passed as parameter.
    /// </summary>
    /// <param name="sceneName">Name of the scene to be loaded.</param>
    public static void LoadScene(string sceneName){
        SceneManager.LoadScene(sceneName);
    }

    public void PreloadScene(string sceneName){
        //preloadCanLoad = false;
        GetComponent<SetupScene>().SetConfig();
        GameConfig.SetDifficultyParameters();
        loadingPanel.SetActive(true);
        StartCoroutine(PreloadSceneAsync(sceneName));
    }
    IEnumerator PreloadSceneAsync(string sceneName){
        yield return null;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                //if (preloadCanLoad == true)
                    asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    void OnMouseDown(){
        LoadScene(onMouseDownSceneLoad);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void OnExitClicked(){
        Application.Quit();
    }
}
