using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerCartas : MonoBehaviour
{
    private readonly List<string> cartasDisponibles = new()
    {
        "AGUILA PESCADORA", "ALCARAVAN", "BUHO LISTADO", "CARPINTERO AHUMADO",
        "CHIRLOBIRLO", "CHORLITO GRITON", "CHULO", "COPETON", "CORMORAN",
        "FOCHA AMERICANA", "GARCITA RAYADA", "GARZA REAL", "GARZA SILBADORA",
        "GAVILAN MAROMERO", "GUACO", "MIRLA PATINARANJA", "PATO PICO AZUL",
        "PERIQUITO DE ANTEOJOS", "SINSONTE", "TINGUA PICO VERDE"
    };

    public TMP_Text textoMensajeCartaDesbloqueada;
    public Button botonContinuar;
    public GameObject cartaPrefab;
    public GameObject imagenFelicidadesPrefab;
    public Transform canvasTransform;

    private void Start()
    {
        if (imagenFelicidadesPrefab != null) imagenFelicidadesPrefab.SetActive(false);
        botonContinuar.onClick.AddListener(CargarInventario);
        BatallaExitosa();
    }

    public void BatallaExitosa()
    {
        string cartaDesbloqueada = SeleccionarCartaAleatoria();
        if (string.IsNullOrEmpty(cartaDesbloqueada)) return;

        PlayerPrefs.SetInt("carta_" + cartaDesbloqueada, 1);
        PlayerPrefs.Save();
        textoMensajeCartaDesbloqueada.text = "¡Has liberado un Ave! " + cartaDesbloqueada + "!";
        InstanciarCarta(cartaDesbloqueada);
        VerificarCartasDesbloqueadas();
        Invoke(nameof(CargarInventario), 8f);
    }

    public void VerificarCartasDesbloqueadas()
    {
        foreach (string carta in cartasDisponibles)
            if (PlayerPrefs.GetInt("carta_" + carta, 0) != 1) return;

        PlayerPrefs.SetInt("carta_FELICIDADES", 1);
        PlayerPrefs.Save();
        InstanciarImagenFelicidades();
    }

    private string SeleccionarCartaAleatoria()
    {
        List<string> disponibles = cartasDisponibles.FindAll(carta => PlayerPrefs.GetInt("carta_" + carta, 0) != 1);
        if (disponibles.Count == 0) return "FELICIDADES";
        return disponibles[Random.Range(0, disponibles.Count)];
    }

    private void InstanciarCarta(string cartaNombre)
    {
        GameObject nuevaCarta = Instantiate(cartaPrefab, canvasTransform);
        Sprite sprite = Resources.Load<Sprite>("CARTASAVESUNITY/" + cartaNombre);
        if (sprite != null) nuevaCarta.GetComponent<Image>().sprite = sprite;
        else Debug.LogWarning("Imagen no encontrada para la carta: " + cartaNombre);
    }

    private void InstanciarImagenFelicidades()
    {
        if (imagenFelicidadesPrefab == null || imagenFelicidadesPrefab.activeSelf) return;
        Instantiate(imagenFelicidadesPrefab, canvasTransform).SetActive(true);
    }

    private void CargarInventario() => SceneManager.LoadScene("Cartas");
}
