using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkManager))]
public class MultiplayerGameFlow : MonoBehaviour
{
    [Header("Game Flow")]
    [SerializeField] private string gameplaySceneName = "MapFPS";

    [Header("Lobby UI")]
    [SerializeField] private GameObject startGameButton;

    private NetworkManager networkManager;
    private bool matchStarted;
    private bool waitingForSceneLoad;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();

        networkManager.NetworkConfig.ConnectionApproval = true;
        networkManager.ConnectionApprovalCallback += ApproveConnection;
        networkManager.OnServerStarted += HandleServerStarted;

        SetStartButtonVisible(false);
    }

    private void OnDestroy()
    {
        if (networkManager == null)
            return;

        networkManager.ConnectionApprovalCallback -= ApproveConnection;
        networkManager.OnServerStarted -= HandleServerStarted;

        if (networkManager.SceneManager != null)
        {
            networkManager.SceneManager.OnLoadEventCompleted -=
                HandleLoadEventCompleted;
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

    private void HandleServerStarted()
    {
        SetStartButtonVisible(networkManager.IsHost);
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

        SetStartButtonVisible(false);

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

        SetStartButtonVisible(networkManager.IsHost);
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

    private void SetStartButtonVisible(bool visible)
    {
        if (startGameButton != null)
            startGameButton.SetActive(visible);
    }
}