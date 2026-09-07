using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordToggle : MonoBehaviour
{
    public TMP_InputField passwordField;
    public Image toggleIcon;
    public Sprite showIcon; // ícono de ojo abierto
    public Sprite hideIcon; // ícono de ojo cerrado

    private bool isPasswordHidden = true;

    public void TogglePasswordVisibility()
    {
        isPasswordHidden = !isPasswordHidden;

        if (isPasswordHidden)
        {
            passwordField.contentType = TMP_InputField.ContentType.Password;
            toggleIcon.sprite = hideIcon;
        }
        else
        {
            passwordField.contentType = TMP_InputField.ContentType.Standard;
            toggleIcon.sprite = showIcon;
        }

        passwordField.ForceLabelUpdate(); // Actualiza visualmente el contenido
    }
}
