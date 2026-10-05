using UnityEngine;

public class EventManager : MonoBehaviour
{
    public int MaxDistance = 35;
    public void ActivateEvent(int eventID)
    {
        WebGLSceneLoader.Load(this, "MainMenu");
    }
}
