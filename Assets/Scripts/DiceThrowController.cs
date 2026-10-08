using System.Collections;
using UnityEngine;

public class DiceThrowController : MonoBehaviour
{
    const int ImpactEffectSlotCount = 15;

    [System.Serializable]
    public class ImpactEffectSlot
    {
        public GameObject prefab;
        public Vector3 scale = Vector3.one;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public float destroyAfterSeconds = 3f;
    }

    [Header("Trayectoria")]
    [SerializeField] Camera throwCamera;
    [SerializeField] Vector2 screenStartViewport = new Vector2(0.5f, 0.14f);
    [SerializeField, Range(0.1f, 0.9f)] float startDepthFactor = 0.35f;
    [SerializeField] float minimumStartDepth = 2f;
    [SerializeField] float arcHeight = 1.25f;
    [SerializeField] float throwScaleMultiplier = 1f;
    [SerializeField, Range(0.03f, 0.35f)] float viewportHeightFraction = 0.14f;
    [SerializeField] float minimumWorldSize = 0.35f;
    [SerializeField] float maximumWorldSize = 2.2f;
    [SerializeField] int projectileLayer = 0;

    [Header("Efectos de impacto")]
    [SerializeField] ImpactEffectSlot[] impactEffects = new ImpactEffectSlot[ImpactEffectSlotCount];
    [SerializeField] bool randomizeImpactEffect = true;
    [SerializeField, Range(0, ImpactEffectSlotCount - 1)] int selectedImpactEffectIndex;
    [SerializeField, Range(0f, 1f)] float targetImpactHeight = 0.28f;
    [SerializeField] float globalImpactVerticalOffset;
    [SerializeField] bool parentEffectToTarget;

    void OnValidate()
    {
        EnsureImpactEffectSlots();
    }

    public IEnumerator Throw(Transform die, Transform target, float seconds, float spinSpeed)
    {
        EnsureImpactEffectSlots();

        if (die == null)
            yield break;

        Camera cameraToUse = GetThrowCamera();
        GameObject projectile = Instantiate(die.gameObject);
        projectile.name = die.name + "_ThrowPreview";
        projectile.transform.SetParent(null, true);
        projectile.SetActive(true);
        SetLayerRecursive(projectile.transform, projectileLayer);
        SetRenderersVisible(projectile, true);
        DisableProjectileAudio(projectile);

        Vector3 startPos = GetScreenStartPosition(cameraToUse, target, die);
        Quaternion startRot = die.rotation;
        Vector3 endPos = GetTargetPoint(target, startPos, cameraToUse);
        Vector3 effectPoint = GetTargetImpactEffectPoint(target, endPos);
        projectile.transform.position = startPos;
        projectile.transform.rotation = startRot;
        projectile.transform.localScale = die.lossyScale * Mathf.Max(0.01f, throwScaleMultiplier);
        FitProjectileScale(projectile.transform, cameraToUse, startPos);
        seconds = Mathf.Max(0.05f, seconds);

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
            projectile.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * arc;
            projectile.transform.Rotate(Vector3.one, spinSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }

        projectile.transform.position = endPos;
        projectile.transform.rotation = Random.rotation;
        SpawnImpactEffect(effectPoint, target, cameraToUse);
        yield return new WaitForSeconds(0.15f);
        Destroy(projectile);
    }

    public void SetImpactEffectIndex(int index)
    {
        selectedImpactEffectIndex = Mathf.Clamp(index, 0, ImpactEffectSlotCount - 1);
        randomizeImpactEffect = false;
    }

    public void SetRandomImpactEffects(bool random)
    {
        randomizeImpactEffect = random;
    }

    Vector3 GetScreenStartPosition(Camera cameraToUse, Transform target, Transform fallback)
    {
        if (cameraToUse == null)
            return fallback.position;

        Vector3 targetPoint = GetTargetPoint(target, fallback.position, cameraToUse);
        Vector3 viewportTarget = cameraToUse.WorldToViewportPoint(targetPoint);
        float targetDepth = viewportTarget.z > cameraToUse.nearClipPlane
            ? viewportTarget.z
            : Vector3.Distance(cameraToUse.transform.position, targetPoint);

        float startDepth = Mathf.Clamp(
            targetDepth * startDepthFactor,
            cameraToUse.nearClipPlane + minimumStartDepth,
            Mathf.Max(cameraToUse.nearClipPlane + minimumStartDepth, targetDepth - 0.5f)
        );

        return cameraToUse.ViewportToWorldPoint(new Vector3(screenStartViewport.x, screenStartViewport.y, startDepth));
    }

    Camera GetThrowCamera()
    {
        if (throwCamera != null)
            return throwCamera;

        if (Camera.main != null)
            return Camera.main;

        Camera[] cameras = FindObjectsOfType<Camera>();
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (candidate != null && candidate.isActiveAndEnabled && candidate.targetTexture == null)
                return candidate;
        }

        return null;
    }

    Vector3 GetTargetPoint(Transform target, Vector3 fallback, Camera cameraToUse)
    {
        if (target == null)
            return cameraToUse != null ? fallback + cameraToUse.transform.forward * 4f : fallback + Vector3.forward * 4f;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
            return target.position;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.center;
    }

    Vector3 GetTargetImpactEffectPoint(Transform target, Vector3 fallback)
    {
        if (target == null)
            return fallback;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
            return target.position;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float height = Mathf.Lerp(bounds.min.y, bounds.max.y, targetImpactHeight);
        return new Vector3(bounds.center.x, height, bounds.center.z);
    }

    void SetLayerRecursive(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        foreach (Transform child in root)
            SetLayerRecursive(child, layer);
    }

    void SetRenderersVisible(GameObject projectile, bool visible)
    {
        Renderer[] renderers = projectile.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = visible;
    }

    void FitProjectileScale(Transform projectile, Camera cameraToUse, Vector3 startPos)
    {
        Renderer[] renderers = projectile.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float currentSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (currentSize <= 0.0001f)
            return;

        float targetSize = 1f;
        if (cameraToUse != null)
        {
            float distance = Mathf.Max(cameraToUse.nearClipPlane + 0.1f, Vector3.Distance(cameraToUse.transform.position, startPos));
            float worldHeight = 2f * distance * Mathf.Tan(cameraToUse.fieldOfView * 0.5f * Mathf.Deg2Rad);
            targetSize = worldHeight * viewportHeightFraction;
        }

        targetSize = Mathf.Clamp(targetSize, minimumWorldSize, maximumWorldSize);
        projectile.localScale *= (targetSize / currentSize) * Mathf.Max(0.01f, throwScaleMultiplier);
    }

    void SpawnImpactEffect(Vector3 impactPosition, Transform target, Camera cameraToUse)
    {
        if (impactEffects == null)
            return;

        int effectIndex = randomizeImpactEffect ? GetRandomImpactEffectIndex() : selectedImpactEffectIndex;
        if (effectIndex < 0 || effectIndex >= impactEffects.Length)
            return;

        ImpactEffectSlot slot = impactEffects[effectIndex];
        if (slot == null || slot.prefab == null)
            return;

        Quaternion rotation = cameraToUse != null
            ? Quaternion.LookRotation(-cameraToUse.transform.forward, Vector3.up)
            : Quaternion.identity;
        rotation *= Quaternion.Euler(slot.rotationOffset);

        Transform parent = parentEffectToTarget ? target : null;
        Vector3 spawnPosition = impactPosition + slot.positionOffset + Vector3.up * globalImpactVerticalOffset;
        GameObject effect = Instantiate(slot.prefab, spawnPosition, rotation, parent);
        effect.transform.localScale = slot.scale;

        float lifetime = Mathf.Max(0f, slot.destroyAfterSeconds);
        if (lifetime > 0f)
            Destroy(effect, lifetime);
    }

    int GetRandomImpactEffectIndex()
    {
        int availableCount = 0;
        for (int i = 0; i < impactEffects.Length; i++)
        {
            if (impactEffects[i] != null && impactEffects[i].prefab != null)
                availableCount++;
        }

        if (availableCount == 0)
            return -1;

        int selected = Random.Range(0, availableCount);
        for (int i = 0; i < impactEffects.Length; i++)
        {
            if (impactEffects[i] == null || impactEffects[i].prefab == null)
                continue;

            if (selected == 0)
                return i;

            selected--;
        }

        return -1;
    }

    void EnsureImpactEffectSlots()
    {
        if (impactEffects == null)
        {
            impactEffects = new ImpactEffectSlot[ImpactEffectSlotCount];
        }
        else if (impactEffects.Length != ImpactEffectSlotCount)
        {
            ImpactEffectSlot[] resized = new ImpactEffectSlot[ImpactEffectSlotCount];
            int count = Mathf.Min(impactEffects.Length, resized.Length);
            for (int i = 0; i < count; i++)
                resized[i] = impactEffects[i];
            impactEffects = resized;
        }

        for (int i = 0; i < impactEffects.Length; i++)
        {
            if (impactEffects[i] == null)
                impactEffects[i] = new ImpactEffectSlot();
        }
    }

    void DisableProjectileAudio(GameObject projectile)
    {
        AudioSource[] sources = projectile.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < sources.Length; i++)
            sources[i].enabled = false;
    }
}
