
using FMODUnity;
using FMOD.Studio;
using UnityEngine;

public class FMODButtonSound  : MonoBehaviour
{
    // Nombre exacto del evento FMOD
    [EventRef]
    public string eventPath = "event:/";

    private EventInstance eventInstance;

    public void PlaySound()
    {
        eventInstance = RuntimeManager.CreateInstance(eventPath);
        eventInstance.start();
        eventInstance.release(); // libera recurso después de reproducir
    }
}
