using UnityEngine;
using UnityEngine.SceneManagement;

public class SphinxAccessController : MonoBehaviour
{
    public const int SphinxEventId = 999;

    [SerializeField] int cartasMinimasParaAparecerEsfinge = 3;
    [SerializeField] int cartasTotalesColeccion = BirdCardProgress.TotalCollectionCards;
    [SerializeField] string finalBossSceneName = "FinalBoss";
    [SerializeField] GameObject sphinxObject;

    public int CartasMinimasParaAparecerEsfinge
    {
        get { return cartasMinimasParaAparecerEsfinge; }
        set { cartasMinimasParaAparecerEsfinge = Mathf.Max(0, value); }
    }

    void Start()
    {
        RefreshSphinxAvailability();
    }

    public bool CanAccessSphinx(int collectedCards)
    {
        return collectedCards >= cartasMinimasParaAparecerEsfinge;
    }

    public int GetCollectedCardsCount()
    {
        return BirdCardProgress.GetCollectedCardsCount();
    }

    public void RegisterBirdCard(BirdCardReward reward)
    {
        BirdCardProgress.RegisterBirdCard(reward);
        RefreshSphinxAvailability();
    }

    public void RegisterBirdCard(string commonName)
    {
        BirdCardProgress.RegisterBirdCard(commonName);
        RefreshSphinxAvailability();
    }

    public void RefreshSphinxAvailability()
    {
        bool available = CanAccessSphinx(GetCollectedCardsCount());
        if (sphinxObject != null)
            sphinxObject.SetActive(available);

        Debug.Log(string.Format("[SphinxAccess] Cartas {0}/{1}. Esfinge disponible: {2}",
            GetCollectedCardsCount(), cartasTotalesColeccion, available));
    }

    public void LoadFinalBossIfAllowed()
    {
        if (!CanAccessSphinx(GetCollectedCardsCount()))
        {
            Debug.Log("[SphinxAccess] La Esfinge sigue bloqueada. Cartas requeridas: " + cartasMinimasParaAparecerEsfinge);
            return;
        }

        SceneManager.LoadScene(finalBossSceneName);
    }
}
