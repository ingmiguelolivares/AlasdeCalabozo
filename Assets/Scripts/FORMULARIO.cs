using UnityEngine;

public class OpenURL : MonoBehaviour
{
    // Pon la URL que quieres abrir en el inspector
    public string url = "https://forms.gle/xbsk2NUS8WcMRM857";

    public void OpenLink()
    {
        Application.OpenURL(url);
    }
}

