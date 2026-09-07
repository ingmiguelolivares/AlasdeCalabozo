using UnityEngine;

public class FMODSceneEventPlayer : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string fmodEvent; // Aquí asignas el evento FMOD desde inspector

    private FMOD.Studio.EventInstance eventInstance;

    void Start()
    {
        eventInstance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        eventInstance.start();
    }

    void OnDestroy()
    {
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        eventInstance.release();
    }
}
