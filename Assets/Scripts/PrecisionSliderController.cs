using UnityEngine;
using UnityEngine.UI;

public class PrecisionSliderController : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] float speed = 1.5f;

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
    }

    public void Begin(float newSpeed)
    {
        ConfigureSlider();
        speed = Mathf.Max(0.05f, newSpeed);
        direction = 1f;
        running = true;
        if (slider != null)
            slider.value = 0f;
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
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }
}
