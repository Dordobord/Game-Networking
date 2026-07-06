using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    }

    public bool TakeDamage(int damageAmount)
    {
        if (!IsServer)
            return false;

        currentHealth.Value -= damageAmount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    public void Heal(int healAmount)
    {
        if (!IsServer)
            return;

        currentHealth.Value += healAmount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");

        currentHealth.Value = maxHealth;
        PlayerSpawnManager.RespawnPlayer(transform);
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}