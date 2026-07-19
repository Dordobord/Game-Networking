using Unity.Netcode;
using UnityEngine;

public class OldBullet : NetworkBehaviour
{
    private ulong playerId;

    [SerializeField] private float speed = 20f;
    [SerializeField] private int damage = 25;
    [SerializeField] private LayerMask playerMask;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetOwner(ulong id)
    {
        playerId = id;
    }

    public override void OnNetworkSpawn()
    {
        rb.linearVelocity = transform.forward * speed;

        Invoke(nameof(DespawnBullet), 5f);
    }

    private void DespawnBullet()
    {

        if (!IsServer)
            return;

        if (NetworkObject != null &&
            NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health =
            other.GetComponent<PlayerHealth>();

        if (health != null)
        {
            bool killed =
                health.TakeDamage(damage);

            if (killed)
            {
                GameManager.main.AddPoint(playerId);
            }
        }

        if (IsServer &&
            NetworkObject != null &&
            NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}