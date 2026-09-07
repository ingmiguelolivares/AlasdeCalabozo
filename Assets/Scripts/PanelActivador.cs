using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class PanelActivador : MonoBehaviour
{
    public GameObject panelParaMostrar;
    public float duracionMostrar = 2f;
    public AnimationCurve intensidadPorDaño; // Curva de intensidad (0-1) según daño

    private Image panelImage;



    private void Awake()
    {
        if (panelParaMostrar != null)
            panelImage = panelParaMostrar.GetComponent<Image>();
    }

    public void MostrarPanelConDaño(int daño, int dañoMaximoEsperado)
    {
        if (panelParaMostrar == null) return;

        panelParaMostrar.SetActive(true);
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif


        float intensidad = intensidadPorDaño.Evaluate((float)daño / dañoMaximoEsperado);
        StartCoroutine(TemblarYSaturar(intensidad));
    }

    private IEnumerator TemblarYSaturar(float intensidad)
    {
        Vector3 originalPos = panelParaMostrar.transform.localPosition;
        float tiempo = 0f;
        float duracionTemblor = Mathf.Lerp(0.3f, 1f, intensidad);
        float fuerzaTemblor = Mathf.Lerp(5f, 30f, intensidad);

        Color originalColor = panelImage != null ? panelImage.color : Color.white;
        Color saturado = originalColor * (1f + intensidad); // Incrementa saturación
        saturado.a = originalColor.a;

        if (panelImage != null) panelImage.color = saturado;

        while (tiempo < duracionTemblor)
        {
            float x = Random.Range(-1f, 1f) * fuerzaTemblor;
            float y = Random.Range(-1f, 1f) * fuerzaTemblor;
            panelParaMostrar.transform.localPosition = originalPos + new Vector3(x, y, 0);

            tiempo += Time.deltaTime;
            yield return null;
        }

        if (panelImage != null) panelImage.color = originalColor;
        panelParaMostrar.transform.localPosition = originalPos;

        yield return new WaitForSeconds(duracionMostrar - duracionTemblor);
        panelParaMostrar.SetActive(false);
    }
}
