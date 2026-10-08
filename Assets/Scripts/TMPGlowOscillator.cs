using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPGlowOscillator : MonoBehaviour
{
    static readonly int GlowInnerId = Shader.PropertyToID("_GlowInner");
    static readonly int GlowOuterId = Shader.PropertyToID("_GlowOuter");
    static readonly int GlowPowerId = Shader.PropertyToID("_GlowPower");

    [Header("Target")]
    [SerializeField] TMP_Text targetText;

    [Header("Timing")]
    [SerializeField] float speed = 1.25f;
    [SerializeField] bool useUnscaledTime = true;

    [Header("Glow Inner")]
    [SerializeField, Range(0f, 0.3f)] float innerMin = 0.05f;
    [SerializeField, Range(0f, 0.3f)] float innerMax = 0.25f;

    [Header("Glow Outer")]
    [SerializeField, Range(0f, 1f)] float outerMin = 0.05f;
    [SerializeField, Range(0f, 1f)] float outerMax = 0.35f;

    [Header("Glow Power")]
    [SerializeField, Range(0.5f, 1f)] float powerMin = 0.5f;
    [SerializeField, Range(0.5f, 1f)] float powerMax = 1f;

    Material runtimeMaterial;

    void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        CreateRuntimeMaterial();
        ClampRanges();
    }

    void OnValidate()
    {
        ClampRanges();
    }

    void Update()
    {
        if (runtimeMaterial == null)
            CreateRuntimeMaterial();

        if (runtimeMaterial == null)
            return;

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float oscillation = (Mathf.Sin(time * speed) + 1f) * 0.5f;

        runtimeMaterial.SetFloat(GlowInnerId, Mathf.Lerp(innerMin, innerMax, oscillation));
        runtimeMaterial.SetFloat(GlowOuterId, Mathf.Lerp(outerMin, outerMax, oscillation));
        runtimeMaterial.SetFloat(GlowPowerId, Mathf.Lerp(powerMin, powerMax, oscillation));
    }

    void OnDestroy()
    {
        if (runtimeMaterial == null)
            return;

        if (Application.isPlaying)
            Destroy(runtimeMaterial);
        else
            DestroyImmediate(runtimeMaterial);
    }

    void CreateRuntimeMaterial()
    {
        if (targetText == null || targetText.fontSharedMaterial == null || runtimeMaterial != null)
            return;

        runtimeMaterial = new Material(targetText.fontSharedMaterial)
        {
            name = $"{targetText.fontSharedMaterial.name} Runtime Glow"
        };

        runtimeMaterial.EnableKeyword("GLOW_ON");
        targetText.fontMaterial = runtimeMaterial;
        targetText.UpdateMeshPadding();
    }

    void ClampRanges()
    {
        innerMin = Mathf.Clamp(innerMin, 0f, 0.3f);
        innerMax = Mathf.Clamp(innerMax, 0f, 0.3f);
        if (innerMax < innerMin)
            innerMax = innerMin;

        outerMin = Mathf.Clamp01(outerMin);
        outerMax = Mathf.Clamp01(outerMax);
        if (outerMax < outerMin)
            outerMax = outerMin;

        powerMin = Mathf.Clamp(powerMin, 0.5f, 1f);
        powerMax = Mathf.Clamp(powerMax, 0.5f, 1f);
        if (powerMax < powerMin)
            powerMax = powerMin;
    }
}
