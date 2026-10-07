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

    public IEnumerator GrantReward(int battleIndex)
    {
        BirdCardReward reward = BirdCardProgress.GetRewardForBattle(battleIndex);
        BirdCardProgress.RegisterBirdCard(reward);

        string message = "Carta obtenida:\n" + reward.ToRewardText();
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
