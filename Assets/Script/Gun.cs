using UnityEngine;
using Unity.Netcode;

public class Gun : NetworkBehaviour
{
    [Header("Damage")]
    [SerializeField]private float damage = 25f;
    [SerializeField]private float range = 100f;
    [SerializeField]private float fireRate = 0.15f;

    [Header("Spread")]
    [SerializeField]private float hipFireSpread = 0.03f;

    [Header("Bullet Visual")]
    [SerializeField]private GameObject bulletPrefab;
    [SerializeField]private Transform muzzlePoint;

    [Header("Floating Damage")]
    [SerializeField]private FloatingDamage floatingDamagePrefab;

    private Camera cam;
    private PlayerInputController _inputManager;
    private PlayerLook _playerLook;
    private float nextFireTime;

    private void Start()
    {
        cam = GetComponentInParent<PlayerLook>()._cam;
        _inputManager = GetComponentInParent<PlayerInputController>();
        _playerLook = GetComponentInParent<PlayerLook>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (_inputManager.OnFoot.Shoot.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Vector3 shootDirection = GetSpreadDirection();
            ShootServerRpc(cam.transform.position, shootDirection, muzzlePoint.position);
            _playerLook.Recoil();
        }
    }

    private Vector3 GetSpreadDirection()
    {
        Vector3 direction = cam.transform.forward;

        direction += cam.transform.right * Random.Range(-hipFireSpread, hipFireSpread);
        direction += cam.transform.up *Random.Range(-hipFireSpread, hipFireSpread);

        return direction.normalized;
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 origin, Vector3 direction, Vector3 muzzlePos)
    {
        Vector3 hitPoint = origin + direction * range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
        {
            hitPoint = hit.point;
            PlayerHealth health = hit.collider.GetComponent<PlayerHealth>();
            if (health != null)
            {
                bool killed = health.TakeDamage((int)damage);
                SpawnFloatingDamageClientRpc(hit.point + Vector3.up,(int)damage);

                if (killed)
                {
                    GameManager.main.AddPoint(OwnerClientId);
                }
            }
        }

        SpawnBulletClientRpc(muzzlePos, hitPoint);
    }

    [ClientRpc]
    private void SpawnBulletClientRpc(Vector3 startPos, Vector3 hitPoint)
    {
        if (bulletPrefab == null)
            return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            startPos,
            Quaternion.LookRotation(hitPoint - startPos)
        );

        BulletVisuals bulletScript = bullet.GetComponent<BulletVisuals>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(hitPoint);
        }
    }

    [ClientRpc]
    private void SpawnFloatingDamageClientRpc(Vector3 position, int damageAmount)
    {
        if (floatingDamagePrefab == null)
            return;

        FloatingDamage floatingDamage = Instantiate(
            floatingDamagePrefab,
            position,
            Quaternion.identity
        );

        floatingDamage.Setup(damageAmount);
    }
}