using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegadorMenu : MonoBehaviour
{
    public void IrAEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }
}
