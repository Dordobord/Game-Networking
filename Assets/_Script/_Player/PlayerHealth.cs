using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("Respawn Settings")]
    [SerializeField, Min(1)] private int respawnDelay = 3;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> isDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private PlayerInputController inputController;
    private UIPlayerRespawn respawnUI;
    private Coroutine respawnCoroutine;

    public bool IsDead => isDead.Value;

    private void Awake()
    {
        inputController = GetComponent<PlayerInputController>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            isDead.Value = false;
        }

        if (IsOwner)
        {
            respawnUI = FindFirstObjectByType<UIPlayerRespawn>();
            respawnUI?.Hide();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        if (IsOwner)
            respawnUI?.Hide();
    }

    public bool TakeDamage(int damageAmount)
    {
        if (!IsServer || isDead.Value || damageAmount <= 0)
            return false;

        currentHealth.Value = Mathf.Clamp(
            currentHealth.Value - damageAmount,
            0,
            maxHealth
        );

        if (currentHealth.Value > 0)
            return false;

        Die();
        return true;
    }

    public void Heal(int healAmount)
    {
        if (!IsServer || isDead.Value || healAmount <= 0)
            return;

        currentHealth.Value = Mathf.Clamp(
            currentHealth.Value + healAmount,
            0,
            maxHealth
        );
    }

    private void Die()
    {
        if (!IsServer || isDead.Value)
            return;

        isDead.Value = true;
        SetRespawningRpc(true);

        respawnCoroutine = StartCoroutine(RespawnCountdown());
    }

    private IEnumerator RespawnCountdown()
    {
        for (int seconds = respawnDelay; seconds > 0; seconds--)
        {
            ShowRespawnCountdownRpc(seconds);
            yield return new WaitForSeconds(1f);
        }

        PlayerSpawnManager.RespawnPlayer(transform);
        currentHealth.Value = maxHealth;
        isDead.Value = false;
        respawnCoroutine = null;

        SetRespawningRpc(false);
    }

    [Rpc(SendTo.Owner)]
    private void ShowRespawnCountdownRpc(int secondsRemaining)
    {
        if (respawnUI == null)
            respawnUI = FindFirstObjectByType<UIPlayerRespawn>();

        respawnUI?.ShowCountdown(secondsRemaining);
    }

    [Rpc(SendTo.Owner)]
    private void SetRespawningRpc(bool respawning)
    {
        if (inputController == null)
            inputController = GetComponent<PlayerInputController>();

        if (respawning)
            GetComponent<PlayerChat>()?.CloseForRespawn();

        inputController?.SetGameplayInputEnabled(!respawning);

        if (respawning)
            return;

        respawnUI?.Hide();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}