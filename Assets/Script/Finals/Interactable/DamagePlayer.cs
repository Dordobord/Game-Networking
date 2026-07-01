using UnityEngine;

public class DamagePlayer : Interactable
{
    public PlayerHealth playerHealth;
    public int damageAmount = 20;

    protected override void Interact()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log("Player took " + damageAmount + " damage.");
        }
    }
}