using UnityEngine;
using Unity.Netcode;
public class NetworkPlayerAttack : NetworkBehaviour
{
    [SerializeField]private float attackRange = 3f;
    [SerializeField]private int damageAmount = 25;
    [SerializeField]private LayerMask playerMask;


    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RequestAttackServerRpc();
        }
    }

    [ServerRpc]
    private void RequestAttackServerRpc()
    {
        Vector3 attackCenter = transform.position + transform.forward;
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRange, playerMask);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            NetworkPlayerHealth targetHealth = hit.GetComponent<NetworkPlayerHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damageAmount);
                break;
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);
    }
}
