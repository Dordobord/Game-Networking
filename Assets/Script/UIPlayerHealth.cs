using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIPlayerHealth : NetworkBehaviour
{
    [SerializeField] private NetworkPlayerHealth playerHealth;

    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateHealth(playerHealth.currentHealth.Value,playerHealth.currentHealth.Value);

        playerHealth.currentHealth.OnValueChanged += UpdateHealth;
    }

    private void OnDestroy()
    {
        playerHealth.currentHealth.OnValueChanged -= UpdateHealth;
    }

    private void UpdateHealth(int previous, int current)
    {
        healthText.text = "Health: " + current;
    }
}