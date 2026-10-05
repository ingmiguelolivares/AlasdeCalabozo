using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnBasedGame : MonoBehaviourPunCallbacks
{
    public static TurnBasedGame Instance;

    [Header("UI")]
    public Text turnText;
    public Text consoleText;
    public TMP_Text questionLogText;
    public Button attackButton, defendButton;
    public Slider warriorHealthSlider, archerHealthSlider, mageHealthSlider;
    public Slider enemyHealthSlider;

    [SerializeField] private SphinxQuestionManager sphinxMgr;
    const int SPHINX_HP = 111;
    const int SPHINX_DICE = 5;
    bool IsSphinxScene => SceneManager.GetActiveScene().name == "FinalBoss";

    int enemyHealth;
    readonly Dictionary<int, int> playerHP = new();
    readonly Dictionary<int, string> playerRole = new();
    List<Player> players = new();
    int currentTurnIndex;
    int currentMonster;

    bool isReady = false;
    int pendingTurn = -1;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Log("❌ No estás en una sala.");
            return;
        }

        attackButton.onClick.AddListener(() => TryAction(true));
        defendButton.onClick.AddListener(() => TryAction(false));

        StartCoroutine(InitGame());
    }

    IEnumerator InitGame()
    {
        yield return new WaitForSeconds(1f);

        players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToList();

        foreach (var p in players)
        {
            if (string.IsNullOrEmpty(p.NickName))
                p.NickName = $"Jugador_{p.ActorNumber}";

            string role = GetRole(p.ActorNumber);
            playerRole[p.ActorNumber] = role;
            playerHP[p.ActorNumber] = 38;  // Vida inicial 50
            SetSlider(p, 38, true);        // Slider con vida inicial
        }

        LogRoles();

        enemyHealth = IsSphinxScene ? SPHINX_HP : GetHPForDifficulty(0);
        enemyHealthSlider.maxValue = enemyHealth;
        enemyHealthSlider.value = enemyHealth;

        if (IsSphinxScene && sphinxMgr == null)
            sphinxMgr = FindFirstObjectByType<SphinxQuestionManager>();

        var modelsLoader = ModelsLoader.Instance;
        if (!IsSphinxScene && modelsLoader != null)
            yield return modelsLoader.EnsureModelsLoaded();

        isReady = true;
        if (pendingTurn != -1)
        {
            StartTurn(pendingTurn);
            pendingTurn = -1;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            currentTurnIndex = Random.Range(0, players.Count);
            photonView.RPC(nameof(SyncTurn), RpcTarget.AllBuffered, currentTurnIndex);

            if (!IsSphinxScene) SpawnMonster();
        }
    }

    [PunRPC]
    void SyncTurn(int index)
    {
        if (!isReady) { pendingTurn = index; return; }

        // Busca un jugador vivo desde el index recibido
        int startIdx = index;
        while (playerHP[players[index].ActorNumber] <= 0)
        {
            index = (index + 1) % players.Count;
            if (index == startIdx)
            {
                // Todos muertos, termina juego
                GameOver();
                return;
            }
        }

        StartTurn(index);
    }


    void StartTurn(int turnIdx)
    {
        int startIdx = turnIdx;
        while (playerHP[players[turnIdx].ActorNumber] <= 0)
        {
            turnIdx = (turnIdx + 1) % players.Count;
            if (turnIdx == startIdx)
            {
                // Todos muertos, termina juego
                GameOver();
                return;
            }
        }
        currentTurnIndex = turnIdx;


        if (AllPlayersDown()) { GameOver(); return; }

        Player p = players[currentTurnIndex];
        turnText.text = $"Turno J{currentTurnIndex + 1} – {p.NickName} ({playerRole[p.ActorNumber]})";
        Log($"🌀 Turno de {p.NickName}\n👹 {enemyHealth} HP\n{PlayersHP()}");

        bool myTurn = PhotonNetwork.LocalPlayer.ActorNumber == p.ActorNumber;
        SetButtons(myTurn && playerHP[p.ActorNumber] > 0);

        if (IsSphinxScene && myTurn && PhotonNetwork.IsMasterClient && sphinxMgr != null)
            sphinxMgr.StartQuestionPhase(p.ActorNumber);
    }

    int NextAliveFrom(int from)
    {
        for (int i = 1; i <= players.Count; i++)
        {
            int idx = (from + i) % players.Count;
            if (playerHP[players[idx].ActorNumber] > 0)
                return idx;
        }
        return from; // Si no hay nadie vivo, retorna el mismo índice (se terminará el juego)
    }


    [PunRPC]
    void RequestNextTurn()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        int next = NextAliveFrom(currentTurnIndex);
        photonView.RPC(nameof(SyncTurn), RpcTarget.AllBuffered, next);
    }


    void EnemyAttackPhase(int prevActor)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int attackerIdx = players.FindIndex(p => p.ActorNumber == prevActor);
        int targetIdx = attackerIdx;

        if (playerHP[players[targetIdx].ActorNumber] <= 0) return;

        int dice = IsSphinxScene ? SPHINX_DICE : GetDiceForDifficulty(currentMonster);
        int dmg = Random.Range(1, dice + 1);

        // --- Aquí bajamos el daño enemigo a la mitad ---
        dmg = Mathf.CeilToInt(dmg * 0.5f);

        int victimActor = players[targetIdx].ActorNumber;

        
        RequestNextTurn();
    }

    IEnumerator EnemyAttackDelayed(int actor)
    {
        yield return new WaitForSeconds(0.5f);
        EnemyAttackPhase(actor);
    }

 

    void TryAction(bool isAttack)
    {
        Player p = players[currentTurnIndex];
        if (PhotonNetwork.LocalPlayer != p) return;
        if (playerHP[p.ActorNumber] <= 0) { SetButtons(false); return; }

        SetButtons(false);

        if (!isAttack)
        {
            Log($"🛡 {p.NickName} se defiende y pasa el turno.");
            photonView.RPC(nameof(RequestNextTurn), RpcTarget.MasterClient);
            return;
        }

        int playerDice = GetPlayerDiceForDifficulty(currentMonster);
        int monsterDice = GetDiceForDifficulty(currentMonster);

        int toEnemy = Random.Range(1, playerDice + 1);
        int toPlayer = Random.Range(1, playerDice + 1);

        int dice = IsSphinxScene ? SPHINX_DICE : GetDiceForDifficulty(currentMonster);
        //int toEnemy = Random.Range(1, dice + 1);
        //int toPlayer = Random.Range(1, dice + 1);

        photonView.RPC(nameof(ResolveAttack), RpcTarget.AllBuffered,
                       p.ActorNumber, toEnemy, toPlayer,
                       IsSphinxScene ? -1 : currentMonster);
    }

    [PunRPC]
    void ResolveAttack(int actor, int toEnemy, int toPlayer, int monsterStage)
    {
        if (!IsSphinxScene && monsterStage != currentMonster) return;

        enemyHealth -= toEnemy;
        playerHP[actor] -= toPlayer;
        ClampHP(actor);

        enemyHealthSlider.value = enemyHealth;
        SetSlider(GetPlayer(actor), playerHP[actor]);

        Log($"⚔ {GetPlayer(actor).NickName} inflige {toEnemy} y recibe {toPlayer}\n👹 {enemyHealth} HP\n{PlayersHP()}");

        if (enemyHealth <= 0)
        {
            if (IsSphinxScene) WinGame();
            else MonsterDefeated();
            return;  // Ya murió el monstruo, no se avanza turno jugador
        }

        if (playerHP[actor] <= 0)
        {
            Log($"💀 {GetPlayer(actor).NickName} ha muerto.");

            if (AllPlayersDown())
            {
                GameOver();
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                int next = NextAliveFrom(currentTurnIndex);
                photonView.RPC(nameof(SyncTurn), RpcTarget.AllBuffered, next);
            }
        }
        else
        {
            // El jugador sigue vivo, el dragón ataca después
            if (PhotonNetwork.IsMasterClient)
                StartCoroutine(EnemyAttackDelayed(actor));
        }
    }


    public void ResolveAnswer(int actor, bool success)
    {
        if (success) return;

        photonView.RPC(nameof(SphinxWrongAnswerDamage), RpcTarget.AllBuffered, actor);

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(EnemyAttackDelayed(actor));
    }

    [PunRPC]
    void SphinxWrongAnswerDamage(int actor)
    {
        if (!IsSphinxScene) return;
        int dmg = Random.Range(1, SPHINX_DICE + 1);
        playerHP[actor] -= dmg; ClampHP(actor);
        SetSlider(GetPlayer(actor), playerHP[actor]);
        Log($"🟥 Respuesta incorrecta ► la Esfinge inflige {dmg} a {GetPlayer(actor).NickName}\n{PlayersHP()}");
    }

    void SpawnMonster()
    {
        var loader = ModelsLoader.Instance;
        if (loader == null) { Debug.LogError("❌ Falta ModelsLoader"); return; }

        GameObject[] src = currentMonster switch
        {
            0 => loader.easyModels,
            1 => loader.mediumModels,
            2 => loader.hardModels,
            _ => null
        };
        if (src == null || src.Length == 0) { Debug.LogError("❌ Array vacío"); return; }

        GameObject chosen = src[Random.Range(0, src.Length)];
        string prefabName = chosen.name;

        enemyHealth = GetHPForDifficulty(currentMonster);
        enemyHealthSlider.maxValue = enemyHealth;
        enemyHealthSlider.value = enemyHealth;

        loader.photonView.RPC("SpawnMonsterByName", RpcTarget.AllBuffered, currentMonster, prefabName);
    }

    void MonsterDefeated()
    {
        Log("🎉 ¡Monstruo derrotado!");
        currentMonster++;

        if (currentMonster >= 3)
        {
            WinGame();
            return;
        }

        SpawnMonster();

        // Reinicia el turno para el nuevo monstruo, 
        // y busca el primer jugador vivo para iniciar turno desde ahí.
        if (PhotonNetwork.IsMasterClient)
        {
            // Busca primer jugador vivo (podrías usar un random si prefieres)
            int firstAlive = -1;
            for (int i = 0; i < players.Count; i++)
            {
                if (playerHP[players[i].ActorNumber] > 0)
                {
                    firstAlive = i;
                    break;
                }
            }

            if (firstAlive == -1)
            {
                // No hay jugadores vivos, termina juego
                GameOver();
                return;
            }

            currentTurnIndex = firstAlive;
            photonView.RPC(nameof(SyncTurn), RpcTarget.AllBuffered, currentTurnIndex);
        }
    }



    void WinGame()
    {
        Log("🏆 ¡Victoria total!");
        SetButtons(false);

        if (PhotonNetwork.IsMasterClient)
        {
            string scene = IsSphinxScene ? "Fin" : "Victoria";
            photonView.RPC(nameof(LoadEndScene), RpcTarget.All, scene);
        }
    }

    void GameOver()
    {
        Log("☠️ Todos los jugadores han caído. GAME OVER.");
        SetButtons(false);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(LoadEndScene), RpcTarget.All, "Perdiste");
        }
    }

    IEnumerator ExitAfter(float t, string scene)
    {
        yield return new WaitForSeconds(t);
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.Disconnect();
        WebGLSceneLoader.Load(this, scene);
    }

    [PunRPC]
    void LoadEndScene(string sceneName)
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        StartCoroutine(LoadSceneAfterDelay(sceneName));
    }

    IEnumerator LoadSceneAfterDelay(string scene)
    {
        yield return new WaitForSeconds(2f);
        PhotonNetwork.LeaveRoom();
        yield return new WaitForSeconds(1f);
        yield return WebGLSceneLoader.LoadRoutine(scene);
    }

    // Función nueva que devuelve HP según dificultad
    int GetHPForDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 0: return 18;   // Fácil
            case 1: return 36;  // Medio
            case 2: return 60;  // Difícil
            default: return 10;
        }
    }

    // Función similar para dado (dice) según dificultad (modificada para daño más bajo)
    int GetDiceForDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 0: return 6;   // Fácil - menos daño
            case 1: return 12;   // Medio - menos daño
            case 2: return 20;   // Difícil - menos daño
            default: return 2;
        }
    }

    int GetPlayerDiceForDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 0: return 6;   // Fácil
            case 1: return 12;   // Medio
            case 2: return 20;  // Difícil
            default: return 4;
        }
    }


    string GetRole(int actor) =>
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue($"Role_{actor}", out var r)
            ? (string)r : "Desconocido";

    Player GetPlayer(int actor) => players.FirstOrDefault(p => p.ActorNumber == actor);

    void ClampHP(int actor) { if (playerHP[actor] < 0) playerHP[actor] = 0; }

    bool AllPlayersDown()
    {
        if (playerHP.Count < players.Count) return false;
        return playerHP.Values.All(hp => hp <= 0);
    }

    string PlayersHP() => string.Join("\n", players.Select(p => $"{p.NickName}: {playerHP[p.ActorNumber]} HP"));

    void SetSlider(Player p, int value, bool setMax = false)
    {
        if (p == null) return;

        string roleKey = playerRole.ContainsKey(p.ActorNumber) ? playerRole[p.ActorNumber].ToLower() : "";
        Slider s = roleKey switch
        {
            "guerrero" => warriorHealthSlider,
            "arquero" => archerHealthSlider,
            "hechicero" => mageHealthSlider,
            _ => null
        };

        if (s == null) return;
        if (setMax) s.maxValue = 38;
        s.value = value;
    }

    void SetButtons(bool enable)
    {
        attackButton.interactable = enable;
        defendButton.interactable = !IsSphinxScene && enable;
    }

    void LogRoles()
    {
        string map = string.Join("\n", players.Select(
            (p, i) => $"J{i + 1} ► {p.NickName} – {playerRole[p.ActorNumber]}"));
        Log($"🗺️ Orden de turnos:\n{map}");
    }

    void Log(string m)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(m);
#endif
        if (consoleText) consoleText.text = m;
        if (questionLogText && IsSphinxScene) questionLogText.text = m;
    }
}
