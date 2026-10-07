using System.Collections.Generic;
using UnityEngine;

public class ModelPreviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform modelRoot;
    [SerializeField] Camera previewCamera;
    [SerializeField] GameObject sceneModel;
    [SerializeField] bool useSceneModelOnStart = true;
    [SerializeField] GameObject defaultModelPrefab;

    [Header("Layer")]
    [SerializeField] string previewLayerName = "ModelPreview";

    [Header("Rotation")]
    [SerializeField] bool autoRotate = true;
    [SerializeField] float rotationSpeed = 25f;
    [SerializeField] Vector3 rotationAxis = Vector3.up;
    [SerializeField] bool suspendAutoRotateForRotationClips = true;

    [Header("Framing")]
    [SerializeField] Vector3 modelPositionOffset = Vector3.zero;
    [SerializeField] Vector3 cameraLookOffset = Vector3.zero;
    [SerializeField] float framePadding = 1.25f;
    [SerializeField] float minimumCameraDistance = 1.5f;

    GameObject currentModelInstance;
    Animator currentAnimator;
    Animation currentAnimation;
    bool dragSuspended;
    bool hasRotationAnimationClip;
    bool currentModelIsSceneObject;

    readonly List<string> animationClipNames = new();

    public GameObject CurrentModelInstance => currentModelInstance;
    public IReadOnlyList<string> AnimationClipNames => animationClipNames;
    public bool AnimatorFound => currentAnimator != null;
    public bool HasAnimationClips => animationClipNames.Count > 0;
    public bool AutoRotate
    {
        get => autoRotate;
        set => autoRotate = value;
    }

    void Awake()
    {
        if (modelRoot == null)
            modelRoot = transform;

        if (previewCamera == null)
            previewCamera = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        if (useSceneModelOnStart && currentModelInstance == null)
        {
            if (sceneModel == null)
                sceneModel = FindChildByName(transform, "Dados");

            if (sceneModel != null)
            {
                SetSceneModel(sceneModel);
                return;
            }
        }

        if (defaultModelPrefab != null && currentModelInstance == null)
            SetModel(defaultModelPrefab);
    }

    void Update()
    {
        if (!ShouldAutoRotate())
            return;

        modelRoot.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime, Space.Self);
    }

    public void SetInteractionActive(bool isInteracting)
    {
        dragSuspended = isInteracting;
    }

    public void RotateModel(float yDegrees)
    {
        if (modelRoot == null)
            return;

        modelRoot.Rotate(Vector3.up, yDegrees, Space.Self);
    }

    public void SetModel(GameObject modelPrefab)
    {
        if (modelRoot == null)
        {
            Debug.LogError("[ModelPreview] Missing modelRoot reference.");
            return;
        }

        ClearCurrentModel();
        animationClipNames.Clear();
        currentAnimator = null;
        currentAnimation = null;
        hasRotationAnimationClip = false;
        currentModelIsSceneObject = false;

        if (modelPrefab == null)
        {
            Debug.LogWarning("[ModelPreview] No model prefab assigned.");
            return;
        }

        modelRoot.localRotation = Quaternion.identity;
        currentModelInstance = Instantiate(modelPrefab, modelRoot);
        currentModelInstance.name = modelPrefab.name;
        currentModelInstance.transform.localPosition = Vector3.zero;
        currentModelInstance.transform.localRotation = modelPrefab.transform.localRotation;
        currentModelInstance.transform.localScale = modelPrefab.transform.localScale;

        ConfigureCurrentModel(modelPrefab.name);
    }

    public void SetSceneModel(GameObject modelObject)
    {
        if (modelRoot == null)
        {
            Debug.LogError("[ModelPreview] Missing modelRoot reference.");
            return;
        }

        ClearCurrentModel();
        animationClipNames.Clear();
        currentAnimator = null;
        currentAnimation = null;
        hasRotationAnimationClip = false;

        if (modelObject == null)
        {
            Debug.LogWarning("[ModelPreview] No scene model assigned.");
            return;
        }

        currentModelInstance = modelObject;
        currentModelIsSceneObject = true;
        currentModelInstance.SetActive(true);

        if (currentModelInstance.transform.parent != modelRoot)
            currentModelInstance.transform.SetParent(modelRoot, true);

        modelRoot.localRotation = Quaternion.identity;

        ConfigureCurrentModel(modelObject.name);
    }

    void ConfigureCurrentModel(string modelName)
    {
        int previewLayer = LayerMask.NameToLayer(previewLayerName);
        if (previewLayer >= 0)
            SetLayerRecursively(currentModelInstance, previewLayer);
        else
            Debug.LogWarning($"[ModelPreview] Layer '{previewLayerName}' not found. Preview isolation may be incomplete.");

        DetectAnimationData();
        FrameModel();

        Debug.Log($"[ModelPreview] Model loaded: {modelName}");
    }

    public void PlayAnimation(string stateName)
    {
        if (string.IsNullOrWhiteSpace(stateName))
            return;

        if (currentAnimator != null && currentAnimator.runtimeAnimatorController != null)
        {
            currentAnimator.Play(stateName);
            return;
        }

        if (currentAnimation != null && currentAnimation.GetClip(stateName) != null)
            currentAnimation.Play(stateName);
    }

    public void ClearCurrentModel()
    {
        if (currentModelInstance == null)
            return;

        if (currentModelIsSceneObject)
        {
            currentModelInstance.SetActive(false);
        }
        else if (Application.isPlaying)
        {
            Destroy(currentModelInstance);
        }
        else
        {
            DestroyImmediate(currentModelInstance);
        }

        currentModelInstance = null;
        currentModelIsSceneObject = false;
    }

    bool ShouldAutoRotate()
    {
        if (!autoRotate || dragSuspended || modelRoot == null)
            return false;

        if (suspendAutoRotateForRotationClips && hasRotationAnimationClip)
            return false;

        return rotationAxis.sqrMagnitude > 0.0001f && Mathf.Abs(rotationSpeed) > 0.001f;
    }

    void DetectAnimationData()
    {
        currentAnimator = currentModelInstance.GetComponentInChildren<Animator>();
        currentAnimation = currentModelInstance.GetComponentInChildren<Animation>();

        if (currentAnimator != null)
        {
            currentAnimator.enabled = true;
            Debug.Log("[ModelPreview] Animator detected");

            RuntimeAnimatorController controller = currentAnimator.runtimeAnimatorController;
            if (controller != null)
            {
                foreach (AnimationClip clip in controller.animationClips)
                    RegisterClip(clip);
            }
        }
        else
        {
            Debug.Log("[ModelPreview] No Animator detected; using automatic rotation");
        }

        if (currentAnimation != null)
        {
            foreach (AnimationState state in currentAnimation)
                RegisterClip(state.clip);

            if (!currentAnimation.isPlaying && currentAnimation.clip != null)
                currentAnimation.Play();
        }
    }

    void RegisterClip(AnimationClip clip)
    {
        if (clip == null || animationClipNames.Contains(clip.name))
            return;

        animationClipNames.Add(clip.name);
        string lowerName = clip.name.ToLowerInvariant();
        if (lowerName.Contains("rotate") || lowerName.Contains("rotation") || lowerName.Contains("spin") || lowerName.Contains("turn"))
            hasRotationAnimationClip = true;
    }

    void FrameModel()
    {
        Renderer[] renderers = currentModelInstance != null
            ? currentModelInstance.GetComponentsInChildren<Renderer>()
            : null;

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning("[ModelPreview] No Renderers found; camera framing skipped.");
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 rootPosition = modelRoot.position;
        currentModelInstance.transform.position += rootPosition - bounds.center + modelPositionOffset;

        if (previewCamera != null)
        {
            Renderer[] updatedRenderers = currentModelInstance.GetComponentsInChildren<Renderer>();
            bounds = updatedRenderers[0].bounds;
            for (int i = 1; i < updatedRenderers.Length; i++)
                bounds.Encapsulate(updatedRenderers[i].bounds);

            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            float fovRadians = previewCamera.fieldOfView * Mathf.Deg2Rad;
            float distance = maxExtent / Mathf.Tan(fovRadians * 0.5f);
            distance = Mathf.Max(minimumCameraDistance, distance * framePadding);

            Vector3 target = bounds.center + cameraLookOffset;
            previewCamera.transform.position = target + new Vector3(0f, bounds.extents.y * 0.15f, -distance);
            previewCamera.transform.LookAt(target);
            previewCamera.nearClipPlane = Mathf.Max(0.01f, distance - maxExtent * 3f);
            previewCamera.farClipPlane = distance + maxExtent * 4f;
        }

        Debug.Log("[ModelPreview] Bounds calculated successfully");
    }

    static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    static GameObject FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child.gameObject;
        }

        return null;
    }
}
