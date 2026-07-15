using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class WeaponPickup : Interactable
{
    [SerializeField] private int weaponId;

    private NetworkObject networkObject;
    private bool isPickedUp;

    public int WeaponId => weaponId;
    public bool IsPickedUp => isPickedUp;

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    protected override void Interact()
    {
        if (isPickedUp || CurrentInteractor == null)
            return;

        PlayerWeaponManager weaponManager =
            CurrentInteractor.GetComponent<PlayerWeaponManager>();

        if (weaponManager == null)
            return;

        weaponManager.RequestPickup(networkObject);
    }

    public bool TryMarkPickedUp()
    {
        if (isPickedUp)
            return false;

        isPickedUp = true;
        return true;
    }
}