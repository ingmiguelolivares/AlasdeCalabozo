using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class SphinxQuestionManager : MonoBehaviourPunCallbacks
{
    public SphinxQuestionBank bank;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text questionText;  // Asegúrate de arrastrar aquí el mismo que se muestra en pantalla
    public Button trueBtn;
    public Button falseBtn;
    public Button attackBtn;
    public Button defendBtn;

    [Header("Refs")]
    public TurnBasedGame turnMgr;

    private bool _correct;
    private int _currentActor;

   void Start()
    {
        if (bank == null)
        {
            bank = Resources.Load<SphinxQuestionBank>("SphinxQuestionBank");
            if (bank == null)
            {
                Debug.LogError("❌ No se pudo cargar el banco desde Resources.");
            }
            else
            {
                Debug.Log($"✅ Banco cargado desde Resources: {bank.name}");
            }
        }
        else
        {
            Debug.Log($"✅ Banco asignado manualmente: {bank.name}");
        }

        if (questionText == null) Debug.LogWarning("⚠️ Falta referencia a questionText.");
        if (photonView == null) Debug.LogError("❌ No se encontró PhotonView en SphinxQuestionManager.");
    }


    public void StartQuestionPhase(int actorNumber)
    {
        _currentActor = actorNumber;
        bool isLocalTurn = PhotonNetwork.LocalPlayer.ActorNumber == actorNumber;
        panel.SetActive(isLocalTurn); // Solo el jugador actual ve el panel

        photonView.RPC(nameof(RpcSetActionButtons), RpcTarget.All, false);

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DelayAndSendQuestion());
        }
    }

    private IEnumerator DelayAndSendQuestion()
    {
        yield return new WaitForSeconds(0.5f); // pequeña espera para sincronizar

        int idx = Random.Range(0, bank.questions.Length);
        var qa = bank.questions[idx];
        photonView.RPC(nameof(RpcSendQuestion), RpcTarget.All, qa.question, qa.answer);
    }

    [PunRPC]
    void RpcSendQuestion(string q, bool answer)
    {
        questionText.text = q;
        _correct = answer;
        Debug.Log($"📘 Pregunta recibida: {q} (Respuesta: {answer})");
    }

    [PunRPC]
    void RpcHandleAnswer(int actorNumber, bool success)
    {
        if (PhotonNetwork.IsMasterClient)
            turnMgr.ResolveAnswer(actorNumber, success);

        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            panel.SetActive(false);

        if (success)
            photonView.RPC(nameof(RpcSetActionButtons), RpcTarget.All, true);
    }

    [PunRPC]
    void RpcSetActionButtons(bool enable)
    {
        if (attackBtn != null) attackBtn.interactable = enable;
        if (defendBtn != null) defendBtn.interactable = enable;
    }

    public void OnTruePressed() => SendAnswer(true);
    public void OnFalsePressed() => SendAnswer(false);

    private void SendAnswer(bool reply)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != _currentActor) return;
        bool success = reply == _correct;
        photonView.RPC(nameof(RpcHandleAnswer), RpcTarget.All, _currentActor, success);
    }
}
