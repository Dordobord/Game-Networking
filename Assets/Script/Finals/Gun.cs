using UnityEngine;

public class Gun : MonoBehaviour
{
    private Camera cam;
    private InputManager inputManager;

    [SerializeField]private float damage = 20f;
    [SerializeField]private float range = 100f;
    [SerializeField]private float fireRate = 0.15f;

    [Header("Bullet")]
    [SerializeField]private GameObject bulletPrefab;
    [SerializeField]private Transform muzzlePoint;

    private float nextFireTime;

    void Start()
    {
        cam = GetComponentInParent<PlayerLook>().cam;
        inputManager = GetComponentInParent<InputManager>();
    }

    void Update()
    {
        if (inputManager.OnFoot.Shoot.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Vector3 hitPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            hitPoint = hit.point;

            if (hit.collider.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(damage);
            }
        }
        else
        {
            hitPoint = ray.origin + ray.direction * range;
        }

        SpawnBullet(hitPoint);
    }

    void SpawnBullet(Vector3 hitPoint)
    {
        if (bulletPrefab == null || muzzlePoint == null) return;

        Vector3 direction = (hitPoint - muzzlePoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject bulletObj = Instantiate(bulletPrefab, muzzlePoint.position, rotation);
        bulletObj.GetComponent<NetworkBullet>().SetTarget(hitPoint);
    }
}