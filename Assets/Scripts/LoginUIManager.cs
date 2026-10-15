using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUIManager : MonoBehaviour
{
    private const string EmailKey = "usuario_correo";
    private const string PasswordHashKey = "usuario_password_hash";

    public TMP_InputField inputCorreo;
    public TMP_InputField inputPassword;
    public Button botonLogin;
    public Button botonRegistro;
    public TMP_Text textoError;

    private void Start()
    {
        textoError.text = string.Empty;
        MigrateLegacyPassword();

        botonLogin.onClick.AddListener(IniciarSesion);
        botonRegistro.onClick.AddListener(Registrar);
    }

    private void IniciarSesion()
    {
        textoError.text = string.Empty;

        bool credentialsMatch = PlayerPrefs.HasKey(EmailKey)
            && PlayerPrefs.GetString(EmailKey) == inputCorreo.text.Trim()
            && PlayerPrefs.GetString(PasswordHashKey) == HashPassword(inputPassword.text);

        if (!credentialsMatch)
        {
            textoError.text = "Correo o contraseña incorrectos.";
            return;
        }

        SceneManager.LoadScene("Location-basedGame");
    }

    private void Registrar()
    {
        string correo = inputCorreo.text.Trim();
        string password = inputPassword.text;
        textoError.text = string.Empty;

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrEmpty(password))
        {
            textoError.text = "Ingresa correo y contraseña.";
            return;
        }

        if (PlayerPrefs.HasKey(EmailKey))
        {
            textoError.text = "Ya hay un usuario registrado en este dispositivo.";
            return;
        }

        PlayerPrefs.SetString(EmailKey, correo);
        PlayerPrefs.SetString(PasswordHashKey, HashPassword(password));
        PlayerPrefs.Save();
        SceneManager.LoadScene("Location-basedGame");
    }

    private static void MigrateLegacyPassword()
    {
        const string legacyKey = "usuario_password";
        if (!PlayerPrefs.HasKey(legacyKey) || PlayerPrefs.HasKey(PasswordHashKey)) return;

        PlayerPrefs.SetString(PasswordHashKey, HashPassword(PlayerPrefs.GetString(legacyKey)));
        PlayerPrefs.DeleteKey(legacyKey);
        PlayerPrefs.Save();
    }

    private static string HashPassword(string password)
    {
        using SHA256 sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return System.Convert.ToBase64String(hash);
    }
}
