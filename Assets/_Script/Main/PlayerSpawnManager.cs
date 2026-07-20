using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    private static int nextSpawnIndex;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        SpawnAtNextPoint(transform);
    }

    public static void RespawnPlayer(Transform player)
    {
        if (player == null)
            return;

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("PlayerSpawnManager: No tagged SpawnPoints were found.");
            return;
        }

        Transform safestSpawn = FindSafestSpawnPoint(spawnPoints, player);
        TeleportPlayer(player, safestSpawn);
    }

    private static void SpawnAtNextPoint(Transform player)
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("PlayerSpawnManager: No tagged SpawnPoints were found.");
            return;
        }

        nextSpawnIndex %= spawnPoints.Length;
        Transform spawnPoint = spawnPoints[nextSpawnIndex].transform;
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;

        TeleportPlayer(player, spawnPoint);
    }

    private static Transform FindSafestSpawnPoint(
        GameObject[] spawnPoints,
        Transform respawningPlayer)
    {
        PlayerHealth[] players = Object.FindObjectsByType<PlayerHealth>(
            FindObjectsSortMode.None
        );

        Transform safestSpawn = spawnPoints[0].transform;
        float bestScore = float.NegativeInfinity;
        bool foundLivingOpponent = false;

        foreach (GameObject spawnPointObject in spawnPoints)
        {
            Transform spawnPoint = spawnPointObject.transform;
            float nearestOpponentDistanceSqr = float.PositiveInfinity;
            bool hasLivingOpponent = false;

            foreach (PlayerHealth playerHealth in players)
            {
                if (playerHealth == null ||
                    playerHealth.transform == respawningPlayer ||
                    playerHealth.IsDead)
                {
                    continue;
                }

                hasLivingOpponent = true;
                float distanceSqr = (spawnPoint.position -
                                     playerHealth.transform.position).sqrMagnitude;

                if (distanceSqr < nearestOpponentDistanceSqr)
                    nearestOpponentDistanceSqr = distanceSqr;
            }

            if (!hasLivingOpponent)
                continue;

            foundLivingOpponent = true;

            if (nearestOpponentDistanceSqr > bestScore)
            {
                bestScore = nearestOpponentDistanceSqr;
                safestSpawn = spawnPoint;
            }
        }

        if (foundLivingOpponent)
            return safestSpawn;

        nextSpawnIndex %= spawnPoints.Length;
        Transform fallbackSpawn = spawnPoints[nextSpawnIndex].transform;
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
        return fallbackSpawn;
    }

    private static void TeleportPlayer(Transform player, Transform spawnPoint)
    {
        PlayerSpawnManager spawnManager =
            player.GetComponent<PlayerSpawnManager>();

        if (spawnManager == null)
        {
            Debug.LogError(
                "PlayerSpawnManager: The player is missing PlayerSpawnManager."
            );

            return;
        }

        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        // Update the server's copy immediately.
        ApplyTeleport(player, spawnPosition, spawnRotation);

        // Remote players own their NetworkTransform, so their owning client
        // must apply the same teleport to prevent position (0, 0, 0) from
        // overwriting the server-selected SpawnPoint.
        if (!spawnManager.IsOwner)
        {
            spawnManager.TeleportOwnerRpc(spawnPosition, spawnRotation);
        }
    }

    [Rpc(SendTo.Owner)]
    private void TeleportOwnerRpc(
        Vector3 spawnPosition,
        Quaternion spawnRotation)
    {
        ApplyTeleport(transform, spawnPosition, spawnRotation);
    }

    private static void ApplyTeleport(
        Transform player,
        Vector3 spawnPosition,
        Quaternion spawnRotation)
    {
        CharacterController controller =
            player.GetComponent<CharacterController>();

        PlayerMovement movement =
            player.GetComponent<PlayerMovement>();

        if (controller != null)
            controller.enabled = false;

        player.SetPositionAndRotation(spawnPosition, spawnRotation);
        movement?.ResetMovementState();

        if (controller != null)
            controller.enabled = true;
    }
}