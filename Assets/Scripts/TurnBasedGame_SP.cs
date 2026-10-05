using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;  // <-- ¡Importante para cargar escenas!
using FMODUnity;
using System.Collections;

/// Combate local single-player: 1 jugador vs 3 monstruos
/// HP jugador = 38 ; HP monstruos = 6, 12, 20 ; solo botón ATACAR.
public class TurnBasedGame_SP : MonoBehaviour
{
    /* ---------- UI ---------- */
    [Header("UI")]
    public Button attackButton;
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;
    public TMP_Text logText;

    [Header("Efectos de Combate")]
    public PanelActivador panelDaño;
    public EventReference golpePocoEfectivo;
    public EventReference golpeNormal;
    public EventReference golpeMuyEfectivo;

    /* ---------- Modelos ---------- */
    public ModelsLoader modelsLoader;

    /* ---------- Datos de combate ---------- */
    int playerHP = 38;
    readonly int[] monsterHP = { 6, 12, 20 };
    readonly int[] monsterDice = { 6, 12, 20 };
    int currentMonster = 0;
    int enemyHP = 0;

    /* ---------- Inicio ---------- */
    IEnumerator Start()
    {
        playerHealthSlider.maxValue = playerHP;
        playerHealthSlider.value = playerHP;

        attackButton.onClick.AddListener(PlayerAttack);

        var loader = ModelsLoader.Instance;
        if (loader != null)
            yield return loader.EnsureModelsLoaded();

        SpawnMonster();
    }

    /* ---------- Acción jugador ---------- */
    void PlayerAttack()
    {
        if (playerHP <= 0) return;

        int diceMax = monsterDice[currentMonster];
        int dmgToEnemy = Random.Range(1, diceMax + 1);
        int dmgToPlayer = Random.Range(1, diceMax + 1);

        enemyHP -= dmgToEnemy;
        playerHP -= dmgToPlayer;

        if (enemyHP < 0) enemyHP = 0;
        if (playerHP < 0) playerHP = 0;

        enemyHealthSlider.value = enemyHP;
        playerHealthSlider.value = playerHP;

        Log($"Atacas por {dmgToEnemy} y recibes {dmgToPlayer}\n {playerHP} HP  |  {enemyHP} HP");

        // Mostrar panel de daño con intensidad
        if (panelDaño != null)
            panelDaño.MostrarPanelConDaño(dmgToPlayer, diceMax);

        // Reproducir sonido según fuerza del golpe
        float porcentaje = (float)dmgToPlayer / diceMax;
        if (porcentaje < 0.33f)
            RuntimeManager.PlayOneShot(golpePocoEfectivo);
        else if (porcentaje < 0.66f)
            RuntimeManager.PlayOneShot(golpeNormal);
        else
            RuntimeManager.PlayOneShot(golpeMuyEfectivo);

        if (enemyHP <= 0) MonsterDefeated();
        else if (playerHP <= 0) GameOver();
    }


    /* ---------- Instanciar monstruo ---------- */
    void SpawnMonster()
    {
        var loader = ModelsLoader.Instance;
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

    }

    void MonsterDefeated()
    {
        Log("¡Monstruo derrotado! ¡Siguiente Calabozo!");
        currentMonster++;

        if (currentMonster >= monsterHP.Length)
        {
            Log("¡Has vencido a los 3 monstruos! ¡Victoria total!");
            attackButton.interactable = false;
            WebGLSceneLoader.Load(this, "Victoria"); // <-- Cambia a la escena de victoria
        }
        else
        {
            SpawnMonster();
        }
    }

    void GameOver()
    {
        Log("Has sido derrotado. GAME OVER.");
        attackButton.interactable = false;
        WebGLSceneLoader.Load(this, "Perdiste"); // <-- Cambia a la escena de derrota
    }

    /* ---------- Helper ---------- */
    void Log(string msg)
    {
        Debug.Log(msg);
        if (logText) logText.text = msg;
    }
}
