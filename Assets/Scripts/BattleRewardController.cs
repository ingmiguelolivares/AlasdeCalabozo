using System.Collections;
using TMPro;
using UnityEngine;

public class BattleRewardController : MonoBehaviour
{
    [SerializeField] TMP_Text rewardText;
    [SerializeField] float displaySeconds = 2.5f;

    public void SetRewardText(TMP_Text text)
    {
        rewardText = text;
    }

    public IEnumerator GrantReward(int battleIndex, bool unlockCard)
    {
        BirdCardReward reward = BirdCardProgress.GetRewardForBattle(battleIndex);
        BirdCardProgress.RegisterBirdCard(reward);

        string message;
        if (unlockCard)
        {
            BirdCardProgress.UnlockCard(reward.commonName);
            message = string.Format("Carta de ave desbloqueada: {0}\nDato: {1}", reward.commonName, reward.curiousFact);
        }
        else
        {
            message = string.Format("Informacion de ave: {0}\nPista: {1}", reward.commonName, reward.sphinxClue);
        }

        Debug.Log("[BattleReward] " + message);

        if (rewardText != null)
        {
            rewardText.text = message;
            rewardText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(displaySeconds);

        if (rewardText != null)
            rewardText.gameObject.SetActive(false);
    }
}
