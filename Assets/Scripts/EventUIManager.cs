using UnityEngine;
using UnityEngine.SceneManagement;

public class EventUIManager : MonoBehaviour
{
    public void OnMapButtonClick()
    {
        SceneManager.LoadScene("Location-basedGame");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Botón presionado!");
#endif
    }
}
