using UnityEngine;

/// Enum con los modos que tu juego reconoce.
/// Agrega m�s modos si los necesitas (p.ej. Tutorial, Survival�).
public enum GameMode
{
    Dragon,      // Escena normal contra el drag�n
    FinalBoss    // Escena Boss Final ��Esfinge
}

/// Singleton que persiste entre escenas y guarda la configuraci�n
/// de la partida que se est� por crear / jugar.
public class MatchSettings : MonoBehaviour
{
    public static MatchSettings Instance;

    [Header("Modo seleccionado para esta partida")]
    public GameMode mode = GameMode.Dragon;   // valor por defecto

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);    // persiste entre escenas
        }
        else
        {
            Destroy(gameObject);              // evita duplicados
 }
    }
}