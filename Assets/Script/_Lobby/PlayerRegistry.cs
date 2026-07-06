using Unity.Netcode;
using UnityEngine;

public class PlayerRegistry : NetworkBehaviour
{
    public static PlayerRegistry main;

    public NetworkVariable<int> playerCount = new NetworkVariable<int>();

    void Awake()
    {
        main = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        UpdateCount();

        NetworkManager.Singleton.OnClientConnectedCallback += OnChange;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnChange;
    }

    private void OnChange(ulong id)
    {
        UpdateCount();
    }

    private void UpdateCount()
    {
        playerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }
}
