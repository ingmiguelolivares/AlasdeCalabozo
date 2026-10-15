using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartaManager : MonoBehaviour
{
    private static readonly string[] CardNames =
    {
        "AGUILA PESCADORA", "ALCARAVAN", "BUHO LISTADO", "CARPINTERO AHUMADO",
        "CHIRLOBIRLO", "CHORLITO GRITON", "CHULO", "COPETON", "CORMORAN",
        "FOCHA AMERICANA", "GARCITA RAYADA", "GARZA REAL", "GARZA SILBADORA",
        "GAVILAN MAROMERO", "GUACO", "MIRLA PATINARANJA", "PATO PICO AZUL",
        "PERIQUITO DE ANTEOJOS", "SINSONTE", "TINGUA PICO VERDE"
    };

    public GameObject cartaPrefab;
    public Transform contenedorCartas;

    private void Start() => MostrarCartasDesbloqueadas();

    public void MostrarCartasDesbloqueadas()
    {
        foreach (Transform child in contenedorCartas) Destroy(child.gameObject);

        foreach (string cartaNombre in CardNames)
        {
            if (PlayerPrefs.GetInt("carta_" + cartaNombre, 0) != 1) continue;

            GameObject cartaUI = Instantiate(cartaPrefab, contenedorCartas);
            cartaUI.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 300);
            Sprite cartaSprite = Resources.Load<Sprite>("CARTASAVESUNITY/" + cartaNombre);
            if (cartaSprite != null) cartaUI.GetComponent<Image>().sprite = cartaSprite;
            else Debug.LogWarning("Imagen no encontrada para la carta: " + cartaNombre);

            TextMeshProUGUI texto = cartaUI.GetComponentInChildren<TextMeshProUGUI>();
            if (texto != null) texto.text = cartaNombre;
        }
    }
}
