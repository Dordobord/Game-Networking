using System;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerScoreData : INetworkSerializable, IEquatable<PlayerScoreData>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Kills;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Kills);
    }

    public bool Equals(PlayerScoreData other)
    {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName) && Kills == other.Kills;
    }
}

public class GameManager : NetworkBehaviour
{
    public static GameManager main;

    [Header("Win Condition")]
    [SerializeField] private int killsToWin = 10;

    [Header("End Game UI")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField, Min(1)] private int returnToLobbyDelay = 10;

    public NetworkList<PlayerScoreData> PlayerScores = new NetworkList<PlayerScoreData>();

    private bool gameEnded;
    private Coroutine returnToLobbyCoroutine;

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        HideEndGameUI();
    }

    private void OnDestroy()
    {
        if (main == this)
            main = null;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                AddPlayerEntry(clientId);
            }

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        AddPlayerEntry(clientId);
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < PlayerScores.Count; i++)
        {
            if (PlayerScores[i].ClientId == clientId)
            {
                PlayerScores.RemoveAt(i);
                break;
            }
        }
    }

    private void AddPlayerEntry(ulong clientId)
    {
        foreach (PlayerScoreData entry in PlayerScores)
        {
            if (entry.ClientId == clientId)
                return; // already registered, don't duplicate
        }

        PlayerScores.Add(new PlayerScoreData
        {
            ClientId = clientId,
            PlayerName = "Player " + clientId,
            Kills = 0
        });
    }

    public void AddPoint(ulong attackerId)
    {
        if (!IsServer || gameEnded)
            return;

        for (int i = 0; i < PlayerScores.Count; i++)
        {
            if (PlayerScores[i].ClientId == attackerId)
            {
                PlayerScoreData updated = PlayerScores[i];
                updated.Kills++;
                PlayerScores[i] = updated; 

                CheckWinner(updated);
                break;
            }
        }
    }

    private void CheckWinner(PlayerScoreData scorer)
    {
        if (scorer.Kills < killsToWin)
            return;

        gameEnded = true;

        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(
            FindObjectsSortMode.None
        );

        foreach (PlayerHealth player in players)
            player.PauseForRoundEnd();

        EndGameClientRpc(scorer.ClientId, scorer.PlayerName.ToString());
        returnToLobbyCoroutine = StartCoroutine(ReturnToLobbyCountdown());
    }

    [ClientRpc]
    private void EndGameClientRpc(ulong winnerClientId, string winnerName)
    {
        SetLocalGameplayEnabled(false);

        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        if (winnerText != null)
        {
            bool localPlayerWon =
                NetworkManager.Singleton.LocalClientId == winnerClientId;

            winnerText.text = localPlayerWon ? "YOU WIN!" : $"YOU LOSE!\n{winnerName} Wins";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator ReturnToLobbyCountdown()
    {
        for (int seconds = returnToLobbyDelay; seconds > 0; seconds--)
        {
            UpdateLobbyCountdownClientRpc(seconds);
            yield return new WaitForSeconds(1f);
        }

        returnToLobbyCoroutine = null;

        MultiplayerGameFlow gameFlow =
            NetworkManager.Singleton.GetComponent<MultiplayerGameFlow>();

        if (gameFlow != null)
            gameFlow.ReturnToLobby();
        else
            Debug.LogError("MultiplayerGameFlow was not found.");
    }

    [ClientRpc]
    private void UpdateLobbyCountdownClientRpc(int secondsRemaining)
    {
        if (countdownText != null)
            countdownText.text = $"Returning to Lobby in {secondsRemaining}...";
    }

    private void SetLocalGameplayEnabled(bool enabled)
    {
        PlayerInputController[] inputControllers =
            FindObjectsByType<PlayerInputController>(FindObjectsSortMode.None);

        foreach (PlayerInputController inputController in inputControllers)
        {
            if (!inputController.IsOwner)
                continue;

            if (!enabled)
                inputController.GetComponent<PlayerChat>()?.CloseForRespawn();

            inputController.SetGameplayInputEnabled(enabled);
        }
    }

    private void HideEndGameUI()
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        if (winnerText != null)
            winnerText.text = "";

        if (countdownText != null)
            countdownText.text = "";
    }
}