using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerAttack : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPoint;

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            ShootServerRpc();
        }
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        GameObject bullet = Instantiate(bulletPrefab,shootPoint.position, shootPoint.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetOwner(OwnerClientId);
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}