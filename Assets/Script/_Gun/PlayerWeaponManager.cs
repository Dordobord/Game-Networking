using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeaponManager : NetworkBehaviour
{
    [Serializable]
    private class WeaponEntry
    {
        public int weaponId;
        public Gun weaponPrefab;
    }

    [Header("Weapon Setup")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private WeaponEntry[] availableWeapons;

    [Header("UI")]
    [SerializeField] private TMP_Text ammoText;

    private readonly NetworkVariable<int> equippedWeaponId = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Gun currentGun;
    private PlayerHealth shooterHealth;
    private float serverNextFireTime;

    public Gun CurrentGun => currentGun;
    public bool HasWeapon => currentGun != null;

    private void Awake()
    {
        shooterHealth = GetComponent<PlayerHealth>();
    }

    public override void OnNetworkSpawn()
    {
        equippedWeaponId.OnValueChanged += OnWeaponChanged;

        EquipWeapon(equippedWeaponId.Value);

        UpdateAmmoTextVisibility();
    }

    public override void OnNetworkDespawn()
    {
        equippedWeaponId.OnValueChanged -= OnWeaponChanged;
    }

    public void RequestPickup(NetworkObject pickupObject)
    {
        if (!IsOwner || pickupObject == null)
            return;

        RequestPickupRpc(pickupObject.NetworkObjectId);
    }

    [Rpc(SendTo.Server)]
    private void RequestPickupRpc(ulong pickupObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                pickupObjectId,
                out NetworkObject pickupObject))
        {
            return;
        }

        WeaponPickup pickup =
            pickupObject.GetComponent<WeaponPickup>();

        if (pickup == null || pickup.IsPickedUp)
            return;

        if (FindWeaponPrefab(pickup.WeaponId) == null)
            return;

        if (!pickup.TryMarkPickedUp())
            return;

        equippedWeaponId.Value = pickup.WeaponId;
        pickupObject.Despawn(true);
    }

    public void RequestShoot(
        Vector3 origin,
        Vector3 direction,
        Vector3 muzzlePosition)
    {
        if (!IsOwner || currentGun == null)
            return;

        ShootRpc(origin, direction, muzzlePosition);
    }

    [Rpc(SendTo.Server)]
    private void ShootRpc(
        Vector3 origin,
        Vector3 direction,
        Vector3 muzzlePosition)
    {
        if (currentGun == null || currentGun.Data == null)
            return;

        GunData gunData = currentGun.Data;

        if (Time.time < serverNextFireTime)
            return;

        serverNextFireTime = Time.time + gunData.fireRate;

        direction.Normalize();

        Vector3 hitPoint =
            origin + direction * gunData.range;

        if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                gunData.range))
        {
            hitPoint = hit.point;

            PlayerHealth targetHealth =
                hit.collider.GetComponentInParent<PlayerHealth>();

            if (targetHealth != null &&
                targetHealth != shooterHealth)
            {
                int damageAmount =
                    Mathf.RoundToInt(gunData.damage);

                bool killed =
                    targetHealth.TakeDamage(damageAmount);

                SpawnFloatingDamageRpc(
                    hit.point + Vector3.up,
                    damageAmount
                );

                if (killed && GameManager.main != null)
                    GameManager.main.AddPoint(OwnerClientId);
            }
        }

        SpawnBulletRpc(muzzlePosition, hitPoint);
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnBulletRpc(
        Vector3 startPosition,
        Vector3 hitPoint)
    {
        if (currentGun == null ||
            currentGun.BulletPrefab == null)
        {
            return;
        }

        Vector3 direction = hitPoint - startPosition;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        GameObject bullet = Instantiate(
            currentGun.BulletPrefab,
            startPosition,
            Quaternion.LookRotation(direction)
        );

        BulletVisuals bulletVisuals =
            bullet.GetComponent<BulletVisuals>();

        if (bulletVisuals != null)
            bulletVisuals.SetTarget(hitPoint);
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnFloatingDamageRpc(
        Vector3 position,
        int damageAmount)
    {
        if (currentGun == null ||
            currentGun.FloatingDamagePrefab == null)
        {
            return;
        }

        FloatingDamage floatingDamage = Instantiate(
            currentGun.FloatingDamagePrefab,
            position,
            Quaternion.identity
        );

        floatingDamage.Setup(damageAmount);
    }

    private void OnWeaponChanged(
        int previousWeaponId,
        int newWeaponId)
    {
        EquipWeapon(newWeaponId);
    }

    private void EquipWeapon(int weaponId)
    {
        if (currentGun != null)
        {
            Destroy(currentGun.gameObject);
            currentGun = null;
        }

        if (weaponId < 0)
        {
            UpdateAmmoTextVisibility();
            return;
        }

        Gun weaponPrefab = FindWeaponPrefab(weaponId);

        if (weaponPrefab == null)
        {
            UpdateAmmoTextVisibility();
            return;
        }

        currentGun = Instantiate(
            weaponPrefab,
            weaponHolder
        );

        currentGun.transform.localPosition = Vector3.zero;
        currentGun.transform.localRotation = Quaternion.identity;
        currentGun.transform.localScale = Vector3.one;

        currentGun.Initialize(this, ammoText);

        UpdateAmmoTextVisibility();
    }
    private Gun FindWeaponPrefab(int weaponId)
    {
        if (availableWeapons == null)
            return null;

        foreach (WeaponEntry entry in availableWeapons)
        {
            if (entry.weaponId == weaponId)
                return entry.weaponPrefab;
        }

        return null;
    }

    private void UpdateAmmoTextVisibility()
    {
        if (ammoText == null)
            return;

        ammoText.gameObject.SetActive(currentGun != null);
    }
}