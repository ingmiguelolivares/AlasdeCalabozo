using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SinglePlayerDiceThrowSetup
{
    const string ScenePath = "Assets/Scenes/multijugador/SinglePlayer.unity";

    [MenuItem("Tools/Dice Combat/Setup Dice Throw Controller")]
    public static void SetupDiceThrowController()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        TurnBasedGame_SP combat = Object.FindAnyObjectByType<TurnBasedGame_SP>();

        if (combat == null)
        {
            Debug.LogError("[DiceThrowSetup] TurnBasedGame_SP not found in SinglePlayer.");
            return;
        }

        DiceThrowController throwController = combat.GetComponent<DiceThrowController>();
        if (throwController == null)
            throwController = combat.gameObject.AddComponent<DiceThrowController>();

        SerializedObject serializedCombat = new SerializedObject(combat);
        SerializedProperty property = serializedCombat.FindProperty("diceThrowController");
        if (property != null)
        {
            property.objectReferenceValue = throwController;
            serializedCombat.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(combat);
        EditorUtility.SetDirty(throwController);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeObject = throwController;
        Debug.Log("[DiceThrowSetup] DiceThrowController added and linked on SinglePlayer combat object.");
    }
}
