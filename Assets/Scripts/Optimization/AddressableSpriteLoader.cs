using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddressableSpriteLoader : MonoBehaviour
{
    [SerializeField] string spriteAddress;
    [SerializeField] Image targetImage;
    [SerializeField] RawImage targetRawImage;

    AsyncOperationHandle<Sprite>? spriteHandle;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetRawImage == null)
            targetRawImage = GetComponent<RawImage>();
    }

    void OnEnable()
    {
        if (!string.IsNullOrWhiteSpace(spriteAddress))
            Load(spriteAddress);
    }

    public void Load(string address)
    {
        Release();
        spriteAddress = address;

        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        spriteHandle = handle;
        handle.Completed += OnSpriteLoaded;
    }

    void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
        {
            Debug.LogWarning($"No se pudo cargar el sprite Addressable: {spriteAddress}", this);
            return;
        }

        if (targetImage != null)
            targetImage.sprite = handle.Result;

        if (targetRawImage != null)
            targetRawImage.texture = handle.Result.texture;
    }

    void OnDisable()
    {
        Release();
    }

    void Release()
    {
        if (!spriteHandle.HasValue)
            return;

        Addressables.Release(spriteHandle.Value);
        spriteHandle = null;
    }
}
