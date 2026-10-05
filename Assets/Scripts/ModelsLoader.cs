using UnityEngine;
using Photon.Pun;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ModelsLoader : MonoBehaviourPun
{
    public static ModelsLoader Instance;

    [Header("Modelos de enemigos por dificultad")]
    public GameObject[] easyModels;
    public GameObject[] mediumModels;
    public GameObject[] hardModels;

    [Header("Calabozos por dificultad")]
    public GameObject easyDungeon;
    public GameObject mediumDungeon;
    public GameObject hardDungeon;

    [Header("Punto de aparición")]
    public Transform spawnPoint;

    [Header("Addressables")]
    [SerializeField] bool loadAddressableModelsWhenArraysEmpty = true;
    [SerializeField] string easyModelsLabel = "monsters_easy";
    [SerializeField] string mediumModelsLabel = "monsters_medium";
    [SerializeField] string hardModelsLabel = "monsters_hard";

    readonly Dictionary<string, GameObject> easyLookup = new();
    readonly Dictionary<string, GameObject> mediumLookup = new();
    readonly Dictionary<string, GameObject> hardLookup = new();
    bool loadStarted;

    public bool IsReady { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    IEnumerator Start()
    {
        yield return EnsureModelsLoaded();
    }

    public IEnumerator EnsureModelsLoaded()
    {
        if (IsReady)
            yield break;

        if (loadStarted)
        {
            while (!IsReady)
                yield return null;
            yield break;
        }

        loadStarted = true;

        if (loadAddressableModelsWhenArraysEmpty)
        {
            if (easyModels == null || easyModels.Length == 0)
                yield return LoadAddressableModels(easyModelsLabel, result => easyModels = result);

            if (mediumModels == null || mediumModels.Length == 0)
                yield return LoadAddressableModels(mediumModelsLabel, result => mediumModels = result);

            if (hardModels == null || hardModels.Length == 0)
                yield return LoadAddressableModels(hardModelsLabel, result => hardModels = result);
        }

        if (easyModels == null || easyModels.Length == 0)
            easyModels = Resources.LoadAll<GameObject>("ModelosBichitos/Easy")
                                   .OrderBy(m => m.name).ToArray();

        if (mediumModels == null || mediumModels.Length == 0)
            mediumModels = Resources.LoadAll<GameObject>("ModelosBichitos/Medium")
                                     .OrderBy(m => m.name).ToArray();

        if (hardModels == null || hardModels.Length == 0)
            hardModels = Resources.LoadAll<GameObject>("ModelosBichitos/Hard")
                                    .OrderBy(m => m.name).ToArray();

        BuildLookup(easyModels, easyLookup);
        BuildLookup(mediumModels, mediumLookup);
        BuildLookup(hardModels, hardLookup);
        IsReady = true;
    }

    static IEnumerator LoadAddressableModels(string label, System.Action<GameObject[]> assign)
    {
        if (string.IsNullOrWhiteSpace(label))
            yield break;

        AsyncOperationHandle<IList<GameObject>> handle = Addressables.LoadAssetsAsync<GameObject>(label, null);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null && handle.Result.Count > 0)
        {
            assign(handle.Result.Where(model => model != null).OrderBy(model => model.name).ToArray());
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"No se encontraron modelos Addressables con la etiqueta '{label}'.");
#endif
        }
    }

    static void BuildLookup(GameObject[] models, Dictionary<string, GameObject> lookup)
    {
        lookup.Clear();
        if (models == null) return;

        foreach (var model in models)
        {
            if (model == null) continue;
            lookup[model.name] = model;
        }
    }

    [PunRPC]
    public void SpawnMonsterByName(int difficulty, string prefabName)
    {
        Dictionary<string, GameObject> lookup = difficulty switch
        {
            0 => easyLookup,
            1 => mediumLookup,
            2 => hardLookup,
            _ => null
        };

        if (lookup == null)
        {
            Debug.LogError("❌ Dificultad fuera de rango en SpawnMonsterByName");
            return;
        }

        if (!lookup.TryGetValue(prefabName, out var prefab) || prefab == null)
        {
            Debug.LogError($"❌ Prefab '{prefabName}' no encontrado en cliente.");
            return;
        }

        // Destruir hijos actuales en spawnPoint antes de crear nuevo
        foreach (Transform child in spawnPoint)
            Destroy(child.gameObject);
        // print(prefab.transform.rotation);
        // Instanciar el prefab con rotación 180 en Y para que mire de frente
        //GameObject monster = Instantiate(prefab, spawnPoint.position, Quaternion.Euler(0, 180, 0), spawnPoint);
        // Obtener la rotación original del prefab
        UnityEngine.Quaternion rotacionOriginal = prefab.transform.rotation;

        // Crear una rotación de 180 grados en Y
        UnityEngine.Quaternion rotacionExtra = UnityEngine.Quaternion.Euler(0, 180f, 0);

        // Combinar la rotación original con la adicional
        UnityEngine.Quaternion nuevaRotacion = rotacionOriginal * rotacionExtra;
        UnityEngine.Vector3 ajuste = new UnityEngine.Vector3(0, -1.341177f, 8.411765f);

        var spawnPos = (spawnPoint != null ? spawnPoint.position : transform.position) + ajuste;
        GameObject monster = Instantiate(prefab, spawnPos, nuevaRotacion, spawnPoint);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("posicion inicial" + (spawnPoint != null ? spawnPoint.position : transform.position) + " position [pre] " + prefab.transform.position);
#endif

        // Reproducir sonido si el prefab tiene MonsterSound
        MonsterSound ms = monster.GetComponent<MonsterSound>();
        if (ms != null)
        {
            ms.PlaySound();
        }

        // Activar calabozo correspondiente
        ActivarCalabozoPorDificultad(difficulty);
    }

    private void ActivarCalabozoPorDificultad(int dificultad)
    {
        if (easyDungeon) easyDungeon.SetActive(dificultad == 0);
        if (mediumDungeon) mediumDungeon.SetActive(dificultad == 1);
        if (hardDungeon) hardDungeon.SetActive(dificultad == 2);
    }
}
