using UnityEngine;

public class WebGLRuntimeOptimizer : MonoBehaviour
{
    [SerializeField] int targetFrameRate = 30;
    [SerializeField] int webGLQualityLevel = 1;
    [SerializeField] ThreadPriority backgroundLoadingPriority = ThreadPriority.Low;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureInstance()
    {
        if (FindObjectOfType<WebGLRuntimeOptimizer>() != null)
            return;

        GameObject root = new GameObject("WebGLRuntimeOptimizer");
        DontDestroyOnLoad(root);
        root.AddComponent<WebGLRuntimeOptimizer>();
    }

    void Awake()
    {
        Application.backgroundLoadingPriority = backgroundLoadingPriority;
        Application.targetFrameRate = Mathf.Max(15, targetFrameRate);

#if UNITY_WEBGL && !UNITY_EDITOR
        int quality = Mathf.Clamp(webGLQualityLevel, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(quality, true);
#endif
    }
}
