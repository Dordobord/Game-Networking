using System.Collections;
using UnityEngine;

public class TargetDummy : MonoBehaviour, IDamageable
{
    [SerializeField]private float maxHealth = 100f;
    [SerializeField]private float flashTime = 0.15f;

    private float currentHealth;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
            return; 
        }

        StopAllCoroutines();
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        rend.material.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        rend.material.color = originalColor;
    }
}