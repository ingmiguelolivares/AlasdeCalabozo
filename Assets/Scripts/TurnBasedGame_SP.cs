using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using System.Collections;

[RequireComponent(typeof(DiceThrowController))]
public class TurnBasedGame_SP : MonoBehaviour
{
    enum CombatInputState { Ready, Aiming, Resolving, Reward }

    [Header("UI")]
    public Button attackButton;
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;
    public TMP_Text logText;

    [Header("Dados y precision")]
    [SerializeField] PrecisionSliderController precisionSlider;
    [SerializeField] TMP_Text diceResultText;
    [SerializeField] TMP_Text rewardText;
    [SerializeField] DiceBattleConfig[] battleConfigs;
    [SerializeField] AttackRiskStyle attackRiskStyle = AttackRiskStyle.Normal;
    [SerializeField] float diceSpinSpeed = 520f;
    [SerializeField] float defenseGoodMin = 0.42f;
    [SerializeField] float defenseGoodMax = 0.58f;
    [SerializeField] float defenseGoodReduction = 0.5f;
    [SerializeField] float defenseBadReduction = 0f;
    [SerializeField] DiceThrowController diceThrowController;

    [Header("Efectos de Combate")]
    public PanelActivador panelDaño;
    public EventReference golpePocoEfectivo;
    public EventReference golpeNormal;
    public EventReference golpeMuyEfectivo;

    [Header("Modelos")]
    public ModelsLoader modelsLoader;

    int playerHP = 38;
    readonly int[] monsterHP = { 6, 12, 20 };
    readonly int[] monsterDice = { 6, 12, 20 };
    int currentMonster;
    int enemyHP;

    CombatInputState inputState = CombatInputState.Ready;
    readonly Transform[] diceByBattle = new Transform[3];
    Transform activeDie;
    EnemyDicePreviewController enemyDicePreviewController;
    BattleRewardController battleRewardController;
    bool defenseWindowOpen;
    float pendingDefenseReduction;

    IEnumerator Start()
    {
        EnsureBattleConfigs();
        EnsureRuntimeHelpers();
        CacheSceneDice();

        playerHealthSlider.maxValue = playerHP;
        playerHealthSlider.value = playerHP;

        attackButton.onClick.RemoveListener(PlayerAttack);
        attackButton.onClick.AddListener(OnAttackButtonPressed);
        SetAttackButtonText("Preparar dado");

        if (precisionSlider != null) precisionSlider.Hide();

        ModelsLoader loader = ModelsLoader.Instance;
        if (loader != null)
            yield return loader.EnsureModelsLoaded(currentMonster);

        SpawnMonster();
    }

    void PlayerAttack()
    {
        OnAttackButtonPressed();
    }

    void OnAttackButtonPressed()
    {
        if (defenseWindowOpen)
        {
            OnDefenseButtonPressed();
            return;
        }

        if (playerHP <= 0 || inputState == CombatInputState.Resolving)
            return;

        if (inputState == CombatInputState.Ready)
        {
            BeginPlayerAim();
            return;
        }

        if (inputState == CombatInputState.Aiming)
        {
            float precision = precisionSlider != null ? precisionSlider.Stop() : 0.5f;
            StartCoroutine(ResolvePlayerAttack(precision));
        }
    }

    void BeginPlayerAim()
    {
        inputState = CombatInputState.Aiming;
        DiceBattleConfig config = GetCurrentConfig();
        if (precisionSlider != null)
            precisionSlider.Begin(GetRiskAdjustedSliderSpeed(config.sliderSpeed));

        SetAttackButtonText("Lanzar dado");
        Log("Prepara el lanzamiento: sincroniza el Foco de Poder para potenciar el dado.");
    }

    IEnumerator ResolvePlayerAttack(float precision)
    {
        inputState = CombatInputState.Resolving;
        attackButton.interactable = false;

        DiceBattleConfig config = GetCurrentConfig();
        int roll = Random.Range(1, config.sides + 1);
        DiceRollResult result = BuildRollResult(config, roll, precision);

        if (diceResultText != null)
            diceResultText.text = result.roll.ToString();

        Log("[Jugador] " + result);

        Transform target = GetEnemyTarget();
        if (diceThrowController != null && activeDie != null)
            yield return diceThrowController.Throw(activeDie, target, config.throwSeconds, diceSpinSpeed);

        enemyHP -= result.finalDamage;
        if (enemyHP < 0) enemyHP = 0;
        enemyHealthSlider.value = enemyHP;

        ShowHitFeedback(result.finalDamage, config.sides, result.quality);
        Log(string.Format("Resultado dado: {0}\nCalidad: {1} x{2:0.00}\nDaño final: {3}\nHP jugador {4} | HP enemigo {5}",
            result.roll, result.quality, result.multiplier, result.finalDamage, playerHP, enemyHP));

        if (enemyHP <= 0)
        {
            yield return MonsterDefeatedRoutine();
            yield break;
        }

        yield return EnemyAttackRoutine();

        if (playerHP <= 0)
        {
            GameOver();
            yield break;
        }

        inputState = CombatInputState.Ready;
        SetAttackButtonText("Preparar dado");
        attackButton.interactable = true;
    }

    IEnumerator EnemyAttackRoutine()
    {
        DiceBattleConfig config = GetCurrentConfig();
        int enemyRoll = Random.Range(1, config.sides + 1);

        defenseWindowOpen = true;
        pendingDefenseReduction = defenseBadReduction;
        if (precisionSlider != null)
            precisionSlider.Begin(config.sliderSpeed * 1.25f);
        attackButton.interactable = true;
        SetAttackButtonText("Defender");

        Log("El enemigo prepara su dado. Activa Defender cuando el Foco de Poder este alineado para reducir daño.");

        if (enemyDicePreviewController != null)
            yield return enemyDicePreviewController.SpinAndReveal(activeDie, enemyRoll, config.enemySpinSeconds, diceSpinSpeed);
        else
            yield return new WaitForSeconds(config.enemySpinSeconds);

        defenseWindowOpen = false;
        attackButton.interactable = false;
        if (precisionSlider != null)
            precisionSlider.Hide();

        int dmgToPlayer = Mathf.Max(0, Mathf.CeilToInt(enemyRoll * (1f - pendingDefenseReduction)));
        playerHP -= dmgToPlayer;
        if (playerHP < 0) playerHP = 0;
        playerHealthSlider.value = playerHP;

        Log(string.Format("[Enemigo] D{0}={1}, defensa reduce {2:P0}, daño final {3}\nHP jugador {4} | HP enemigo {5}",
            config.sides, enemyRoll, pendingDefenseReduction, dmgToPlayer, playerHP, enemyHP));

        if (panelDaño != null)
            panelDaño.MostrarPanelConDaño(dmgToPlayer, config.sides);

        float porcentaje = config.sides > 0 ? (float)dmgToPlayer / config.sides : 0f;
        if (porcentaje < 0.33f)
            RuntimeManager.PlayOneShot(golpePocoEfectivo);
        else if (porcentaje < 0.66f)
            RuntimeManager.PlayOneShot(golpeNormal);
        else
            RuntimeManager.PlayOneShot(golpeMuyEfectivo);
    }

    void OnDefenseButtonPressed()
    {
        if (!defenseWindowOpen)
            return;

        float value = precisionSlider != null ? precisionSlider.Stop() : 0f;
        bool good = value >= Mathf.Min(defenseGoodMin, defenseGoodMax) && value <= Mathf.Max(defenseGoodMin, defenseGoodMax);
        pendingDefenseReduction = good ? defenseGoodReduction : defenseBadReduction;
        defenseWindowOpen = false;
        attackButton.interactable = false;
        if (precisionSlider != null)
            precisionSlider.Hide();

        Log(good ? "Defensa precisa: daño enemigo reducido." : "Defensa imprecisa: casi no reduces el golpe.");
    }

    DiceRollResult BuildRollResult(DiceBattleConfig config, int roll, float precision)
    {
        DiceRollResult result = new DiceRollResult();
        result.sides = config.sides;
        result.roll = roll;
        result.precisionValue = precision;

        PrecisionZone zone = EvaluatePrecision(config, precision);
        result.quality = zone != null ? zone.quality : PrecisionQuality.Mala;
        result.multiplier = zone != null ? zone.multiplier : 0.75f;

        ApplyRiskStyle(ref result);

        result.criticalFailure = roll == 1;
        if (result.criticalFailure)
        {
            result.quality = PrecisionQuality.FalloCritico;
            result.multiplier = config.criticalFailureMultiplier;
        }

        result.finalDamage = Mathf.Max(0, Mathf.RoundToInt(roll * result.multiplier));
        return result;
    }

    PrecisionZone EvaluatePrecision(DiceBattleConfig config, float precision)
    {
        PrecisionZone best = null;
        if (config.precisionZones == null)
            return null;

        for (int i = 0; i < config.precisionZones.Length; i++)
        {
            PrecisionZone zone = config.precisionZones[i];
            if (zone != null && zone.Contains(precision))
                best = zone;
        }

        return best;
    }

    void ApplyRiskStyle(ref DiceRollResult result)
    {
        if (attackRiskStyle == AttackRiskStyle.Seguro)
            result.multiplier = Mathf.Min(result.multiplier, 1.25f);
        else if (attackRiskStyle == AttackRiskStyle.Arriesgado)
        {
            if (result.quality == PrecisionQuality.Perfecta)
                result.multiplier += 0.25f;
            else if (result.quality == PrecisionQuality.Mala)
                result.multiplier = Mathf.Min(result.multiplier, 0.5f);
        }
    }

    float GetRiskAdjustedSliderSpeed(float baseSpeed)
    {
        if (attackRiskStyle == AttackRiskStyle.Seguro)
            return baseSpeed * 0.8f;
        if (attackRiskStyle == AttackRiskStyle.Arriesgado)
            return baseSpeed * 1.25f;
        return baseSpeed;
    }

    void SpawnMonster()
    {
        ModelsLoader loader = ModelsLoader.Instance;
        if (loader == null) { Debug.LogError("Falta ModelsLoader"); return; }

        GameObject[] src = currentMonster switch
        {
            0 => loader.easyModels,
            1 => loader.mediumModels,
            2 => loader.hardModels,
            _ => null
        };
        if (src == null || src.Length == 0)
        {
            Debug.LogError("No hay modelos para esta dificultad.");
            return;
        }

        GameObject chosen = src[Random.Range(0, src.Length)];
        string prefabName = chosen.name;

        enemyHP = monsterHP[currentMonster];
        enemyHealthSlider.maxValue = enemyHP;
        enemyHealthSlider.value = enemyHP;

        loader.SpawnMonsterByName(currentMonster, prefabName);
        SelectBattleDie(currentMonster);
        inputState = CombatInputState.Ready;
        SetAttackButtonText("Preparar dado");
        attackButton.interactable = true;
    }

    IEnumerator MonsterDefeatedRoutine()
    {
        inputState = CombatInputState.Reward;
        attackButton.interactable = false;
        SetAttackButtonText("Continuar");

        bool finalMonsterDefeated = currentMonster >= monsterHP.Length - 1;
        Log(finalMonsterDefeated
            ? "¡Monstruo derrotado! Carta de ave desbloqueada."
            : "¡Monstruo derrotado! Has recibido una pista de ave.");

        if (battleRewardController != null)
            yield return battleRewardController.GrantReward(currentMonster, finalMonsterDefeated);

        currentMonster++;

        if (currentMonster >= monsterHP.Length)
        {
            Log("¡Has vencido a los 3 monstruos! ¡Victoria total!");
            attackButton.interactable = false;
            WebGLSceneLoader.Load(this, "Victoria");
        }
        else
        {
            ModelsLoader loader = ModelsLoader.Instance;
            if (loader != null)
                yield return loader.EnsureModelsLoaded(currentMonster);

            SpawnMonster();
        }
    }

    void GameOver()
    {
        Log("Has sido derrotado. GAME OVER.");
        attackButton.interactable = false;
        WebGLSceneLoader.Load(this, "Perdiste");
    }

    void Log(string msg)
    {
        Debug.Log(msg);
        if (logText) logText.text = msg;
    }

    void EnsureBattleConfigs()
    {
        if (battleConfigs == null || battleConfigs.Length < 3)
            battleConfigs = new DiceBattleConfig[3];

        for (int i = 0; i < 3; i++)
        {
            if (battleConfigs[i] == null)
                battleConfigs[i] = new DiceBattleConfig();

            battleConfigs[i].sides = monsterDice[i];
            battleConfigs[i].label = "D" + monsterDice[i];
            battleConfigs[i].EnsureDefaults(i);
        }
    }

    DiceBattleConfig GetCurrentConfig()
    {
        EnsureBattleConfigs();
        return battleConfigs[Mathf.Clamp(currentMonster, 0, battleConfigs.Length - 1)];
    }

    void EnsureRuntimeHelpers()
    {
        if (diceThrowController == null)
            diceThrowController = GetComponent<DiceThrowController>();
        if (diceThrowController == null)
            diceThrowController = gameObject.AddComponent<DiceThrowController>();

        if (enemyDicePreviewController == null)
            enemyDicePreviewController = GetComponent<EnemyDicePreviewController>();
        if (enemyDicePreviewController == null)
            enemyDicePreviewController = gameObject.AddComponent<EnemyDicePreviewController>();

        if (battleRewardController == null)
            battleRewardController = GetComponent<BattleRewardController>();
        if (battleRewardController == null)
            battleRewardController = gameObject.AddComponent<BattleRewardController>();

        EnsurePrecisionUI();
    }

    void EnsurePrecisionUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        if (precisionSlider == null)
            precisionSlider = CreateSlider(canvas.transform, "PrecisionSlider", new Vector2(0f, -210f));
        if (diceResultText == null)
            diceResultText = CreateText(canvas.transform, "DiceResultText", new Vector2(0f, -310f), 32);
        if (rewardText == null)
            rewardText = CreateText(canvas.transform, "BirdRewardText", new Vector2(0f, 120f), 24);

        battleRewardController.SetRewardText(rewardText);
        enemyDicePreviewController.SetResultText(diceResultText);
    }

    PrecisionSliderController CreateSlider(Transform parent, string name, Vector2 anchoredPosition)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(360f, 28f);
        rect.anchoredPosition = anchoredPosition;
        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        return go.AddComponent<PrecisionSliderController>();
    }

    TMP_Text CreateText(Transform parent, string name, Vector2 anchoredPosition, int size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(620f, 90f);
        rect.anchoredPosition = anchoredPosition;
        TMP_Text text = go.GetComponent<TMP_Text>();
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.text = "";
        return text;
    }

    void CacheSceneDice()
    {
        GameObject diceRootObject = GameObject.Find("Dados");
        if (diceRootObject == null)
        {
            Debug.LogWarning("[DiceCombat] No se encontro ModelPreviewEnvironment > PreviewModelRoot > Dados.");
            return;
        }

        Transform diceRoot = diceRootObject.transform;
        Transform[] children = diceRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == diceRoot)
                continue;

            string lower = child.name.ToLowerInvariant();
            if (diceByBattle[0] == null && (lower.Contains("d06") || lower.Contains("d6") || lower.Contains("6")))
                diceByBattle[0] = child;
            else if (diceByBattle[1] == null && (lower.Contains("d12") || lower.Contains("12")))
                diceByBattle[1] = child;
            else if (diceByBattle[2] == null && (lower.Contains("d20") || lower.Contains("20")))
                diceByBattle[2] = child;
        }

        int fallbackIndex = 0;
        foreach (Transform child in diceRoot)
        {
            if (fallbackIndex < diceByBattle.Length && diceByBattle[fallbackIndex] == null)
                diceByBattle[fallbackIndex] = child;
            fallbackIndex++;
        }

        SelectBattleDie(0);
    }

    void SelectBattleDie(int battleIndex)
    {
        for (int i = 0; i < diceByBattle.Length; i++)
        {
            if (diceByBattle[i] != null)
                diceByBattle[i].gameObject.SetActive(i == battleIndex);
        }

        activeDie = battleIndex >= 0 && battleIndex < diceByBattle.Length ? diceByBattle[battleIndex] : null;
        Debug.Log(activeDie != null
            ? "[DiceCombat] Dado activo batalla " + (battleIndex + 1) + ": " + activeDie.name
            : "[DiceCombat] No hay dado asignado para batalla " + (battleIndex + 1));
    }

    Transform GetEnemyTarget()
    {
        if (modelsLoader != null && modelsLoader.spawnPoint != null && modelsLoader.spawnPoint.childCount > 0)
            return modelsLoader.spawnPoint.GetChild(0);

        ModelsLoader loader = ModelsLoader.Instance;
        if (loader != null && loader.spawnPoint != null)
            return loader.spawnPoint;

        return transform;
    }

    void ShowHitFeedback(int damage, int maxDamage, PrecisionQuality quality)
    {
        if (panelDaño != null)
            panelDaño.MostrarPanelConDaño(damage, Mathf.Max(1, maxDamage));

    }

    void SetAttackButtonText(string text)
    {
        if (attackButton == null)
            return;

        TMP_Text tmp = attackButton.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
            return;
        }

        Text legacy = attackButton.GetComponentInChildren<Text>();
        if (legacy != null)
            legacy.text = text;
    }
}
