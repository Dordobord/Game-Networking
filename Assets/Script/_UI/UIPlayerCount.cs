using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerCountUI : MonoBehaviour
{
    public TMP_Text text;

    private void Start()
    {
        PlayerRegistry.main.playerCount.OnValueChanged += UpdateUI;
        UpdateUI(0, PlayerRegistry.main.playerCount.Value);
    }

    private void UpdateUI(int oldValue, int newValue)
    {
        text.text = "Players: " + newValue;
    }
}