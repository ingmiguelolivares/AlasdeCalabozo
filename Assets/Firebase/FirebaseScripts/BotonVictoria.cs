using UnityEngine;
using UnityEngine.UI; // Necesario para trabajar con UI
using UnityEngine.SceneManagement; // Necesario para cargar escenas
using TMPro;

public class BotonVictoria : MonoBehaviour
{
    public Button botonIrAVictoria; // El botón para ir a la escena de Victoria

    private void Start()
    {
        // Añadir listener al botón para cargar la escena de Victoria
        botonIrAVictoria.onClick.AddListener(IrAVictoria);
    }

    // Función que será llamada al presionar el botón
    public void IrAVictoria()
    {
        SceneManager.LoadScene("Victoria"); // Cambiar "Victoria" por el nombre de la escena que desees
    }
}
