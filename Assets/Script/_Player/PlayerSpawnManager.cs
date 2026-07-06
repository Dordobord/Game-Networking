using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    private static int nextSpawnIndex;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        RespawnPlayer(transform);
    }

    public static void RespawnPlayer(Transform player)
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No SpawnPoints found!");
            return;
        }

        Transform spawnPoint = spawnPoints[nextSpawnIndex].transform;

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        player.position = spawnPoint.position;
        player.rotation = spawnPoint.rotation;

        if (controller != null)
            controller.enabled = true;

        nextSpawnIndex++;

        if (nextSpawnIndex >= spawnPoints.Length)
            nextSpawnIndex = 0;
    }
}