using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkManager))]
public class MultiplayerGameFlow : MonoBehaviour
{
    [Header("Game Flow")]
    [SerializeField] private string gameplaySceneName = "MapFPS";
    [SerializeField] private string lobbySceneName = "MultiplayerLobby";

    private NetworkManager networkManager;
    private bool matchStarted;
    private bool waitingForSceneLoad;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();

        networkManager.NetworkConfig.ConnectionApproval = true;
        networkManager.ConnectionApprovalCallback += ApproveConnection;
    }

    private void OnDestroy()
    {
        if (networkManager == null)
            return;

        networkManager.ConnectionApprovalCallback -= ApproveConnection;

        if (networkManager.SceneManager != null)
        {
            networkManager.SceneManager.OnLoadEventCompleted -=
                HandleLoadEventCompleted;

            networkManager.SceneManager.OnLoadEventCompleted -=
                HandleLobbyLoadCompleted;
        }
    }

    private void ApproveConnection(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        if (matchStarted)
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Pending = false;
            response.Reason = "The match has already started.";
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Position = null;
        response.Rotation = null;
        response.Pending = false;
    }

    public void StartGame()
    {
        if (networkManager == null ||
            !networkManager.IsListening ||
            !networkManager.IsServer)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        if (matchStarted || waitingForSceneLoad)
            return;

        if (networkManager.SceneManager == null)
        {
            Debug.LogError(
                "Netcode scene management is unavailable."
            );
            return;
        }

        matchStarted = true;
        waitingForSceneLoad = true;

        networkManager.SceneManager.OnLoadEventCompleted +=
            HandleLoadEventCompleted;

        SceneEventProgressStatus loadStatus =
            networkManager.SceneManager.LoadScene(
                gameplaySceneName,
                LoadSceneMode.Single
            );

        if (loadStatus == SceneEventProgressStatus.Started)
            return;

        Debug.LogError(
            $"Could not load scene '{gameplaySceneName}'. " +
            $"Status: {loadStatus}"
        );

        networkManager.SceneManager.OnLoadEventCompleted -=
            HandleLoadEventCompleted;

        matchStarted = false;
        waitingForSceneLoad = false;

    }

    public void ReturnToLobby()
    {
        if (networkManager == null ||
            !networkManager.IsListening ||
            !networkManager.IsServer ||
            waitingForSceneLoad)
        {
            return;
        }

        if (networkManager.SceneManager == null)
        {
            Debug.LogError("Netcode scene management is unavailable.");
            return;
        }

        waitingForSceneLoad = true;

        networkManager.SceneManager.OnLoadEventCompleted +=
            HandleLobbyLoadCompleted;

        SceneEventProgressStatus loadStatus =
            networkManager.SceneManager.LoadScene(
                lobbySceneName,
                LoadSceneMode.Single
            );

        if (loadStatus == SceneEventProgressStatus.Started)
            return;

        networkManager.SceneManager.OnLoadEventCompleted -=
            HandleLobbyLoadCompleted;

        waitingForSceneLoad = false;

        Debug.LogError(
            $"Could not load scene '{lobbySceneName}'. " +
            $"Status: {loadStatus}"
        );
    }

    private void HandleLoadEventCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (!networkManager.IsServer ||
            sceneName != gameplaySceneName)
        {
            return;
        }

        networkManager.SceneManager.OnLoadEventCompleted -=
            HandleLoadEventCompleted;

        waitingForSceneLoad = false;

        foreach (ulong clientId in clientsCompleted)
        {
            SpawnPlayer(clientId);
        }

        foreach (ulong clientId in clientsTimedOut)
        {
            Debug.LogWarning(
                $"Client {clientId} timed out while loading " +
                $"{gameplaySceneName}."
            );
        }
    }

    private void HandleLobbyLoadCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (!networkManager.IsServer || sceneName != lobbySceneName)
            return;

        networkManager.SceneManager.OnLoadEventCompleted -=
            HandleLobbyLoadCompleted;

        waitingForSceneLoad = false;
        matchStarted = false;

        foreach (ulong clientId in clientsTimedOut)
        {
            Debug.LogWarning(
                $"Client {clientId} timed out while returning to " +
                $"'{lobbySceneName}'."
            );
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!networkManager.ConnectedClients.TryGetValue(
                clientId,
                out NetworkClient client))
        {
            return;
        }

        if (client.PlayerObject != null)
            return;

        GameObject playerPrefab =
            networkManager.NetworkConfig.PlayerPrefab;

        if (playerPrefab == null)
        {
            Debug.LogError(
                "The NetworkManager Default Player Prefab is missing."
            );
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab);

        NetworkObject playerNetworkObject =
            playerInstance.GetComponent<NetworkObject>();

        if (playerNetworkObject == null)
        {
            Debug.LogError(
                "The player prefab requires a NetworkObject."
            );

            Destroy(playerInstance);
            return;
        }

        playerNetworkObject.SpawnAsPlayerObject(
            clientId,
            destroyWithScene: true
        );
    }

}