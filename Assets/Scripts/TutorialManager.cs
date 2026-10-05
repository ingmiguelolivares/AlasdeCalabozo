using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject PantallaPaso3;
    public GameObject PantallaPaso4;

    public void MostrarPaso4()
    {
        PantallaPaso3.SetActive(false);
        PantallaPaso4.SetActive(true);
    }

    public void RegresarAlMenu()
    {
        WebGLSceneLoader.Load(this, "MenuPrincipal");
    }
}
