using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if !UNITY_WEBGL && !UNITY_EDITOR
using Firebase.Auth;
using Firebase;
#endif

public class LoginUIManager : MonoBehaviour
{
    public TMP_InputField inputCorreo;
    public TMP_InputField inputPassword;
    public Button botonLogin;
    public Button botonRegistro;
    public TMP_Text textoError;

    private FirebaseController firebase;

    private void Start()
    {
        textoError.text = "";
        firebase = FindObjectOfType<FirebaseController>();

#if !UNITY_WEBGL && !UNITY_EDITOR
        if (firebase.GetUser() != null)
        {
            Debug.Log("Usuario ya logueado, cargando escena...");
            SceneManager.LoadScene("Location-basedGame");
        }
#elif UNITY_WEBGL || UNITY_EDITOR
    if (PlayerPrefs.HasKey("usuario_correo"))
    {
        Debug.Log("Usuario simulado ya logueado, cargando escena...");
        SceneManager.LoadScene("Location-basedGame");
    }
#endif

        botonLogin.onClick.AddListener(() => {
            IniciarSesion();
        });

        botonRegistro.onClick.AddListener(() => {
            Registrar();
        });
    }


    private void IniciarSesion()
    {
        string correo = inputCorreo.text;
        string password = inputPassword.text;
        textoError.text = "";

#if !UNITY_WEBGL && !UNITY_EDITOR
        if (Firebase.Auth.FirebaseAuth.DefaultInstance == null)
        {
            textoError.text = "🔥 Firebase Auth no está inicializado.";
            Debug.LogError("🔥 FirebaseAuth.DefaultInstance está vacío.");
            return;
        }

        IniciarSesionAsync(correo, password);
#else
    if (PlayerPrefs.HasKey("usuario_correo") && 
        PlayerPrefs.GetString("usuario_correo") == correo &&
        PlayerPrefs.GetString("usuario_password") == password)
    {
        Debug.Log("✅ Login simulado exitoso");
        SceneManager.LoadScene("Location-basedGame");
    }
    else
    {
        textoError.text = "❌ Login fallido (simulado)";
        Debug.LogWarning("Login fallido en WebGL o Editor.");
    }
#endif
    }

#if !UNITY_WEBGL && !UNITY_EDITOR
    private async void IniciarSesionAsync(string correo, string password)
    {
        try
        {
            bool resultado = await firebase.SignIn(correo, password);
            if (resultado)
            {
                Debug.Log("✅ Login exitoso");
                SceneManager.LoadScene("Location-basedGame");
            }
            else
            {
                textoError.text = "❌ Falló el login.";
            }
        }
        catch (FirebaseException fe)
        {
            string mensaje = fe.Message.ToLower();
            if (mensaje.Contains("password") && mensaje.Contains("invalid"))
                textoError.text = "🔒 Contraseña incorrecta.";
            else if (mensaje.Contains("internal error"))
                textoError.text = "🔒 Contraseña incorrecta.";
            else if (mensaje.Contains("email") && mensaje.Contains("badly"))
                textoError.text = "📧 Correo inválido.";
            else if (mensaje.Contains("user record") && mensaje.Contains("no"))
                textoError.text = "👻 Usuario no encontrado.";
            else if (mensaje.Contains("network error"))
                textoError.text = "🌐 Error de red.";
            else
                textoError.text = "🔥 Error inesperado: " + fe.Message;

            Debug.LogError("FirebaseException: " + fe.Message);
        }
    }
#endif

    private void Registrar()
    {
        string correo = inputCorreo.text;
        string password = inputPassword.text;
        textoError.text = "";

#if !UNITY_WEBGL && !UNITY_EDITOR
        if (Firebase.Auth.FirebaseAuth.DefaultInstance == null)
        {
            textoError.text = "🔥 Firebase Auth no está inicializado.";
            Debug.LogError("🔥 FirebaseAuth.DefaultInstance está vacío.");
            return;
        }

        RegistrarAsync(correo, password);
#else
    if (PlayerPrefs.HasKey("usuario_correo"))
    {
        textoError.text = "📧 Ya hay un usuario registrado.";
        return;
    }

    PlayerPrefs.SetString("usuario_correo", correo);
    PlayerPrefs.SetString("usuario_password", password);
    PlayerPrefs.Save();

    Debug.Log("👤 Usuario simulado registrado correctamente");
    SceneManager.LoadScene("Location-basedGame");
#endif
    }

#if !UNITY_WEBGL && !UNITY_EDITOR
    private async void RegistrarAsync(string correo, string password)
    {
        try
        {
            bool resultado = await firebase.RegisterNewUser(correo, password);
            if (resultado)
            {
                Debug.Log("👤 Usuario registrado correctamente");
                SceneManager.LoadScene("Location-basedGame");
            }
            else
            {
                textoError.text = "❌ Falló el registro.";
            }
        }
        catch (FirebaseException fe)
        {
            string mensaje = fe.Message.ToLower();

            if (mensaje.Contains("email address is already in use"))
                textoError.text = "📧 Este correo ya está registrado.";
            else if (mensaje.Contains("email") && mensaje.Contains("badly"))
                textoError.text = "📧 El correo está mal escrito.";
            else if (mensaje.Contains("password") && mensaje.Contains("6 characters"))
                textoError.text = "🔐 La contraseña debe tener al menos 6 caracteres.";
            else
                textoError.text = "🔥 Error inesperado: " + fe.Message;

            Debug.LogError("FirebaseException: " + fe.Message);
        }
        catch (System.Exception e)
        {
            textoError.text = "🔥 Error inesperado: " + e.Message;
            Debug.LogError(e);
        }
    }
#endif

}
