using UnityEngine;
using UnityEngine.UI;

public class PrecisionSliderController : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] float speed = 1.5f;
    [Header("Visuales")]
    [SerializeField] Image fillImage;
    [SerializeField] Shadow glowEffect;
    [SerializeField] bool autoCreateGlow = true;
    [SerializeField] Color lowPowerColor = new Color(0.35f, 0.55f, 1f, 1f);
    [SerializeField] Color midPowerColor = new Color(0.95f, 0.76f, 0.25f, 1f);
    [SerializeField] Color maxPowerColor = new Color(1f, 0.28f, 0.08f, 1f);
    [SerializeField, Range(0f, 1f)] float minGlowAlpha = 0.15f;
    [SerializeField, Range(0f, 1f)] float maxGlowAlpha = 0.95f;
    [SerializeField] Vector2 minGlowDistance = new Vector2(1f, -1f);
    [SerializeField] Vector2 maxGlowDistance = new Vector2(6f, -6f);

    bool running;
    float direction = 1f;

    public float Value
    {
        get { return slider != null ? slider.value : 0.5f; }
    }

    void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        ConfigureSlider();
        CacheVisuals(true);
        UpdateVisuals();
    }

    void Update()
    {
        if (!running || slider == null)
            return;

        float value = slider.value + direction * speed * Time.deltaTime;
        if (value >= 1f)
        {
            value = 1f;
            direction = -1f;
        }
        else if (value <= 0f)
        {
            value = 0f;
            direction = 1f;
        }

        slider.value = value;
        UpdateVisuals();
    }

    void OnValidate()
    {
        ConfigureSlider();
        CacheVisuals(false);
        UpdateVisuals();
    }

    public void Begin(float newSpeed)
    {
        ConfigureSlider();
        speed = Mathf.Max(0.05f, newSpeed);
        direction = 1f;
        running = true;
        if (slider != null)
            slider.value = 0f;
        CacheVisuals(true);
        UpdateVisuals();
        gameObject.SetActive(true);
    }

    public float Stop()
    {
        running = false;
        return Value;
    }

    public void Hide()
    {
        running = false;
        gameObject.SetActive(false);
    }

    void ConfigureSlider()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    void CacheVisuals(bool allowCreateGlow)
    {
        if (slider == null)
            return;

        if (fillImage == null && slider.fillRect != null)
            fillImage = slider.fillRect.GetComponent<Image>();

        if (fillImage == null)
            return;

        if (glowEffect == null)
            glowEffect = fillImage.GetComponent<Shadow>();

        if (glowEffect == null && autoCreateGlow && allowCreateGlow)
        {
            glowEffect = fillImage.gameObject.AddComponent<Shadow>();
            glowEffect.useGraphicAlpha = true;
        }
    }

    void UpdateVisuals()
    {
        float value = Value;
        Color powerColor = value < 0.5f
            ? Color.Lerp(lowPowerColor, midPowerColor, value * 2f)
            : Color.Lerp(midPowerColor, maxPowerColor, (value - 0.5f) * 2f);

        if (fillImage != null)
            fillImage.color = powerColor;

        if (glowEffect == null)
            return;

        Color glowColor = powerColor;
        glowColor.a = Mathf.Lerp(minGlowAlpha, maxGlowAlpha, value);
        glowEffect.effectColor = glowColor;
        glowEffect.effectDistance = Vector2.Lerp(minGlowDistance, maxGlowDistance, value);
    }
}
