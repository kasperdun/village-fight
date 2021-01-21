using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    // Loading Progress: private setter, public getter
    private float _loadingProgress;
    public float LoadingProgress { get { return _loadingProgress; } }

    // guarantee this will be always a singleton only - can't use the constructor!
    protected SceneLoader() { }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneAsync(string sceneName)
    {
        // kick-off the one co-routine to rule them all
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    public void UnloadScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        var asyncScene = SceneManager.LoadSceneAsync(sceneName);
 
        // this value stops the scene from displaying when it's finished loading
        asyncScene.allowSceneActivation = false;

        while (!asyncScene.isDone)
        {
            // loading bar progress
            _loadingProgress = Mathf.Clamp01(asyncScene.progress / 0.9f) * 100;

            // scene has loaded as much as possible, the last 10% can't be multi-threaded
            if (asyncScene.progress >= 0.9f)
            {
                // we finally show the scene
                asyncScene.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}