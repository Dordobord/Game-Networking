using Unity.Netcode;

public class PlayerRegistry : NetworkBehaviour
{
    public static PlayerRegistry main;

    public NetworkVariable<int> playerCount = new();

    private void Awake()
    {
        main = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        UpdatePlayerCount();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientChanged;
        }
    }

    private void OnClientChanged(ulong clientId)
    {
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        playerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

    public override void OnDestroy()
    {
        if (main == this)
            main = null;

        base.OnDestroy();
    }
}