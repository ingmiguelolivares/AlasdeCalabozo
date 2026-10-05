using UnityEngine;

public class EventUIManager : MonoBehaviour
{
    public void OnMapButtonClick()
    {
        WebGLSceneLoader.Load(this, "Location-basedGame");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Botón presionado!");
#endif
    }
}
