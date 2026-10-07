using System.Collections;
using UnityEngine;
using TMPro;

public class EnemyDicePreviewController : MonoBehaviour
{
    [SerializeField] TMP_Text resultText;

    public void SetResultText(TMP_Text text)
    {
        resultText = text;
    }

    public IEnumerator SpinAndReveal(Transform die, int result, float seconds, float spinSpeed)
    {
        if (resultText != null)
            resultText.text = "?";

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            if (die != null)
                die.Rotate(Vector3.up + Vector3.right, spinSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }

        if (resultText != null)
            resultText.text = result.ToString();
    }
}
