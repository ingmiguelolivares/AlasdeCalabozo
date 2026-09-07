using FMODUnity;
using FMOD.Studio;
using UnityEngine;

public class MonsterSound : MonoBehaviour
{
    [EventRef]
    public string fmodEventPath = "event:/Monster_Tarrasque";

    private EventInstance eventInstance;

    public void PlaySound()
    {
        eventInstance = RuntimeManager.CreateInstance(fmodEventPath);
        eventInstance.start();
        eventInstance.release();
    }
}
