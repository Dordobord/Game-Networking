using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIScoreboard : MonoBehaviour
{
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text scoreListText;

    private readonly List<PlayerScoreData> sortedScores = new List<PlayerScoreData>();

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool show = Keyboard.current.tabKey.isPressed;

        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(show);

        if (show)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (GameManager.main == null)
            return;

        sortedScores.Clear();
        foreach (PlayerScoreData entry in GameManager.main.PlayerScores)
        {
            sortedScores.Add(entry);
        }

        sortedScores.Sort((a, b) => b.Kills.CompareTo(a.Kills));

        if (playerCountText != null)
        {
            playerCountText.text = "Players: " + sortedScores.Count;
        }

        if (scoreListText != null)
        {
            string text = "";

            foreach (PlayerScoreData entry in sortedScores)
            {
                text += entry.PlayerName + " - " + entry.Kills + " kills\n";
            }

            scoreListText.text = text;
        }
    }
}