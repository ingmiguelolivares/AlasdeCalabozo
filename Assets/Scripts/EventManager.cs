using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    public int MaxDistance = 35;
    private bool isLoadingSinglePlayer;

    public void ActivateEvent(int eventID)
    {
        if (isLoadingSinglePlayer)
        {
            Debug.LogWarning("🎮 La escena SinglePlayer ya se está cargando.");
            return;
        }

        const string singlePlayerScene = "SinglePlayer";
        if (!Application.CanStreamedLevelBeLoaded(singlePlayerScene))
        {
            Debug.LogError($"❌ No se puede cargar '{singlePlayerScene}'. Verifica que esté habilitada en File > Build Profiles.");
            return;
        }

        // El flujo publicado es local: no pasa por el menú multijugador de Photon.
        isLoadingSinglePlayer = true;
        Debug.Log($"🎮 Evento {eventID}: cargando escena local {singlePlayerScene}.");
        SceneManager.LoadSceneAsync(singlePlayerScene, LoadSceneMode.Single);
    }
}
