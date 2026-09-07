using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class CartasLoader : MonoBehaviour
{
    void Start()
    {
        ImprimirCartasEnCarpeta();
    }

    void ImprimirCartasEnCarpeta()
    {
        Sprite[] cartas = Resources.LoadAll<Sprite>("CARTASAVESUNITY");

        if (cartas.Length == 0)
        {
            Debug.LogWarning("No se encontraron cartas en la carpeta Resources/CARTASAVESUNITY/");
        }

        foreach (Sprite carta in cartas)
        {
            Debug.Log("🃏 Carta encontrada: " + carta.name);
        }
    }
}
#endif
