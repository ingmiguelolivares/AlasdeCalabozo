using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Sphinx Question Bank")]
public class SphinxQuestionBank : ScriptableObject
{
    [System.Serializable]
    public struct QA
    {
        [TextArea(2, 4)] public string question;
        public bool answer;  // true = Verdadero, false = Falso
    }

    public QA[] questions = new QA[25];
}