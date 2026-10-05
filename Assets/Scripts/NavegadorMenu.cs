using UnityEngine;

public class NavegadorMenu : MonoBehaviour
{
    public void IrAEscena(string nombre)
    {
        WebGLSceneLoader.Load(this, nombre);
    }
}
