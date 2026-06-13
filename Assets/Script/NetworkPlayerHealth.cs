using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private FloatingDamageText floatingDamagePrefab;
    [SerializeField] private Transform damageTextSpawnPoint;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>( 100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    public void OnHealthChanged(
        int previousValue,
        int newValue)
    {
        Debug.Log($"{gameObject.name} Health Change: " + $"{previousValue} -> {newValue}");
    }

    public bool TakeDamage(int damageAmount)
    {
        if (!IsServer)
            return false;

        currentHealth.Value -= damageAmount;
        currentHealth.Value =
            Mathf.Clamp( currentHealth.Value, 0, maxHealth);

        ShowDamageClientRpc(damageAmount);

        if (currentHealth.Value <= 0)
        {
            Respawn();
            return true;
        }
        return false;
    }

    public void Respawn()
    {
        currentHealth.Value = maxHealth;

        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");

        int randomIndex = Random.Range(0, spawnPointObjects.Length);

        Transform selectedSpawn = spawnPointObjects[randomIndex].transform;

        CharacterController cc = GetComponent<CharacterController>();

        if (cc != null)
        {
            cc.enabled = false;
        }

        transform.position = selectedSpawn.position;

        transform.rotation = selectedSpawn.rotation;

        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    [ClientRpc]
    private void ShowDamageClientRpc(
        int damageAmount)
    {
        FloatingDamageText damageText = Instantiate(floatingDamagePrefab, damageTextSpawnPoint.position, Quaternion.identity);
        damageText.Setup(damageAmount);
    }
}