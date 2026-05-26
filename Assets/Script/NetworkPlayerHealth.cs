using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerHealth : NetworkBehaviour
{
    [SerializeField]private int maxHealth = 100;
    [SerializeField] private FloatingDamageText floatingDamagePrefab;
    [SerializeField] private Transform damageTextSpawnPoint;
    //Network Sync Health variable
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone, //the host client and server can read this variable.
        NetworkVariableWritePermission.Server //the server can only change this value.
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    public void OnHealthChanged(int previousValue, int newValue)
    {
        Debug.Log($"{gameObject.name} Health Change: {previousValue} -> {newValue}");
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsServer) return;
        currentHealth.Value -= damageAmount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        ShowDamageClientRpc(damageAmount);

        if (currentHealth.Value <= 0)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        currentHealth.Value = maxHealth;
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("Spawnpoint");
        int randomIndex = Random.Range(0, spawnPointObjects.Length);
        Transform selectedSpawn = spawnPointObjects[randomIndex].transform;

        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        transform.position = selectedSpawn.position;
        transform.rotation = selectedSpawn.rotation;    

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    [ClientRpc]
    private void ShowDamageClientRpc(int damageAmount)
    {
        FloatingDamageText damageText = Instantiate(floatingDamagePrefab, damageTextSpawnPoint.position, Quaternion.identity);

        damageText.Setup(damageAmount);
    }

}
