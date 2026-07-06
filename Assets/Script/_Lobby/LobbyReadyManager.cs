using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyReadyManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerListTxt;
    [SerializeField] private TMP_Text readyStatusTxt;
    [SerializeField] private GameObject startGameBtn;
    
    private Dictionary<ulong, bool> playerReadyStates = new Dictionary<ulong, bool>();
    //uassign long integer, bool for ready or not

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                playerReadyStates[clientId] = false; // Initialize all players as not ready
            }
            startGameBtn.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        }
    }

    private void Singleton_OnClientDisconnectCallback(ulong obj)
    {
        throw new NotImplementedException();
    }

    private void Singleton_OnClientConnectedCallback(ulong obj)
    {
        throw new NotImplementedException();
    }
}
