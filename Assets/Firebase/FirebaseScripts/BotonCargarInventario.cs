using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BotonCargarInventario : MonoBehaviour
{
    public Button boton; // Referencia al botón en el Inspector
    private CartaManager cartaManager;  // Variable para acceder a CartaManager

    private void Start()
    {
        // Aseguramos que el botón tenga asignado el listener
        if (boton != null)
        {
            boton.onClick.AddListener(CargarInventario);
        }

        // Obtener el componente de CartaManager para mostrar las cartas
        cartaManager = FindObjectOfType<CartaManager>();
    }

    // Función para cargar la escena del Inventario y mostrar las cartas
    private void CargarInventario()
    {
        if (cartaManager != null)
        {
            cartaManager.MostrarCartasDesbloqueadas();  // Llamar la función para mostrar las cartas
        }

        // Cargar la escena del Inventario
        SceneManager.LoadScene("Cartas");  // Nombre de la escena de Inventario
    }
}

