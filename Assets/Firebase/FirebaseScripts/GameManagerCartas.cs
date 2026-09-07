using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManagerCartas : MonoBehaviour
{
    public TMP_Text textoMensajeCartaDesbloqueada; // Para mostrar el mensaje de la carta desbloqueada
    public Button botonContinuar; // El bot�n para continuar
    public GameObject cartaPrefab; // Prefab de la carta que se instanciar�
    public GameObject imagenFelicidadesPrefab; // Prefab de la imagen de Felicidades
    public Transform canvasTransform; // Transform del Canvas donde se instanciar�n las cartas

    // Lista de cartas disponibles (que el jugador puede desbloquear)
    private List<string> cartasDisponibles = new List<string>()
    {
        "AGUILA PESCADORA", "ALCARAVAN", "BUHO LISTADO", "CARPINTERO AHUMADO",
        "CHIRLOBIRLO", "CHORLITO GRITON", "CHULO", "COPETON", "CORMORAN",
        "FOCHA AMERICANA", "GARCITA RAYADA", "GARZA REAL", "GARZA SILBADORA",
        "GAVILAN MAROMERO", "GUACO", "MIRLA PATINARANJA", "PATO PICO AZUL",
        "PERIQUITO DE ANTEOJOS", "SINSONTE", "TINGUA PICO VERDE"
    };

    private void Start()
    {
        // Desactivar la imagen de Felicidades al inicio
        if (imagenFelicidadesPrefab != null)
        {
            imagenFelicidadesPrefab.SetActive(false); // Aseguramos que la imagen de Felicidades est� oculta al principio
        }

        // Agregar el listener al bot�n para continuar
        botonContinuar.onClick.AddListener(CargarInventario);
        BatallaExitosa();  // Llamar para simular una batalla exitosa al iniciar
    }

    public void BatallaExitosa()
    {
        // Llamamos a la funci�n de selecci�n de carta de manera as�ncrona con un callback
        SeleccionarCartaAleatoria((cartaDesbloqueada) =>
        {
            if (string.IsNullOrEmpty(cartaDesbloqueada))
            {
                Debug.LogWarning("No se pudo seleccionar una carta.");
                return;
            }

            // Guardamos la carta como desbloqueada en Firebase
            #if UNITY_ANDROID && !UNITY_EDITOR
FirebaseController.Instance.GuardarCartaDesbloqueada(cartaDesbloqueada);
#else
PlayerPrefs.SetInt("carta_" + cartaDesbloqueada, 1);
PlayerPrefs.Save();
#endif

            // Mostramos la carta en la UI
            textoMensajeCartaDesbloqueada.text = "¡Has liberado un Ave! " + cartaDesbloqueada + "!";
            Sprite cartaSprite = Resources.Load<Sprite>("CARTASAVESUNITY/" + cartaDesbloqueada); // Cargar la imagen
            print("CARTASAVESUNITY/" + cartaDesbloqueada);
            if (cartaSprite != null)
            {
                // Instanciar la carta dentro del Canvas
                InstanciarCarta(cartaDesbloqueada);
            }
            else
            {
                Debug.Log("Imagen no encontrada para la carta: " + cartaDesbloqueada);
            }

            // Verificamos si todas las cartas han sido desbloqueadas
            VerificarCartasDesbloqueadas(); // Llamamos a esta funci�n para verificar si el jugador ha desbloqueado todas las cartas

            // Redirige a la escena del Inventario despu�s de un retraso
            Invoke("CargarInventario", 8f); // Redirige despu�s de 2 segundos
        });
    }

    // Instanciar la carta dentro del Canvas
    private void InstanciarCarta(string cartaDesbloqueada)
    {
        // Instanciamos el prefab de la carta
        GameObject nuevaCarta = Instantiate(cartaPrefab, canvasTransform);

        // Configurar el componente Image de la carta instanciada
        Image imagenCarta = nuevaCarta.GetComponent<Image>();
        if (imagenCarta != null)
        {
            // Cargar la imagen de la carta desde Resources
            Sprite cartaSprite = Resources.Load<Sprite>("CARTASAVESUNITY/" + cartaDesbloqueada);
            if (cartaSprite != null)
            {
                imagenCarta.sprite = cartaSprite; // Asignar la imagen al componente Image
            }
            else
            {
                Debug.LogWarning("Imagen no encontrada para la carta: " + cartaDesbloqueada);
            }
        }
    }

    // Funci�n que verifica si todas las cartas han sido desbloqueadas
public void VerificarCartasDesbloqueadas()
{
#if UNITY_ANDROID && !UNITY_EDITOR
    // 🔥 Obtener las cartas desbloqueadas desde Firebase
    FirebaseController.Instance.ObtenerCartasDesbloqueadas((cartas) =>
    {
        bool todasDesbloqueadas = true;

        // Verificamos si todas las cartas están desbloqueadas
        foreach (var carta in cartas)
        {
            if (!carta.Value) // Si alguna carta no está desbloqueada
            {
                todasDesbloqueadas = false;
                break;
            }
        }

        // Si todas las cartas están desbloqueadas, mostramos la imagen de Felicidades
        if (todasDesbloqueadas)
        {
            FirebaseController.Instance.GuardarCartaDesbloqueada("FELICIDADES");
            InstanciarImagenFelicidades();
        }
    });
#else
    // 💾 Verificamos las cartas desbloqueadas con PlayerPrefs
    bool todasDesbloqueadas = true;

    foreach (string carta in cartasDisponibles)
    {
        if (PlayerPrefs.GetInt("carta_" + carta, 0) != 1)
        {
            todasDesbloqueadas = false;
            break;
        }
    }

    if (todasDesbloqueadas)
    {
        PlayerPrefs.SetInt("carta_FELICIDADES", 1);
        PlayerPrefs.Save();
        InstanciarImagenFelicidades();
    }
#endif
}
    // Instanciar la imagen de Felicidades dentro del Canvas
    private void InstanciarImagenFelicidades()
    {
        // Verificar si la imagen de Felicidades ya est� instanciada
        if (imagenFelicidadesPrefab != null && !imagenFelicidadesPrefab.activeSelf)
        {
            // Instanciamos el prefab de la imagen de Felicidades
            GameObject imagenFelicidades = Instantiate(imagenFelicidadesPrefab, canvasTransform);

            // Asignar la imagen de Felicidades
            Sprite felicidadesSprite = Resources.Load<Sprite>("CARTASAVESUNITY/Felicidades");
            if (felicidadesSprite != null)
            {
                imagenFelicidades.GetComponent<Image>().sprite = felicidadesSprite; // Asignar la imagen de Felicidades al componente Image
            }
            else
            {
                Debug.LogWarning("Imagen de Felicidades no encontrada.");
            }
        }
        else
        {
            Debug.LogWarning("Prefab de Felicidades ya instanciado o no asignado.");
        }
    }

  private void SeleccionarCartaAleatoria(System.Action<string> callback)
{
#if UNITY_ANDROID && !UNITY_EDITOR
    List<string> cartasDesbloqueadas = new List<string>();

    // Obtenemos las cartas desbloqueadas desde Firebase
    FirebaseController.Instance.ObtenerCartasDesbloqueadas((cartas) =>
    {
        // Filtramos las cartas desbloqueadas
        foreach (KeyValuePair<string, bool> carta in cartas)
        {
            if (carta.Value) // Si está desbloqueada
            {
                cartasDesbloqueadas.Add(carta.Key);
            }
        }

        // Filtramos las cartas disponibles que no han sido desbloqueadas
        List<string> cartasDisponiblesSinDesbloqueadas = new List<string>(cartasDisponibles);

        foreach (var carta in cartasDesbloqueadas)
        {
            cartasDisponiblesSinDesbloqueadas.Remove(carta);
        }

        // Si todas las cartas han sido desbloqueadas, no necesitamos seleccionar ninguna nueva
        if (cartasDisponiblesSinDesbloqueadas.Count == 0)
        {
            // Si todas las cartas están desbloqueadas, pasamos "FELICIDADES" al callback
            callback("FELICIDADES");
            Debug.LogWarning("¡Todas las cartas ya están desbloqueadas!");
        }
        else
        {
            // Seleccionar una carta aleatoria de las cartas disponibles no desbloqueadas
            int index = Random.Range(0, cartasDisponiblesSinDesbloqueadas.Count);
            string cartaSeleccionada = cartasDisponiblesSinDesbloqueadas[index];
            callback(cartaSeleccionada); // Pasamos la carta seleccionada al callback
        }
    });
#else
    List<string> cartasDesbloqueadas = new List<string>();

    // Obtenemos las cartas desbloqueadas desde PlayerPrefs
    foreach (string carta in cartasDisponibles)
    {
        if (PlayerPrefs.GetInt("carta_" + carta, 0) == 1)
        {
            cartasDesbloqueadas.Add(carta);
        }
    }

    // Filtramos las cartas disponibles que no han sido desbloqueadas
    List<string> cartasDisponiblesSinDesbloqueadas = new List<string>(cartasDisponibles);

    foreach (var carta in cartasDesbloqueadas)
    {
        cartasDisponiblesSinDesbloqueadas.Remove(carta);
    }

    // Si todas las cartas han sido desbloqueadas, no necesitamos seleccionar ninguna nueva
    if (cartasDisponiblesSinDesbloqueadas.Count == 0)
    {
        callback("FELICIDADES");
        Debug.LogWarning("¡Todas las cartas ya están desbloqueadas!");
    }
    else
    {
        // Seleccionar una carta aleatoria de las cartas disponibles no desbloqueadas
        int index = Random.Range(0, cartasDisponiblesSinDesbloqueadas.Count);
        string cartaSeleccionada = cartasDisponiblesSinDesbloqueadas[index];
        callback(cartaSeleccionada);
    }
#endif
}

    // Cargar la escena de Inventario
    void CargarInventario()
    {
        SceneManager.LoadScene("Cartas");
    }
}