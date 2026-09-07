using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CartaManager : MonoBehaviour
{
    public GameObject cartaPrefab; // Prefab de la carta (un "Image" que representa la carta)
    public Transform contenedorCartas; // Panel donde se mostrar�n las cartas
    private FirebaseController firebaseController;


private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    firebaseController = FindObjectOfType<FirebaseController>();

    // Verificamos si el usuario está autenticado
    if (firebaseController.GetUser() == null)
    {
        Debug.LogWarning("Usuario no autenticado. Redirigiendo al Iniciar Sesion.");
        // Redirige al login si el usuario no está autenticado
        SceneManager.LoadScene("IniciarSesion");  // Cambia "Login" por el nombre de tu escena de login
        return;
    }
#endif

        // Mostrar cartas desbloqueadas
        MostrarCartasDesbloqueadas();
    }
 public void MostrarCartasDesbloqueadas()
{
#if UNITY_ANDROID && !UNITY_EDITOR
    // 🔥 Ejecutar solo en Android real
    firebaseController.ObtenerCartasDesbloqueadas((cartas) =>
    {
        // Limpiar las cartas previas
        foreach (Transform child in contenedorCartas)
        {
            Destroy(child.gameObject);
        }

        // Instanciar solo las cartas desbloqueadas
        foreach (KeyValuePair<string, bool> carta in cartas)
        {
            if (carta.Value)
            {
                // Instanciamos la carta en la UI
                GameObject cartaUI = Instantiate(cartaPrefab, contenedorCartas);

                // Aseguramos que la carta tenga el tamaño adecuado
                cartaUI.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 300);

                // Cargar la imagen de la carta desde Resources
                string cartaNombre = carta.Key;
                Sprite cartaSprite = Resources.Load<Sprite>("CARTASAVESUNITY/" + cartaNombre);

                if (cartaSprite != null)
                {
                    cartaUI.GetComponent<Image>().sprite = cartaSprite;
                }
                else
                {
                    Debug.LogWarning("Imagen no encontrada para la carta: " + cartaNombre);
                }

                // Asignar texto
                TextMeshProUGUI texto = cartaUI.GetComponentInChildren<TextMeshProUGUI>();
                if (texto != null)
                {
                    texto.text = cartaNombre;
                }
            }else{
                print("carta no funciona");
            }
        }
    });
#else
    // 💾 Ejecutar en otras plataformas usando PlayerPrefs

    // Limpiar cartas previas
    foreach (Transform child in contenedorCartas)
    {
        Destroy(child.gameObject);
    }

    // Evitamos usar cartasDisponibles directamente si está en otra clase
    // Se puede usar una lista fija o una fuente externa si es necesario.
    string[] nombresCartas = new string[]
    {
        "AGUILA PESCADORA", "ALCARAVAN", "BUHO LISTADO", "CARPINTERO AHUMADO",
        "CHIRLOBIRLO", "CHORLITO GRITON", "CHULO", "COPETON", "CORMORAN",
        "FOCHA AMERICANA", "GARCITA RAYADA", "GARZA REAL", "GARZA SILBADORA",
        "GAVILAN MAROMERO", "GUACO", "MIRLA PATINARANJA", "PATO PICO AZUL",
        "PERIQUITO DE ANTEOJOS", "SINSONTE", "TINGUA PICO VERDE"
    };

    foreach (string cartaNombre in nombresCartas)
    {
        if (PlayerPrefs.GetInt("carta_" + cartaNombre, 0) == 1)
        {
            GameObject cartaUI = Instantiate(cartaPrefab, contenedorCartas);
            cartaUI.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 300);
             print("CARTASAVESUNITY/" + cartaNombre);
            Sprite cartaSprite = Resources.Load<Sprite>("CARTASAVESUNITY/" + cartaNombre);
                if (cartaSprite != null)
                {
                    cartaUI.GetComponent<Image>().sprite = cartaSprite;
                   
                }
                else
                {
                    Debug.Log("Imagen no encontrada para la carta: " + cartaNombre);
                }

            TextMeshProUGUI texto = cartaUI.GetComponentInChildren<TextMeshProUGUI>();
            if (texto != null)
            {
                texto.text = cartaNombre;
            }
        }
    }
#endif
}
}
