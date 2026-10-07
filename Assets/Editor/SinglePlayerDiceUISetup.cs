using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SinglePlayerDiceUISetup
{
    const string ScenePath = "Assets/Scenes/multijugador/SinglePlayer.unity";

    [MenuItem("Tools/Dice Combat/Setup SinglePlayer UI")]
    public static void SetupSinglePlayerUI()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        TurnBasedGame_SP combat = Object.FindAnyObjectByType<TurnBasedGame_SP>();

        if (canvas == null)
        {
            Debug.LogError("[DiceUISetup] Canvas not found. No se creara un Canvas nuevo.");
            return;
        }

        if (combat == null)
        {
            Debug.LogError("[DiceUISetup] TurnBasedGame_SP not found.");
            return;
        }

        PrecisionSliderController precisionSlider = EnsureSlider(canvas.transform, "PrecisionSlider", new Vector2(0f, -210f), "Precision Fill");
        PrecisionSliderController defenseSlider = EnsureSlider(canvas.transform, "DefenseSlider", new Vector2(0f, -260f), "Defense Fill");
        Button defenseButton = EnsureButton(canvas.transform, "ButtonDefense", "Defender", new Vector2(270f, -260f));
        TMP_Text diceResultText = EnsureText(canvas.transform, "DiceResultText", new Vector2(0f, -310f), new Vector2(620f, 70f), 32, "");
        TMP_Text rewardText = EnsureText(canvas.transform, "BirdRewardText", new Vector2(0f, 120f), new Vector2(720f, 180f), 22, "");

        precisionSlider.gameObject.SetActive(false);
        defenseSlider.gameObject.SetActive(false);
        defenseButton.gameObject.SetActive(false);
        diceResultText.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(false);

        SerializedObject serializedCombat = new SerializedObject(combat);
        Assign(serializedCombat, "precisionSlider", precisionSlider);
        Assign(serializedCombat, "defenseSlider", defenseSlider);
        Assign(serializedCombat, "defenseButton", defenseButton);
        Assign(serializedCombat, "diceResultText", diceResultText);
        Assign(serializedCombat, "rewardText", rewardText);
        serializedCombat.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[DiceUISetup] SinglePlayer UI de dados creada/conectada sin duplicar Canvas, EventSystem ni camaras.");
    }

    static void Assign(SerializedObject target, string propertyName, Object value)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property != null)
            property.objectReferenceValue = value;
        else
            Debug.LogWarning("[DiceUISetup] Serialized property not found: " + propertyName);
    }

    static PrecisionSliderController EnsureSlider(Transform parent, string name, Vector2 anchoredPosition, string fillName)
    {
        Transform existing = parent.Find(name);
        GameObject root = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Slider));
        if (existing == null)
            root.transform.SetParent(parent, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(360f, 28f);
        rect.anchoredPosition = anchoredPosition;

        Image background = EnsureImage(root.transform, "Background", new Color(0.07f, 0.07f, 0.08f, 0.85f));
        Stretch(background.rectTransform, Vector2.zero, Vector2.zero);

        RectTransform fillArea = EnsureRect(root.transform, "Fill Area");
        fillArea.anchorMin = Vector2.zero;
        fillArea.anchorMax = Vector2.one;
        fillArea.offsetMin = new Vector2(6f, 6f);
        fillArea.offsetMax = new Vector2(-6f, -6f);

        Image fill = EnsureImage(fillArea, fillName, new Color(0.95f, 0.76f, 0.25f, 1f));
        Stretch(fill.rectTransform, Vector2.zero, Vector2.zero);

        Slider slider = root.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.wholeNumbers = false;
        slider.targetGraphic = fill;
        slider.fillRect = fill.rectTransform;

        PrecisionSliderController controller = root.GetComponent<PrecisionSliderController>();
        if (controller == null)
            controller = root.AddComponent<PrecisionSliderController>();

        return controller;
    }

    static Button EnsureButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        Transform existing = parent.Find(name);
        GameObject root = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        if (existing == null)
            root.transform.SetParent(parent, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(150f, 44f);
        rect.anchoredPosition = anchoredPosition;

        Image image = root.GetComponent<Image>();
        image.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);

        TMP_Text text = EnsureText(root.transform, "Text", Vector2.zero, rect.sizeDelta, 20, label);
        text.alignment = TextAlignmentOptions.Center;
        Stretch(text.rectTransform, Vector2.zero, Vector2.zero);

        return root.GetComponent<Button>();
    }

    static TMP_Text EnsureText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, int fontSize, string textValue)
    {
        Transform existing = parent.Find(name);
        GameObject root = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        if (existing == null)
            root.transform.SetParent(parent, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        TMP_Text text = root.GetComponent<TMP_Text>();
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.text = textValue;
        text.color = Color.white;
        return text;
    }

    static Image EnsureImage(Transform parent, string name, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject root = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
        if (existing == null)
            root.transform.SetParent(parent, false);

        Image image = root.GetComponent<Image>();
        image.color = color;
        return image;
    }

    static RectTransform EnsureRect(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        GameObject root = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        if (existing == null)
            root.transform.SetParent(parent, false);
        return root.GetComponent<RectTransform>();
    }

    static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
