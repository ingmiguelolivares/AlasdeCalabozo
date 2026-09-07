using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class positions : MonoBehaviour
{
    public Transform jugador;
    public Transform mapa;

    void Update()
    {
        if (jugador != null && mapa != null)
        {
            Debug.Log($"[Jugador] Posición: {jugador.position}, Rotación: {jugador.rotation.eulerAngles}");
            Debug.Log($"[Mapa]    Posición: {mapa.position}, Rotación: {mapa.rotation.eulerAngles}");
        }
        else
        {
            Debug.LogWarning("🚨 Asigna referencias a 'jugador' y 'mapa' en el Inspector.");
        }
    }
}
#endif
