using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// One entry per connected player. Synced to every client so the
// scoreboard (Tab) and kill feed can read it directly.
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
        return ClientId == other.ClientId;
    }
}

public class GameManager : NetworkBehaviour
{
    public static GameManager main;

    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private int killsToWin = 5;

    public NetworkList<PlayerScoreData> PlayerScores = new NetworkList<PlayerScoreData>();

    private bool gameEnded;

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        if (winnerText != null)
            winnerText.text = "";
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
        if (scorer.Kills >= killsToWin)
        {
            gameEnded = true;
            EndGameClientRpc(scorer.PlayerName.ToString() + " Wins!");
        }
    }

    [ClientRpc]
    private void EndGameClientRpc(string message)
    {
        Debug.Log(message);

        if (winnerText != null)
            winnerText.text = message;
    }
}