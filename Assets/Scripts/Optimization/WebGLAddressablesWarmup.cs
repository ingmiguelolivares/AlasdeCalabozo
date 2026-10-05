using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WebGLAddressablesWarmup : MonoBehaviour
{
    [SerializeField] string[] labelsToDownloadAfterStartup;
    [SerializeField] float startupDelaySeconds = 1f;

    IEnumerator Start()
    {
        if (labelsToDownloadAfterStartup == null || labelsToDownloadAfterStartup.Length == 0)
            yield break;

        yield return new WaitForSeconds(startupDelaySeconds);

        foreach (string label in labelsToDownloadAfterStartup)
        {
            if (string.IsNullOrWhiteSpace(label))
                continue;

            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(label);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
                Debug.LogWarning($"No se pudieron precargar dependencias Addressables para la etiqueta '{label}'.", this);

            Addressables.Release(handle);
        }
    }
}
