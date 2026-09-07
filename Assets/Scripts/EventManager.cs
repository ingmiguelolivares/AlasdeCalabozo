using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    public int MaxDistance = 35;
    public void ActivateEvent(int eventID)
    {
        
            SceneManager.LoadScene("MainMenu");
        
       
    }
}
