using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance;

    public Button warriorButton, witcherButton, archerButton, startGameButton;
    public Text statusText;

    private Dictionary<int, string> assignedRoles = new Dictionary<int, string>();
    private const string ROLE_KEY_PREFIX = "Role_";
    private const string GAME_STARTED_KEY = "GameStarted";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("❌ No estás en una sala.");
            return;
        }

        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }

    public void SelectWarrior()  { SelectRole("Guerrero"); }
    public void SelectWitcher()  { SelectRole("Hechicero"); }
    public void SelectArcher()   { SelectRole("Arquero"); }
    public void StartMultiplayerGame() { StartGame(); }

    private void SelectRole(string role)
    {
        if (!PhotonNetwork.InRoom) return;

        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (assignedRoles.ContainsKey(myActorNumber))
        {
            Debug.LogWarning($"⚠ Ya seleccionaste un rol: {assignedRoles[myActorNumber]}");
            return;
        }

        if (assignedRoles.ContainsValue(role))
        {
            Debug.LogWarning("⚠ Este rol ya ha sido seleccionado por otro jugador.");
            return;
        }

        Hashtable roleData = new Hashtable
        {
            [ROLE_KEY_PREFIX + myActorNumber] = role
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(roleData);

        statusText.text = $"🔹 Has seleccionado: {role}";
        Debug.Log($"✅ Jugador {myActorNumber} ha elegido {role}");
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        foreach (var key in propertiesThatChanged.Keys)
        {
            if (key.ToString().StartsWith(ROLE_KEY_PREFIX))
            {
                if (int.TryParse(key.ToString().Replace(ROLE_KEY_PREFIX, ""), out int playerID))
                {
                    assignedRoles[playerID] = (string)propertiesThatChanged[key];
                }
            }
        }

        if (propertiesThatChanged.ContainsKey(GAME_STARTED_KEY))
        {
            GameMode modo = GameMode.Dragon; // por defecto

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameMode"))
            {
                modo = (GameMode)(int)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"];
            }

            string escenaFinal = modo == GameMode.FinalBoss
                ? "FinalBoss"
                : "multplayertest02";

            Debug.Log("🎲 ¡El juego ha comenzado! Cargando escena: " + escenaFinal);
            PhotonNetwork.LoadLevel(escenaFinal);
        }
    }

    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.LogWarning("⚠ Se necesitan al menos 2 jugadores para iniciar.");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount != assignedRoles.Count)
        {
            Debug.LogWarning("⚠ Todos los jugadores deben elegir un rol antes de comenzar.");
            return;
        }

        GameMode modoElegido = MatchSettings.Instance.mode;

        Hashtable gameStartData = new Hashtable
        {
            [GAME_STARTED_KEY] = true,
            ["GameMode"] = (int)modoElegido
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(gameStartData);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        assignedRoles.Remove(otherPlayer.ActorNumber);
    }
}
