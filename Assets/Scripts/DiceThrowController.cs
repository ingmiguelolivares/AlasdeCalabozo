using System.Collections;
using UnityEngine;

public class DiceThrowController : MonoBehaviour
{
    public IEnumerator Throw(Transform die, Transform target, float seconds, float spinSpeed)
    {
        if (die == null)
            yield break;

        Vector3 startPos = die.position;
        Quaternion startRot = die.rotation;
        Vector3 endPos = target != null ? target.position : startPos + die.forward * 2f;
        seconds = Mathf.Max(0.05f, seconds);

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            float arc = Mathf.Sin(t * Mathf.PI) * 1.25f;
            die.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * arc;
            die.Rotate(Vector3.one, spinSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }

        die.position = endPos;
        die.rotation = Random.rotation;
        yield return new WaitForSeconds(0.15f);
        die.position = startPos;
        die.rotation = startRot;
    }
}
