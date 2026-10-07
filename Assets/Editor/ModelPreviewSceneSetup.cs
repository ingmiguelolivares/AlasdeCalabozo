using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ModelPreviewSceneSetup
{
    const string ScenePath = "Assets/Scenes/multijugador/SinglePlayer.unity";
    const string LayerName = "ModelPreview";
    const string AssetFolder = "Assets/ModelPreview";
    const string RenderTexturePath = AssetFolder + "/ModelPreviewRenderTexture.renderTexture";
    const string UIPrefabPath = "Assets/Prefabs/ModelPreviewUI.prefab";

    [MenuItem("Tools/Model Preview/Setup SinglePlayer")]
    public static void SetupSinglePlayer()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        int previewLayer = EnsureLayer(LayerName);
        int previewMask = 1 << previewLayer;

        EnsureFolder(AssetFolder);
        RenderTexture renderTexture = EnsureRenderTexture();

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[ModelPreviewSetup] Canvas not found in SinglePlayer.");
            return;
        }

        GameObject environment = EnsureGameObject("ModelPreviewEnvironment");
        environment.transform.position = new Vector3(10000f, 10000f, 10000f);
        environment.transform.rotation = Quaternion.identity;
        environment.transform.localScale = Vector3.one;
        environment.layer = previewLayer;

        Transform modelRoot = EnsureChild(environment.transform, "PreviewModelRoot");
        modelRoot.localPosition = Vector3.zero;
        modelRoot.localRotation = Quaternion.identity;
        modelRoot.localScale = Vector3.one;
        modelRoot.gameObject.layer = previewLayer;

        Camera previewCamera = EnsurePreviewCamera(environment.transform, renderTexture, previewMask);
        Light previewLight = EnsurePreviewLight(environment.transform, previewMask);

        ModelPreviewController controller = environment.GetComponent<ModelPreviewController>();
        if (controller == null)
            controller = environment.AddComponent<ModelPreviewController>();

        GameObject sceneModel = FindSceneModel(environment.transform);

        var controllerSo = new SerializedObject(controller);
        controllerSo.FindProperty("modelRoot").objectReferenceValue = modelRoot;
        controllerSo.FindProperty("previewCamera").objectReferenceValue = previewCamera;
        controllerSo.FindProperty("sceneModel").objectReferenceValue = sceneModel;
        controllerSo.FindProperty("useSceneModelOnStart").boolValue = true;
        controllerSo.FindProperty("defaultModelPrefab").objectReferenceValue = null;
        controllerSo.FindProperty("previewLayerName").stringValue = LayerName;
        controllerSo.FindProperty("autoRotate").boolValue = true;
        controllerSo.FindProperty("rotationSpeed").floatValue = 25f;
        controllerSo.FindProperty("rotationAxis").vector3Value = Vector3.up;
        controllerSo.ApplyModifiedPropertiesWithoutUndo();

        if (sceneModel != null)
        {
            sceneModel.SetActive(true);
            sceneModel.transform.SetParent(modelRoot, true);
            SetObjectLayerRecursively(sceneModel, previewLayer);
        }

        GameObject panel = EnsurePreviewPanel(canvas.transform, renderTexture, controller);
        SaveReusableUIPrefab(panel);

        ExcludePreviewLayerFromSceneCameras(previewMask, previewCamera);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ModelPreviewSetup] SinglePlayer model preview setup complete.");
    }

    [MenuItem("Tools/Model Preview/Validate SinglePlayer")]
    public static void ValidateSinglePlayer()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        int previewLayer = LayerMask.NameToLayer(LayerName);
        int previewMask = 1 << previewLayer;
        bool pass = true;

        pass &= LogCheck(previewLayer >= 0, "Layer ModelPreview exists");
        pass &= LogCheck(Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude).Length == 1, "Single Canvas present");
        pass &= LogCheck(Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude).Length == 1, "Single EventSystem present");

        ModelPreviewController controller = Object.FindAnyObjectByType<ModelPreviewController>();
        Camera previewCamera = GameObject.Find("PreviewCamera")?.GetComponent<Camera>();
        Light previewLight = GameObject.Find("PreviewLight")?.GetComponent<Light>();
        RawImage rawImage = GameObject.Find("ModelPreviewRawImage")?.GetComponent<RawImage>();
        ModelPreviewDrag drag = rawImage != null ? rawImage.GetComponent<ModelPreviewDrag>() : null;
        RenderTexture renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);

        pass &= LogCheck(controller != null, "ModelPreviewController present");
        pass &= LogCheck(previewCamera != null, "PreviewCamera present");
        pass &= LogCheck(previewLight != null, "PreviewLight present");
        pass &= LogCheck(rawImage != null, "ModelPreviewRawImage present");
        pass &= LogCheck(renderTexture != null && renderTexture.width == 512 && renderTexture.height == 512, "RenderTexture is 512x512");

        if (previewCamera != null)
        {
            pass &= LogCheck(previewCamera.targetTexture == renderTexture, "PreviewCamera targets RenderTexture");
            pass &= LogCheck(previewCamera.cullingMask == previewMask, "PreviewCamera only renders ModelPreview layer");
            pass &= LogCheck(previewCamera.GetComponent<AudioListener>() == null, "PreviewCamera has no AudioListener");
        }

        if (previewLight != null)
            pass &= LogCheck(previewLight.cullingMask == previewMask, "PreviewLight only affects ModelPreview layer");

        if (rawImage != null)
        {
            pass &= LogCheck(rawImage.texture == renderTexture, "RawImage displays RenderTexture");
            pass &= LogCheck(rawImage.raycastTarget, "RawImage receives pointer events");
        }

        pass &= LogCheck(drag != null && drag is IDragHandler, "Drag handler uses EventSystem pointer events");

        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude))
        {
            if (camera == previewCamera || camera.targetTexture != null)
                continue;

            pass &= LogCheck((camera.cullingMask & previewMask) == 0, $"{camera.name} excludes ModelPreview layer");
        }

        GameObject sceneModel = FindSceneModel(GameObject.Find("ModelPreviewEnvironment")?.transform);
        pass &= LogCheck(sceneModel != null, "Scene model Dados found");

        if (controller != null && sceneModel != null)
        {
            controller.SetSceneModel(sceneModel);
            GameObject firstInstance = controller.CurrentModelInstance;
            pass &= LogCheck(firstInstance == sceneModel, "Scene model is used directly");
            pass &= LogCheck(firstInstance.GetComponentsInChildren<Renderer>().Length > 0, "Preview model has renderers");
            pass &= LogCheck(AllChildrenUseLayer(firstInstance.transform, previewLayer), "Preview model layer assigned recursively");

            Quaternion beforeRotation = controller.transform.GetChild(0).localRotation;
            controller.RotateModel(15f);
            Quaternion afterRotation = controller.transform.GetChild(0).localRotation;
            pass &= LogCheck(beforeRotation != afterRotation, "Manual drag rotation path rotates model root");

            Debug.Log($"[ModelPreviewValidation] Animator found: {controller.AnimatorFound}");
            Debug.Log($"[ModelPreviewValidation] Animation clips found: {controller.AnimationClipNames.Count}");
        }

        if (pass)
            Debug.Log("[ModelPreviewValidation] PASS");
        else
            Debug.LogError("[ModelPreviewValidation] FAIL");
    }

    static RenderTexture EnsureRenderTexture()
    {
        RenderTexture renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32)
            {
                name = "ModelPreviewRenderTexture",
                useMipMap = false,
                autoGenerateMips = false
            };
            AssetDatabase.CreateAsset(renderTexture, RenderTexturePath);
        }

        renderTexture.width = 512;
        renderTexture.height = 512;
        renderTexture.depth = 24;
        renderTexture.format = RenderTextureFormat.ARGB32;
        EditorUtility.SetDirty(renderTexture);
        return renderTexture;
    }

    static Camera EnsurePreviewCamera(Transform parent, RenderTexture renderTexture, int previewMask)
    {
        Transform child = parent.Find("PreviewCamera");
        GameObject cameraObject = child != null ? child.gameObject : new GameObject("PreviewCamera");
        cameraObject.transform.SetParent(parent, false);
        cameraObject.layer = LayerMask.NameToLayer(LayerName);

        Camera camera = cameraObject.GetComponent<Camera>();
        if (camera == null)
            camera = cameraObject.AddComponent<Camera>();

        Object.DestroyImmediate(cameraObject.GetComponent<AudioListener>());

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camera.cullingMask = previewMask;
        camera.targetTexture = renderTexture;
        camera.fieldOfView = 35f;
        camera.nearClipPlane = 0.03f;
        camera.farClipPlane = 100f;
        camera.depth = -10f;
        camera.allowHDR = true;
        camera.allowMSAA = true;

        camera.transform.localPosition = new Vector3(0f, 1f, -4f);
        camera.transform.localRotation = Quaternion.identity;
        camera.transform.LookAt(parent.position);
        return camera;
    }

    static Light EnsurePreviewLight(Transform parent, int previewMask)
    {
        Transform child = parent.Find("PreviewLight");
        GameObject lightObject = child != null ? child.gameObject : new GameObject("PreviewLight");
        lightObject.transform.SetParent(parent, false);
        lightObject.layer = LayerMask.NameToLayer(LayerName);

        Light light = lightObject.GetComponent<Light>();
        if (light == null)
            light = lightObject.AddComponent<Light>();

        light.type = LightType.Directional;
        light.intensity = 1.3f;
        light.color = Color.white;
        light.cullingMask = previewMask;
        light.shadows = LightShadows.Soft;

        light.transform.localPosition = new Vector3(0f, 2f, -2f);
        light.transform.localRotation = Quaternion.Euler(35f, -30f, 0f);
        return light;
    }

    static GameObject EnsurePreviewPanel(Transform canvasTransform, RenderTexture renderTexture, ModelPreviewController controller)
    {
        Transform existing = canvasTransform.Find("ModelPreviewPanel");
        GameObject panel = existing != null ? existing.gameObject : new GameObject("ModelPreviewPanel", typeof(RectTransform));
        panel.transform.SetParent(canvasTransform, false);
        panel.layer = LayerMask.NameToLayer("UI");

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-48f, -220f);
        panelRect.sizeDelta = new Vector2(360f, 360f);
        panelRect.localScale = Vector3.one;

        Image background = panel.GetComponent<Image>();
        if (background == null)
            background = panel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.25f);
        background.raycastTarget = false;

        Transform rawImageTransform = panel.transform.Find("ModelPreviewRawImage");
        GameObject rawImageObject = rawImageTransform != null
            ? rawImageTransform.gameObject
            : new GameObject("ModelPreviewRawImage", typeof(RectTransform));
        rawImageObject.transform.SetParent(panel.transform, false);
        rawImageObject.layer = LayerMask.NameToLayer("UI");

        RectTransform rawRect = rawImageObject.GetComponent<RectTransform>();
        rawRect.anchorMin = Vector2.zero;
        rawRect.anchorMax = Vector2.one;
        rawRect.offsetMin = new Vector2(12f, 12f);
        rawRect.offsetMax = new Vector2(-12f, -12f);
        rawRect.localScale = Vector3.one;

        RawImage rawImage = rawImageObject.GetComponent<RawImage>();
        if (rawImage == null)
            rawImage = rawImageObject.AddComponent<RawImage>();
        rawImage.texture = renderTexture;
        rawImage.color = Color.white;
        rawImage.raycastTarget = true;

        ModelPreviewDrag drag = rawImageObject.GetComponent<ModelPreviewDrag>();
        if (drag == null)
            drag = rawImageObject.AddComponent<ModelPreviewDrag>();

        var dragSo = new SerializedObject(drag);
        dragSo.FindProperty("previewController").objectReferenceValue = controller;
        dragSo.FindProperty("dragSensitivity").floatValue = 0.35f;
        dragSo.FindProperty("resumeAutoRotateDelay").floatValue = 0.5f;
        dragSo.ApplyModifiedPropertiesWithoutUndo();

        return panel;
    }

    static GameObject FindDefaultModelPrefab()
    {
        ModelsLoader loader = Object.FindAnyObjectByType<ModelsLoader>();
        if (loader != null && loader.easyModels != null)
        {
            foreach (GameObject model in loader.easyModels)
            {
                if (model != null)
                    return model;
            }
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Modelos Bichitos/EasyModels" });
        if (guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static GameObject FindSceneModel(Transform environment)
    {
        if (environment == null)
            return null;

        foreach (Transform child in environment.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Dados")
                return child.gameObject;
        }

        return null;
    }

    static bool AllChildrenUseLayer(Transform root, int layer)
    {
        if (root.gameObject.layer != layer)
            return false;

        foreach (Transform child in root)
        {
            if (!AllChildrenUseLayer(child, layer))
                return false;
        }

        return true;
    }

    static void SetObjectLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
            SetObjectLayerRecursively(child.gameObject, layer);
    }

    static bool LogCheck(bool condition, string label)
    {
        if (condition)
            Debug.Log($"[ModelPreviewValidation] PASS: {label}");
        else
            Debug.LogError($"[ModelPreviewValidation] FAIL: {label}");

        return condition;
    }

    static void ExcludePreviewLayerFromSceneCameras(int previewMask, Camera previewCamera)
    {
        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude))
        {
            if (camera == previewCamera || camera.targetTexture != null)
                continue;

            camera.cullingMask &= ~previewMask;
            EditorUtility.SetDirty(camera);
        }
    }

    static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0)
            return existing;

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 6; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(layer.stringValue))
                continue;

            layer.stringValue = layerName;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return i;
        }

        throw new System.InvalidOperationException("No free Unity layer available for ModelPreview.");
    }

    static GameObject EnsureGameObject(string name)
    {
        GameObject existing = GameObject.Find(name);
        return existing != null ? existing : new GameObject(name);
    }

    static Transform EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            return child;

        GameObject childObject = new GameObject(name);
        childObject.transform.SetParent(parent, false);
        return childObject.transform;
    }

    static void SaveReusableUIPrefab(GameObject panel)
    {
        string directory = Path.GetDirectoryName(UIPrefabPath);
        if (!string.IsNullOrEmpty(directory))
            EnsureFolder(directory.Replace("\\", "/"));

        PrefabUtility.SaveAsPrefabAssetAndConnect(panel, UIPrefabPath, InteractionMode.AutomatedAction);
    }

    static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string folderName = Path.GetFileName(folderPath);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, folderName);
    }
}
