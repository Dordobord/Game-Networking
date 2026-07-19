using TMPro;
using UnityEngine;

public class PlayerCountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerCountText;

    private void Start()
    {
        if (PlayerRegistry.main == null)
        {
            Debug.LogError("PlayerCountUI: PlayerRegistry was not found.");
            enabled = false;
            return;
        }

        PlayerRegistry.main.playerCount.OnValueChanged += UpdateUI;
        UpdateUI(0, PlayerRegistry.main.playerCount.Value);
    }

    private void OnDestroy()
    {
        if (PlayerRegistry.main != null)
        {
            PlayerRegistry.main.playerCount.OnValueChanged -= UpdateUI;
        } 
            
    }

    private void UpdateUI(int previousValue, int newValue)
    {
        if (playerCountText != null)
        {
            playerCountText.text = "Players: " + newValue;
        }
    }
}