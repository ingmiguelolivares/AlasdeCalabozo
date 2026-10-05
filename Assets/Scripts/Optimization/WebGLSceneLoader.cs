using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WebGLSceneLoader
{
    public static void Load(MonoBehaviour owner, string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (owner != null && owner.isActiveAndEnabled)
        {
            owner.StartCoroutine(LoadRoutine(sceneName, mode));
            return;
        }

        SceneManager.LoadScene(sceneName, mode);
    }

    public static IEnumerator LoadRoutine(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
        if (asyncLoad == null)
        {
            yield break;
        }

        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
