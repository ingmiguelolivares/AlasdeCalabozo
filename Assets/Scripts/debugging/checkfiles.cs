using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;

public class CartasLoader : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    IEnumerator Start()
    {
        yield return ImprimirCartasEnCarpeta();
    }

    IEnumerator ImprimirCartasEnCarpeta()
    {
        AsyncOperationHandle<IList<Sprite>> handle = Addressables.LoadAssetsAsync<Sprite>("cards", null);
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null || handle.Result.Count == 0)
        {
            Debug.LogWarning("No se encontraron cartas Addressables con la etiqueta 'cards'.");
            Addressables.Release(handle);
            yield break;
        }

        foreach (Sprite carta in handle.Result)
        {
            Debug.Log("🃏 Carta encontrada: " + carta.name);
        }

        Addressables.Release(handle);
    }
#endif
}
