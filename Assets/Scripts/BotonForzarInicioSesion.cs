using UnityEngine;
using UnityEngine.SceneManagement;

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

        // (Opcional) Limpiar PlayerPrefs si están afectando el inicio
        // PlayerPrefs.DeleteAll();

        // Carga segura
        SceneManager.LoadScene("IniciarSesion", LoadSceneMode.Single);
    }
}
