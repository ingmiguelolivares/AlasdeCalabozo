#if UNITY_EDITOR
using System.IO;
using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class WebGLOptimizationTools
{
    const string MenuRoot = "Tools/WebGL Optimization/";
    const string CardsFolder = "Assets/AddressableContent/CARTASAVESUNITY";

    [MenuItem(MenuRoot + "Prepare Project For WebGL")]
    public static void PrepareProjectForWebGL()
    {
        ConfigureAddressables();
        ConfigureCardTextures();
        ConfigureUiTextures();
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

            importer.defaultSampleSettings = settings;
            importer.preloadAudioData = false;
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
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { folder });
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
}
#endif
