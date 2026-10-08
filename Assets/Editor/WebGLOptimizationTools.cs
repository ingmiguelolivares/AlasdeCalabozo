#if UNITY_EDITOR
using System.IO;
using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WebGLOptimizationTools
{
    const string MenuRoot = "Tools/WebGL Optimization/";
    const string CardsFolder = "Assets/AddressableContent/CARTASAVESUNITY";
    const string ModelTexturesFolder = "Assets/Modelos Bichitos/Materiales bichos";
    const string ModelsFolder = "Assets/Modelos Bichitos";
    const string VfxFolder = "Assets/Hovl Studio/Magic effects pack";
    const string EasyModelsFolder = "Assets/Modelos Bichitos/EasyModels";
    const string MediumModelsFolder = "Assets/Modelos Bichitos/MidModels";
    const string HardModelsFolder = "Assets/Modelos Bichitos/HardModels";
    static readonly string[] CombatScenePaths =
    {
        "Assets/Scenes/multijugador/SinglePlayer.unity",
        "Assets/Scenes/multijugador/multplayertest02.unity"
    };

    [MenuItem(MenuRoot + "Prepare Project For WebGL")]
    public static void PrepareProjectForWebGL()
    {
        ConfigureAddressables();
        ConfigureMonsterAddressables();
        ClearDirectModelReferencesInCombatScenes();
        ConfigureCardTextures();
        ConfigureUiTextures();
        ConfigureModelTextures();
        ConfigureVfxTextures();
        ConfigureModelImporters();
        ConfigureAudioClips();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Preparacion WebGL terminada. Revisa los grupos Addressables y ejecuta Build > New Build > Default Build Script.");
    }

    [MenuItem(MenuRoot + "Configure Addressables Groups")]
    public static void ConfigureAddressables()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("No se encontro AddressableAssetSettings. Abre Window > Asset Management > Addressables > Groups para crearlo.");
            return;
        }

        settings.BuildRemoteCatalog = true;
        settings.OptimizeCatalogSize = true;
        SetBuildAddressablesWithPlayer(settings);

        settings.AddLabel("cards", false);
        settings.AddLabel("webgl_remote", false);

        AddressableAssetGroup cardsGroup = GetOrCreatePackedGroup(settings, "Remote_Cards", true);
        AddFolderAssetsToGroup(settings, cardsGroup, CardsFolder, "cards");

        EditorUtility.SetDirty(settings);
        Debug.Log("Grupos Addressables configurados para contenido remoto.");
    }

    [MenuItem(MenuRoot + "Configure Monster Addressables")]
    public static void ConfigureMonsterAddressables()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("No se encontro AddressableAssetSettings. Abre Window > Asset Management > Addressables > Groups para crearlo.");
            return;
        }

        settings.BuildRemoteCatalog = true;
        settings.OptimizeCatalogSize = true;
        SetBuildAddressablesWithPlayer(settings);

        settings.AddLabel("monsters_easy", false);
        settings.AddLabel("monsters_medium", false);
        settings.AddLabel("monsters_hard", false);
        settings.AddLabel("webgl_remote", false);

        AddressableAssetGroup easyGroup = GetOrCreatePackedGroup(settings, "Remote_Monsters_Easy", true);
        AddressableAssetGroup mediumGroup = GetOrCreatePackedGroup(settings, "Remote_Monsters_Medium", true);
        AddressableAssetGroup hardGroup = GetOrCreatePackedGroup(settings, "Remote_Monsters_Hard", true);

        AddFolderAssetsToGroup(settings, easyGroup, EasyModelsFolder, "monsters_easy", "t:Prefab");
        AddFolderAssetsToGroup(settings, mediumGroup, MediumModelsFolder, "monsters_medium", "t:Prefab");
        AddFolderAssetsToGroup(settings, hardGroup, HardModelsFolder, "monsters_hard", "t:Prefab");

        EditorUtility.SetDirty(settings);
        Debug.Log("Modelos de enemigos configurados como Addressables remotos.");
    }

    [MenuItem(MenuRoot + "Clear Direct Model References In Combat Scenes")]
    public static void ClearDirectModelReferencesInCombatScenes()
    {
        string activeScenePath = SceneManager.GetActiveScene().path;

        foreach (string scenePath in CombatScenePaths)
        {
            if (!File.Exists(scenePath))
                continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ModelsLoader[] loaders = UnityEngine.Object.FindObjectsOfType<ModelsLoader>(true);
            foreach (ModelsLoader loader in loaders)
            {
                SerializedObject serializedLoader = new SerializedObject(loader);
                ClearArray(serializedLoader, "easyModels");
                ClearArray(serializedLoader, "mediumModels");
                ClearArray(serializedLoader, "hardModels");
                SetBool(serializedLoader, "loadAddressableModelsWhenArraysEmpty", true);
                SetString(serializedLoader, "easyModelsLabel", "monsters_easy");
                SetString(serializedLoader, "mediumModelsLabel", "monsters_medium");
                SetString(serializedLoader, "hardModelsLabel", "monsters_hard");
                serializedLoader.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(loader);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);

        Debug.Log("Referencias directas a modelos limpiadas en escenas de combate. Los enemigos cargaran por Addressables.");
    }

    [MenuItem(MenuRoot + "Configure Texture Importers")]
    public static void ConfigureCardTextures()
    {
        ConfigureTexturesInFolder(CardsFolder, 1024, true);
        ConfigureTexturesInFolder("Assets/ASSETS/00.CARTAS", 1024, true);
        ConfigureTexturesInFolder("Assets/ASSETS/15.Cartas", 1024, true);
    }

    [MenuItem(MenuRoot + "Configure UI Texture Importers")]
    public static void ConfigureUiTextures()
    {
        ConfigureTexturesInFolder("Assets/ASSETS/00.NUEVOS ASSETS", 1024, true);
        ConfigureTexturesInFolder("Assets/13.Perdiste", 1024, true);
    }

    [MenuItem(MenuRoot + "Configure Model Texture Importers")]
    public static void ConfigureModelTextures()
    {
        ConfigureTexturesInFolder(ModelTexturesFolder, 1024, true);
    }

    [MenuItem(MenuRoot + "Configure VFX Texture Importers")]
    public static void ConfigureVfxTextures()
    {
        ConfigureTexturesInFolder(VfxFolder, 512, true);
    }

    [MenuItem(MenuRoot + "Configure Model Importers")]
    public static void ConfigureModelImporters()
    {
        ConfigureModelsInFolder(ModelsFolder);
        ConfigureModelsInFolder("Assets/ASSETS");
        Debug.Log("Importadores de modelos configurados para WebGL.");
    }

    [MenuItem(MenuRoot + "Configure Audio Importers")]
    public static void ConfigureAudioClips()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/ImaginatioSound" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
                continue;

            long size = new FileInfo(path).Length;
            var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = size > 5 * 1024 * 1024 ? 0.55f : 0.65f;
            settings.loadType = size > 5 * 1024 * 1024 ? AudioClipLoadType.Streaming : AudioClipLoadType.CompressedInMemory;
            settings.preloadAudioData = false;

            importer.defaultSampleSettings = settings;
            importer.loadInBackground = true;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        Debug.Log("Importadores de audio configurados para WebGL.");
    }

    static AddressableAssetGroup GetOrCreatePackedGroup(AddressableAssetSettings settings, string groupName, bool remote)
    {
        AddressableAssetGroup group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, false, null,
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));
        }

        var schema = group.GetSchema<BundledAssetGroupSchema>();
        if (schema == null)
            schema = group.AddSchema<BundledAssetGroupSchema>();

        schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;
        schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
        schema.UseAssetBundleCache = true;
        schema.IncludeInBuild = true;

        if (remote)
        {
            schema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
            schema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");
        }

        return group;
    }

    static void SetBuildAddressablesWithPlayer(AddressableAssetSettings settings)
    {
        var property = typeof(AddressableAssetSettings).GetProperty("BuildAddressablesWithPlayerBuild");
        if (property == null || !property.PropertyType.IsEnum)
            return;

        property.SetValue(settings, Enum.ToObject(property.PropertyType, 1));
    }

    static void AddFolderAssetsToGroup(AddressableAssetSettings settings, AddressableAssetGroup group, string folder, string label)
    {
        AddFolderAssetsToGroup(settings, group, folder, label, "t:Object");
    }

    static void AddFolderAssetsToGroup(AddressableAssetSettings settings, AddressableAssetGroup group, string folder, string label, string filter)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        string[] guids = AssetDatabase.FindAssets(filter, new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
                continue;

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = Path.GetFileNameWithoutExtension(path);
            entry.SetLabel(label, true, true);
            entry.SetLabel("webgl_remote", true, true);
        }
    }

    static void ClearArray(SerializedObject target, string propertyName)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property != null && property.isArray)
            property.arraySize = 0;
    }

    static void SetBool(SerializedObject target, string propertyName, bool value)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property != null)
            property.boolValue = value;
    }

    static void SetString(SerializedObject target, string propertyName, string value)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property != null)
            property.stringValue = value;
    }

    static void ConfigureTexturesInFolder(string folder, int maxSize, bool crunch)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.compressionQuality = 50;

            var webgl = importer.GetPlatformTextureSettings("WebGL");
            webgl.overridden = true;
            webgl.maxTextureSize = maxSize;
            webgl.textureCompression = TextureImporterCompression.Compressed;
            webgl.crunchedCompression = crunch;
            webgl.compressionQuality = 50;
            importer.SetPlatformTextureSettings(webgl);

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }

    static void ConfigureModelsInFolder(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                continue;

            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;
            importer.importCameras = false;
            importer.importLights = false;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }
}
#endif
