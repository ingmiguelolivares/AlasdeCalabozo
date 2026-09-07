using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

using UnityEngine.SceneManagement;
#if UNITY_WEBGL || UNITY_EDITOR
using System.Security.Cryptography;
using System.Text;
#endif

#if UNITY_ANDROID || UNITY_IOS
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
#endif

public class FirebaseController : MonoBehaviour
{
    public static FirebaseController Instance; // Instancia estática

#if UNITY_ANDROID || UNITY_IOS
    private FirebaseAuth auth;
    private FirebaseUser user;
    private DatabaseReference db;
#endif

    public TMP_Text textoEstadoFirebase;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
#if UNITY_ANDROID || UNITY_IOS
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        task.ContinueWithOnMainThread(t =>
        {
            if (t.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                user = auth.CurrentUser;
                db = FirebaseDatabase.DefaultInstance.RootReference;

                Debug.Log("✅ Firebase listo");

                if (textoEstadoFirebase != null)
                    textoEstadoFirebase.text = "✅ Firebase listo. Auth: " + (auth != null);

                // 👇 Removido: no redirigimos aquí
                // Moveremos la redirección a Start()
            }
            else
            {
                Debug.LogError("❌ Firebase no disponible: " + t.Result);
                if (textoEstadoFirebase != null)
                    textoEstadoFirebase.text = "❌ Firebase error: " + t.Result.ToString();
            }
        });
#else
        Debug.Log("🌐 WebGL activo - Firebase será manejado vía JavaScript");
#endif
    }

    private void Start()
    {
#if UNITY_ANDROID || UNITY_IOS     
        // ✅ Ahora sí: redirigimos SOLO si estamos en otra escena
        if (user == null && SceneManager.GetActiveScene().name != "IniciarSesion")
        {
            Debug.Log("🔁 Usuario no logueado, cargando escena de login...");
            SceneManager.LoadScene("IniciarSesion");
        }
#endif
    }





    public async Task<bool> SignIn(string email, string password)
    {
#if UNITY_ANDROID || UNITY_IOS
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;
            return true;
        }
        catch (FirebaseException fe)
        {
            Debug.LogError("🔥 Firebase login error: " + fe.Message);
            throw fe;
        }
        catch (System.Exception e)
        {
            Debug.LogError("🔥 Error inesperado en login: " + e.Message);
            throw e;
        }
#endif
#if UNITY_WEBGL || UNITY_EDITOR

    bool exists = PlayerPrefs.HasKey("usuario_correo");
    string storedEmail = PlayerPrefs.GetString("usuario_correo", "");

    // Compat: migrar clave antigua en texto plano si existe
    if (PlayerPrefs.HasKey("usuario_password") && !PlayerPrefs.HasKey("usuario_password_hash"))
    {
        string legacyPass = PlayerPrefs.GetString("usuario_password", "");
        PlayerPrefs.SetString("usuario_password_hash", HashPassword(legacyPass));
        PlayerPrefs.DeleteKey("usuario_password");
        PlayerPrefs.Save();
    }

    string storedPassHash = PlayerPrefs.GetString("usuario_password_hash", "");
    bool success = exists && storedEmail == email && storedPassHash == HashPassword(password);
    return await Task.FromResult(success);

#endif
    }





    public async Task<bool> RegisterNewUser(string email, string password)
    {
#if UNITY_ANDROID || UNITY_IOS
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;
            Debug.Log("Usuario creado: " + user.Email);

            string uid = user.UserId;
            db.Child("Usuarios").Child(uid).Child("correo").SetValueAsync(email).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log("Correo guardado para el usuario: " + email);
                else
                    Debug.LogError("Error al guardar correo: " + task.Exception);
            });

            db.Child("Usuarios").Child(uid).Child("cartasDesbloqueadas").SetValueAsync(new Dictionary<string, bool>
            {
                { "ÁGUILA PESCADORA", false },
                { "ALCARAVÁN", false },
                { "BUHO LISTADO", false },
                { "CARPINTERO AHUMADO", false },
                { "CHIRLOBIRLO", false },
                { "CHORLITO GRITÓN", false },
                { "CHULO", false },
                { "COPETÓN", false },
                { "CORMORÁN", false },
                { "FOCHA AMERICANA", false },
                { "GARCITA RAYADA", false },
                { "GARZA REAL", false },
                { "GARZA SILBADORA", false },
                { "GAVILÁN MAROMERO", false },
                { "GUACO", false },
                { "MIRLA PATINARANJA", false },
                { "PATO PICO AZUL", false },
                { "PERIQUITO DE ANTEOJOS", false },
                { "SINSONTE", false },
                { "TINGUA PICO VERDE", false },
                { "FELICIDADES", false }
            }).ContinueWithOnMainThread(cartasTask =>
            {
                if (cartasTask.IsCompleted)
                    Debug.Log("Cartas inicializadas para el usuario.");
                else
                    Debug.LogError("Error al inicializar cartas: " + cartasTask.Exception);
            });

            return true;
        }
        catch (FirebaseException fe)
        {
            Debug.LogError("🔥 Firebase registro error: " + fe.Message);
            throw fe;
        }
        catch (System.Exception e)
        {
            Debug.LogError("🔥 Error inesperado en registro: " + e.Message);
            throw e;
        }
#endif
#if UNITY_WEBGL || UNITY_EDITOR

    if (PlayerPrefs.HasKey("usuario_correo"))
    {
        return await Task.FromResult(false); // Usuario ya existe
    }

    PlayerPrefs.SetString("usuario_correo", email);
    PlayerPrefs.SetString("usuario_password_hash", HashPassword(password));

    // Inicializar todas las cartas
    var cartas = GetNombresCartas();
    foreach (var carta in cartas)
    {
        PlayerPrefs.SetInt("cartasDesbloqueadas_" + carta, 0);
    }

    PlayerPrefs.Save();
    return await Task.FromResult(true);

#endif

    }

#if UNITY_WEBGL || UNITY_EDITOR
    static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return "";
        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return System.Convert.ToBase64String(hash);
    }
#endif





    public void ObtenerCorreoDelUsuario(System.Action<string> callback)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (user == null)
        {
            Debug.LogWarning("Usuario no autenticado");
            callback(null);
            return;
        }

        string uid = user.UserId;
        db.Child("Usuarios").Child(uid).Child("correo").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
                callback(task.Result.Value.ToString());
            else
            {
                Debug.LogWarning("No se encontró el correo del usuario.");
                callback(null);
            }
        });
#endif
#if UNITY_WEBGL || UNITY_EDITOR

    if (!PlayerPrefs.HasKey("usuario_correo"))
    {
        callback(null);
    }
    else
    {
        callback(PlayerPrefs.GetString("usuario_correo"));
    }

#endif
    }





    public void SignOut()
    {
#if UNITY_ANDROID || UNITY_IOS
        auth.SignOut();
        user = null;
        Debug.Log("Sesión cerrada");
#endif
#if UNITY_WEBGL || UNITY_EDITOR

    Debug.Log("Simulación de cierre de sesión.");

#endif
    }






    public object GetUser()
    {
#if UNITY_ANDROID || UNITY_IOS
        return user;
#endif
#if UNITY_WEBGL || UNITY_EDITOR

    if (PlayerPrefs.HasKey("usuario_correo"))
    {
        return new { Email = PlayerPrefs.GetString("usuario_correo") };
    }
    return null;

#endif
    }





    public void GuardarCartaDesbloqueada(string nombreCarta)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (user == null)
        {
            Debug.LogWarning("Usuario no autenticado");
            return;
        }

        string uid = user.UserId;
        db.Child("Usuarios").Child(uid).Child("cartasDesbloqueadas").Child(nombreCarta).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists && (bool)task.Result.Value)
            {
                Debug.Log("❌ La carta ya está desbloqueada: " + nombreCarta);
                return;
            }

            db.Child("Usuarios").Child(uid).Child("cartasDesbloqueadas").Child(nombreCarta).SetValueAsync(true).ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCompleted)
                    Debug.Log("Carta desbloqueada: " + nombreCarta);
                else
                    Debug.LogError("Error al guardar carta: " + saveTask.Exception);
            });
        });
#endif
#if UNITY_WEBGL || UNITY_EDITOR

    string key = "cartasDesbloqueadas_" + nombreCarta;
    if (PlayerPrefs.GetInt(key, 0) == 1)
    {
        Debug.Log("❌ La carta ya está desbloqueada (PlayerPrefs): " + nombreCarta);
        return;
    }

    PlayerPrefs.SetInt(key, 1);
    PlayerPrefs.Save();
    Debug.Log("✅ Carta desbloqueada (PlayerPrefs): " + nombreCarta);

#endif
    }




    public void ObtenerCartasDesbloqueadas(System.Action<Dictionary<string, bool>> callback)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (user == null)
        {
            Debug.LogWarning("Usuario no autenticado");
            callback(new Dictionary<string, bool>());
            return;
        }

        string uid = user.UserId;
        db.Child("Usuarios").Child(uid).Child("cartasDesbloqueadas").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var cartas = new Dictionary<string, bool>();
                foreach (var carta in task.Result.Children)
                    cartas[carta.Key] = (bool)carta.Value;

                callback(cartas);
            }
            else
            {
                Debug.LogWarning("No se encontraron cartas desbloqueadas.");
                callback(new Dictionary<string, bool>());
            }
        });
#endif
#if UNITY_WEBGL || UNITY_EDITOR

    var resultado = new Dictionary<string, bool>();
    var cartas = GetNombresCartas();

    foreach (var carta in cartas)
    {
        string key = "cartasDesbloqueadas_" + carta;
        bool desbloqueada = PlayerPrefs.GetInt(key, 0) == 1;
        resultado[carta] = desbloqueada;
    }

    callback(resultado);

#endif
    }




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
