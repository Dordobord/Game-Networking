using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkCrate : NetworkBehaviour
{
    [Header("Crate")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform weaponSpawnPoint;

    [Header("Random Weapon Pickups")]
    [SerializeField] private NetworkObject[] weaponPickupPrefabs;

    [Header("Cooldown")]
    [SerializeField] private float reopenCooldown = 5f;

    private readonly NetworkVariable<bool> isOpen = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool weaponSpawned;
    private bool canOpen = true;
    private NetworkObject spawnedWeaponPickup;

    public bool IsOpen => isOpen.Value;

    public override void OnNetworkSpawn()
    {
        isOpen.OnValueChanged += OnOpenChanged;
        SetOpenAnimation(isOpen.Value);
    }

    public override void OnNetworkDespawn()
    {
        isOpen.OnValueChanged -= OnOpenChanged;
    }

    public void RequestOpen()
    {
        if (!IsSpawned || isOpen.Value)
            return;

        RequestOpenRpc();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestOpenRpc()
    {
        if (isOpen.Value || !canOpen)
            return;

        canOpen = false;
        isOpen.Value = true;
    }

    public void FinishOpenAnimation()
    {
        if (!IsServer || !isOpen.Value || weaponSpawned)
            return;

        SpawnRandomWeapon();
    }

    private void OnOpenChanged(bool previousValue, bool newValue)
    {
        SetOpenAnimation(newValue);
    }

    private void SetOpenAnimation(bool open)
    {
        if (animator != null)
            animator.SetBool("IsOpen", open);
    }

    private void SpawnRandomWeapon()
    {
        if (weaponSpawnPoint == null ||
            weaponPickupPrefabs == null ||
            weaponPickupPrefabs.Length == 0)
        {
            Debug.LogWarning($"{name}: Crate weapon spawn setup is incomplete.");
            return;
        }

        NetworkObject selectedPrefab =
            weaponPickupPrefabs[Random.Range(0, weaponPickupPrefabs.Length)];

        if (selectedPrefab == null)
        {
            Debug.LogWarning($"{name}: A weapon pickup prefab is missing.");
            return;
        }

        spawnedWeaponPickup = Instantiate(
            selectedPrefab,
            weaponSpawnPoint.position,
            weaponSpawnPoint.rotation
        );

        spawnedWeaponPickup.Spawn();
        weaponSpawned = true;

        StartCoroutine(WaitForWeaponPickup());
    }

    private IEnumerator WaitForWeaponPickup()
    {
        yield return new WaitUntil(() => spawnedWeaponPickup == null || !spawnedWeaponPickup.IsSpawned);

        spawnedWeaponPickup = null;
        weaponSpawned = false;
        isOpen.Value = false;

        yield return new WaitForSeconds(reopenCooldown);

        canOpen = true;
    }
}