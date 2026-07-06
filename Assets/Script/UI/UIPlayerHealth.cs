using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
public class UIPlayerHealth : NetworkBehaviour
{
    [Header("Health Bar")]
    [SerializeField]private Image frontHealthBar;
    [SerializeField]private Image backHealthBar;
    [SerializeField]private float chipSpeed = 2f;

    [Header("Damage overlay")]
    [SerializeField]private Image overlay;
    [SerializeField]private float duration = 1f;
    [SerializeField]private float fadeSpeed = 2f;
    
    private PlayerHealth playerHealth;
    private float lerpTimer;
    private float durationTimer;
    private float maxHealth;

    void Update()
    {
        UpdateHealthUI();
        if(overlay != null && overlay.color.a > 0)
        {
            if (playerHealth.currentHealth.Value < 30)
            {
                return;
            }

            durationTimer += Time.deltaTime;

            if (durationTimer > duration)
            {
                float temp = overlay.color.a;
                temp -= Time.deltaTime * fadeSpeed;
                overlay.color = new Color(overlay.color.r, overlay.color.b, overlay.color.b, temp);
            }
        }
    }
    private void UpdateHealthUI()
    {
        if (playerHealth == null) return;

        float fillFront = frontHealthBar.fillAmount;
        float fillback = backHealthBar.fillAmount;
        float healthFraction = (float)playerHealth.currentHealth.Value / maxHealth;

        if (fillback > healthFraction)
        {
            frontHealthBar.fillAmount = healthFraction;
            backHealthBar.color = Color.red;

            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete *= percentComplete;

            backHealthBar.fillAmount = Mathf.Lerp(fillback, healthFraction, percentComplete);
        }

        if (fillback < healthFraction)
        {
            backHealthBar.color = Color.blue;
            backHealthBar.fillAmount = healthFraction;

            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete *= percentComplete;

            frontHealthBar.fillAmount = Mathf.Lerp(fillFront, backHealthBar.fillAmount, percentComplete);
        }
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        lerpTimer = 0f;

        if (newValue < previousValue)
        {
            durationTimer = 0f;
            if (overlay != null)
            {
                overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 1);
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.Log("NetworkPlayerHealth script missing");
            return;
        }

        maxHealth = playerHealth.GetMaxHealth();

        playerHealth.currentHealth.OnValueChanged += OnHealthChanged;
        
        frontHealthBar.fillAmount = 1f;
        backHealthBar.fillAmount = 1f;

        if (overlay != null)
        {
            overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (playerHealth != null)
        {
            playerHealth.currentHealth.OnValueChanged -= OnHealthChanged;
        }
    }
}