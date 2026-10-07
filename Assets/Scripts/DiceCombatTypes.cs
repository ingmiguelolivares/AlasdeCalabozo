using System;
using UnityEngine;

public enum PrecisionQuality
{
    Mala,
    Normal,
    Buena,
    Perfecta,
    FalloCritico
}

public enum AttackRiskStyle
{
    Seguro,
    Normal,
    Arriesgado
}

[Serializable]
public class PrecisionZone
{
    [Range(0f, 1f)] public float min = 0.4f;
    [Range(0f, 1f)] public float max = 0.6f;
    public PrecisionQuality quality = PrecisionQuality.Normal;
    public float multiplier = 1f;

    public bool Contains(float value)
    {
        return value >= Mathf.Min(min, max) && value <= Mathf.Max(min, max);
    }
}

[Serializable]
public class DiceBattleConfig
{
    public string label = "D6";
    public int sides = 6;
    public float sliderSpeed = 1.2f;
    public float enemySpinSeconds = 1f;
    public float throwSeconds = 0.55f;
    public float criticalFailureMultiplier = 0f;
    public PrecisionZone[] precisionZones;

    public void EnsureDefaults(int battleIndex)
    {
        if (sides <= 0)
            sides = battleIndex == 0 ? 6 : battleIndex == 1 ? 12 : 20;

        if (string.IsNullOrWhiteSpace(label))
            label = "D" + sides;

        if (sliderSpeed <= 0f)
            sliderSpeed = battleIndex == 0 ? 1.1f : battleIndex == 1 ? 1.7f : 2.25f;

        if (enemySpinSeconds <= 0f)
            enemySpinSeconds = battleIndex == 0 ? 0.9f : battleIndex == 1 ? 1.15f : 1.4f;

        if (throwSeconds <= 0f)
            throwSeconds = 0.55f;

        if (precisionZones == null || precisionZones.Length == 0)
            precisionZones = CreateDefaultZones(battleIndex);
    }

    static PrecisionZone[] CreateDefaultZones(int battleIndex)
    {
        if (battleIndex == 0)
        {
            return new[]
            {
                Zone(0f, 1f, PrecisionQuality.Mala, 0.75f),
                Zone(0.2f, 0.8f, PrecisionQuality.Normal, 1f),
                Zone(0.35f, 0.65f, PrecisionQuality.Buena, 1.25f),
                Zone(0.46f, 0.54f, PrecisionQuality.Perfecta, 1.5f)
            };
        }

        if (battleIndex == 1)
        {
            return new[]
            {
                Zone(0f, 1f, PrecisionQuality.Mala, 0.75f),
                Zone(0.28f, 0.72f, PrecisionQuality.Normal, 1f),
                Zone(0.42f, 0.58f, PrecisionQuality.Buena, 1.25f),
                Zone(0.485f, 0.515f, PrecisionQuality.Perfecta, 1.5f)
            };
        }

        return new[]
        {
            Zone(0f, 1f, PrecisionQuality.Mala, 0.75f),
            Zone(0.34f, 0.66f, PrecisionQuality.Normal, 1f),
            Zone(0.455f, 0.545f, PrecisionQuality.Buena, 1.25f),
            Zone(0.495f, 0.505f, PrecisionQuality.Perfecta, 1.5f)
        };
    }

    static PrecisionZone Zone(float min, float max, PrecisionQuality quality, float multiplier)
    {
        return new PrecisionZone { min = min, max = max, quality = quality, multiplier = multiplier };
    }
}

public struct DiceRollResult
{
    public int sides;
    public int roll;
    public float precisionValue;
    public PrecisionQuality quality;
    public float multiplier;
    public int finalDamage;
    public bool criticalFailure;

    public override string ToString()
    {
        return string.Format("D{0}={1}, precision={2:0.00}, calidad={3}, x{4:0.00}, daño={5}",
            sides, roll, precisionValue, quality, multiplier, finalDamage);
    }
}
