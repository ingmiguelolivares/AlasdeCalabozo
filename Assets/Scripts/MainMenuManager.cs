using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    /* ---------- Referencias UI ---------- */
    [Header("Botones")]
    public Button btnCrearCampaña;
    public Button btnBossFinal;
    public Button btnUnirse;
    public Button btnSinglePlayer;

    [Header("Campos de texto")]
    public InputField inputRoomID;
    public Text       txtRoomCode;
    public Text       txtEstado;

    /* ---------- Flags internos ---------- */
    bool pendingSinglePlayer = false;   // para saber que esperamos desconexión

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;

        btnCrearCampaña .onClick.AddListener(() => CrearCampaña(GameMode.Dragon));
        btnBossFinal    .onClick.AddListener(() => CrearCampaña(GameMode.FinalBoss));
        btnUnirse       .onClick.AddListener(UnirseACampaña);
        btnSinglePlayer .onClick.AddListener(PlaySingleplayer);

        txtRoomCode.gameObject.SetActive(false);
        txtEstado.text = "🔌 Conectando a Photon…";
    }

    /* ---------- Callbacks Photon ---------- */
    public override void OnConnectedToMaster()
    {
        txtEstado.text = "✅ Conectado a Photon. Elige tu modo.";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        txtEstado.text = "✅ Desconectado de Photon.";

        // Si veníamos del botón single-player, cargamos la escena local ahora
        if (pendingSinglePlayer)
        {
            pendingSinglePlayer = false;   // resetea el flag
            LoadSingleplayerScene();
        }
    }

    public override void OnCreatedRoom()
    {
        txtRoomCode.text = $"📘 Código de campaña: {PhotonNetwork.CurrentRoom.Name}";
        txtRoomCode.gameObject.SetActive(true);
        txtEstado.text = "✅ Campaña creada. Esperando jugadores…";
    }

    public override void OnJoinedRoom()
    {
        txtEstado.text = $"✅ Unido a campaña: {PhotonNetwork.CurrentRoom.Name}";
        CheckStart();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        txtEstado.text = $"👤 {newPlayer.NickName} se unió ({PhotonNetwork.CurrentRoom.PlayerCount}/3)";
        CheckStart();
    }

    public override void OnJoinRoomFailed(short code, string msg)  => txtEstado.text = $"❌ No se pudo unir: {msg}";
    public override void OnCreateRoomFailed(short code, string msg)=> txtEstado.text = $"❌ Error al crear sala: {msg}";

    /* ---------- Multijugador ---------- */
    void CrearCampaña(GameMode mode)
    {
        MatchSettings.Instance.mode = mode;
        string roomID = Random.Range(10000, 99999).ToString();
        RoomOptions op = new RoomOptions { MaxPlayers = 3 };

        PhotonNetwork.CreateRoom(roomID, op);
        txtEstado.text = $"⏳ Creando campaña ({(mode == GameMode.FinalBoss ? "Boss Final" : "Dragón")})…";
    }

    void UnirseACampaña()
    {
        string roomID = inputRoomID.text.Trim();
        if (string.IsNullOrEmpty(roomID))
        {
            txtEstado.text = "⚠ Ingresa un código válido.";
            return;
        }

        PhotonNetwork.JoinRoom(roomID);
        txtEstado.text = "⏳ Intentando unirse…";
    }

    void CheckStart()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 3 && PhotonNetwork.IsMasterClient)
        {
            txtEstado.text = "🚀 Cargando Lobby…";
            PhotonNetwork.LoadLevel("Lobby");
        }
    }

    /* ---------- SINGLEPLAYER ---------- */
    void PlaySingleplayer()
    {
        // Deshabilita los botones multijugador mientras se desconecta
        ToggleMultiplayerButtons(false);
        LoadSingleplayerScene();
        //ParameterCode revisar con conexión a red
        /*if (PhotonNetwork.IsConnected)
        {
            pendingSinglePlayer = true;      // señalamos que esperamos callback
            txtEstado.text = "🔌 Saliendo de Photon…";
            PhotonNetwork.Disconnect();
        }
        else
        {
            LoadSingleplayerScene();
        }*/
    }

    void LoadSingleplayerScene()
    {
        txtEstado.text = "🎮 Iniciando Single Player…";
        WebGLSceneLoader.Load(this, "SinglePlayer");   // ajusta el nombre si difiere
    }

    void ToggleMultiplayerButtons(bool enable)
    {
        btnCrearCampaña.interactable = enable;
        btnBossFinal   .interactable = enable;
        btnUnirse      .interactable = enable;
    }
}
