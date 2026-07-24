using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerHealth : NetworkBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image frontHealthBar;
    [SerializeField] private Image backHealthBar;
    [SerializeField] private float chipSpeed = 2f;

    [Header("Damage Overlay")]
    [SerializeField] private Image overlay;
    [SerializeField] private float overlayDuration = 1f;
    [SerializeField] private float overlayFadeSpeed = 2f;
    [SerializeField] private int persistentOverlayHealth = 30;

    private PlayerHealth playerHealth;

    private float maxHealth;
    private float chipTimer;
    private float overlayTimer;

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
            Debug.LogError("UIPlayerHealth: PlayerHealth was not found.");
            enabled = false;
            return;
        }

        maxHealth = playerHealth.GetMaxHealth();

        playerHealth.currentHealth.OnValueChanged += OnHealthChanged;

        SetHealthBars(playerHealth.currentHealth.Value);

        if (overlay != null)
            SetOverlayAlpha(0f);
    }

    public override void OnNetworkDespawn()
    {
        if (playerHealth != null)
            playerHealth.currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (playerHealth == null)
            return;

        UpdateHealthBars();
        UpdateDamageOverlay();
    }

    private void UpdateHealthBars()
    {
        float healthFraction =
            Mathf.Clamp01(playerHealth.currentHealth.Value / maxHealth);

        float frontFill = frontHealthBar.fillAmount;
        float backFill = backHealthBar.fillAmount;

        chipTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(chipTimer / chipSpeed);
        float smoothProgress = progress * progress;

        if (backFill > healthFraction)
        {
            frontHealthBar.fillAmount = healthFraction;
            backHealthBar.color = Color.red;
            backHealthBar.fillAmount = Mathf.Lerp(
                backFill,
                healthFraction,
                smoothProgress
            );
        }
        else if (frontFill < healthFraction)
        {
            backHealthBar.color = Color.blue;
            backHealthBar.fillAmount = healthFraction;
            frontHealthBar.fillAmount = Mathf.Lerp(frontFill, healthFraction, smoothProgress);
        }
    }

    private void UpdateDamageOverlay()
    {
        if (overlay == null || overlay.color.a <= 0f)
            return;

        if (playerHealth.currentHealth.Value < persistentOverlayHealth)
            return;

        overlayTimer += Time.deltaTime;

        if (overlayTimer <= overlayDuration)
            return;

        float alpha = Mathf.MoveTowards(
            overlay.color.a,
            0f,
            overlayFadeSpeed * Time.deltaTime
        );

        SetOverlayAlpha(alpha);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        chipTimer = 0f;

        if (newValue >= maxHealth)
        {
            SetHealthBars(newValue);

            if (overlay != null)
                SetOverlayAlpha(0f);

            return;
        }

        if (newValue < previousValue && overlay != null)
        {
            overlayTimer = 0f;
            SetOverlayAlpha(1f);
        }
    }

    private void SetHealthBars(int health)
    {
        float healthFraction = Mathf.Clamp01(health / maxHealth);

        frontHealthBar.fillAmount = healthFraction;
        backHealthBar.fillAmount = healthFraction;
        backHealthBar.color = Color.blue;
    }

    private void SetOverlayAlpha(float alpha)
    {
        Color color = overlay.color;
        color.a = alpha;
        overlay.color = color;
    }
}