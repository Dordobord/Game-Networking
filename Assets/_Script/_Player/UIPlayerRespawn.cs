using TMPro;
using UnityEngine;

public class UIPlayerRespawn : MonoBehaviour
{
    [SerializeField] private TMP_Text respawnText;

    private void Awake()
    {
        Hide();
    }

    public void ShowCountdown(int secondsRemaining)
    {
        if (respawnText == null)
            return;

        respawnText.text = $"Respawning in {secondsRemaining}...";
        respawnText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (respawnText == null)
            return;

        respawnText.text = string.Empty;
        respawnText.gameObject.SetActive(false);
    }
}