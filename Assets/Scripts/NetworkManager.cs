using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    public Dictionary<int, string> playerNames = new Dictionary<int, string>();
    public bool listoParaUnirse = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (GetComponent<PhotonView>() == null)
        {
            Debug.LogError("❌ No hay un PhotonView en NetworkManager. Asegúrate de agregar uno manualmente en Unity.");
        }
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("🔄 Conectando a Photon...");

            // ✅ Forzar región US (elimina DevRegion, ya no existe)
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "us";

            // ✅ Conectar usando configuración actual
            PhotonNetwork.ConnectUsingSettings();

            // ✅ Activar logs detallados de red (opcional)
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.DebugOut = ExitGames.Client.Photon.DebugLevel.ALL;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Conectado a Photon Master Server.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("🎮 En el Lobby de Photon. Listo para crear o unirse a salas.");
        listoParaUnirse = true;
    }

    public void CrearCampaña(string codigo)
    {
        if (!listoParaUnirse)
        {
            Debug.LogWarning("⏳ Esperando conexión al lobby para crear campaña...");
            return;
        }

        RoomOptions opciones = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(codigo, opciones, TypedLobby.Default);
        Debug.Log("🏗️ Creando sala con código: " + codigo);
    }

    public void UnirseACampaña(string codigo)
    {
        if (!listoParaUnirse)
        {
            Debug.LogWarning("⏳ Aún no conectado al lobby. Espera un momento...");
            return;
        }

        PhotonNetwork.JoinRoom(codigo);
        Debug.Log("🔑 Intentando unirse a la sala con código: " + codigo);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"✅ {PhotonNetwork.LocalPlayer.NickName} se unió a la sala: {PhotonNetwork.CurrentRoom.Name}");

        var view = GetComponent<PhotonView>();
        if (view == null)
        {
            Debug.LogError("❌ No se encontró un PhotonView en NetworkManager.");
            return;
        }

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        string nick = PhotonNetwork.LocalPlayer.NickName;

        if (!playerNames.ContainsKey(actorNumber))
        {
            view.RPC("SyncPlayerName", RpcTarget.AllBuffered, actorNumber, nick);
        }
    }

    [PunRPC]
    void SyncPlayerName(int actorNumber, string nickname)
    {
        if (!playerNames.ContainsKey(actorNumber))
        {
            playerNames[actorNumber] = nickname;
        }
    }

    public string GetPlayerName(int actorNumber)
    {
        return playerNames.ContainsKey(actorNumber) ? playerNames[actorNumber] : "Jugador Desconocido";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("❌ No se pudo unir a la sala: " + message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"⚠️ Desconectado de Photon: {cause}. Reintentando conexión...");
        listoParaUnirse = false;

        // ✅ Reconexión forzada a región US
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "us";
        PhotonNetwork.ConnectUsingSettings();
    }
}
