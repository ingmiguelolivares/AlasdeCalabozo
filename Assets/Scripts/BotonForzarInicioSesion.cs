using UnityEngine;

public class BotonForzarInicioSesion : MonoBehaviour
{
    public void CargarEscenaIniciarSesion()
    {
        Debug.Log("Intentando cargar escena: IniciarSesion");

        // Elimina managers persistentes si los hay
        GameObject mapaManager = GameObject.Find("MapaManager");
        if (mapaManager != null)
        {
            Destroy(mapaManager);
            Debug.Log("MapaManager destruido.");
        }

        // Opcional: limpiar PlayerPrefs si afectan el inicio.
        // PlayerPrefs.DeleteAll();

        // Carga segura
        WebGLSceneLoader.Load(this, "IniciarSesion");
    }
}
