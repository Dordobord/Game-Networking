using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIScoreboard : MonoBehaviour
{
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text scoreListText;

    private readonly List<PlayerScoreData> sortedScores = new();

    private GameManager subscribedGameManager;
    private bool isVisible;

    private void Start()
    {
        SetScoreboardVisible(false);
        TrySubscribeToScores();
    }

    private void Update()
    {
        TrySubscribeToScores();

        if (Keyboard.current == null)
            return;

        bool shouldShow = Keyboard.current.tabKey.isPressed;

        if (shouldShow == isVisible)
            return;

        SetScoreboardVisible(shouldShow);

        if (shouldShow)
            Refresh();
    }

    private void OnDestroy()
    {
        UnsubscribeFromScores();
    }

    private void TrySubscribeToScores()
    {
        if (GameManager.main == null ||
            subscribedGameManager == GameManager.main)
        {
            return;
        }

        UnsubscribeFromScores();

        subscribedGameManager = GameManager.main;
        subscribedGameManager.PlayerScores.OnListChanged += HandleScoresChanged;

        Refresh();
    }

    private void UnsubscribeFromScores()
    {
        if (subscribedGameManager == null)
            return;

        subscribedGameManager.PlayerScores.OnListChanged -= HandleScoresChanged;
        subscribedGameManager = null;
    }

    private void HandleScoresChanged(NetworkListEvent<PlayerScoreData> changeEvent)
    {
        Refresh();
    }

    private void SetScoreboardVisible(bool visible)
    {
        isVisible = visible;

        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(visible);
    }

    private void Refresh()
    {
        if (subscribedGameManager == null)
            return;

        sortedScores.Clear();

        foreach (PlayerScoreData entry in subscribedGameManager.PlayerScores)
            sortedScores.Add(entry);

        sortedScores.Sort((a, b) => b.Kills.CompareTo(a.Kills));

        if (playerCountText != null)
            playerCountText.text = $"Players: {sortedScores.Count}";

        if (scoreListText == null)
            return;

        scoreListText.text = BuildScoreList();
    }

    private string BuildScoreList()
    {
        System.Text.StringBuilder builder = new();

        foreach (PlayerScoreData entry in sortedScores)
        {
            builder.Append(entry.PlayerName).Append(" - ").Append(entry.Kills).AppendLine(" kills");
        }
        return builder.ToString();
    }
}