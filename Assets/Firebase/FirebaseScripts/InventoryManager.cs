using System.Collections.Generic;
using UnityEngine;

#if !UNITY_WEBGL && !UNITY_EDITOR
using Firebase;
using Firebase.Auth;
using Firebase.Database;
#endif

public class InventoryManager : MonoBehaviour
{
#if !UNITY_WEBGL && !UNITY_EDITOR
    private FirebaseAuth auth;
    private FirebaseUser user;
    private DatabaseReference db;
#endif

    void Start()
    {
#if !UNITY_WEBGL && !UNITY_EDITOR
        // Inicializa Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseDatabase.DefaultInstance.RootReference;
            user = auth.CurrentUser;

            if (user != null)
            {
                Debug.Log("Usuario autenticado: " + user.Email);
                ObtenerCartasDesbloqueadas();
            }
            else
            {
                Debug.LogWarning("Usuario no autenticado");
                // SceneManager.LoadScene("Login");
            }
        });
#else
        Debug.Log("🌐 WebGL o Editor: usando PlayerPrefs");
        ObtenerCartasDesbloqueadas();
#endif
    }

#if !UNITY_WEBGL && !UNITY_EDITOR
    void ObtenerCartasDesbloqueadas()
    {
        if (user != null)
        {
            string uid = user.UserId;
            db.Child("Usuarios").Child(uid).Child("cartasDesbloqueadas").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        foreach (var carta in snapshot.Children)
                        {
                            string cartaNombre = carta.Key;
                            bool cartaDesbloqueada = (bool)carta.Value;

                            if (cartaDesbloqueada)
                            {
                                Debug.Log("✅ Carta desbloqueada: " + cartaNombre);
                            }
                        }
                    }
                }
            });
        }
    }
#endif

#if UNITY_WEBGL || UNITY_EDITOR
    void ObtenerCartasDesbloqueadas()
    {
        List<string> cartas = GetNombresCartas();
        foreach (var carta in cartas)
        {
            string key = "cartasDesbloqueadas_" + carta;
            bool desbloqueada = PlayerPrefs.GetInt(key, 0) == 1;
            if (desbloqueada)
            {
                Debug.Log("✅ Carta desbloqueada (PlayerPrefs): " + carta);
            }
        }
    }
#endif

    // Utilidad compartida
    private List<string> GetNombresCartas()
    {
        return new List<string>
        {
            "ÁGUILA PESCADORA",
            "ALCARAVÁN",
            "BUHO LISTADO",
            "CARPINTERO AHUMADO",
            "CHIRLOBIRLO",
            "CHORLITO GRITÓN",
            "CHULO",
            "COPETÓN",
            "CORMORÁN",
            "FOCHA AMERICANA",
            "GARCITA RAYADA",
            "GARZA REAL",
            "GARZA SILBADORA",
            "GAVILÁN MAROMERO",
            "GUACO",
            "MIRLA PATINARANJA",
            "PATO PICO AZUL",
            "PERIQUITO DE ANTEOJOS",
            "SINSONTE",
            "TINGUA PICO VERDE",
            "FELICIDADES"
        };
    }
}
