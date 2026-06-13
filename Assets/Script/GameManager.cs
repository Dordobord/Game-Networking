using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager main;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text winnerText;

    private NetworkVariable<int> player1Score = new();
    private NetworkVariable<int> player2Score = new();

    private bool gameEnded;

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        winnerText.text = "";
    }

    public override void OnNetworkSpawn()
    {
        player1Score.OnValueChanged += UpdateScoreUI;
        player2Score.OnValueChanged += UpdateScoreUI;

        UpdateScoreUI(0, 0);
    }

    private void UpdateScoreUI(int oldValue, int newValue)
    {
        scoreText.text = "Player 1: " + player1Score.Value + "\nPlayer 2: " + player2Score.Value;
    }

    public void AddPoint(ulong attackerId)
    {
        if (!IsServer || gameEnded)
            return;

        if (attackerId == 0)
        {
            player1Score.Value++;
        }
        else
        {
            player2Score.Value++;
        }

        CheckWinner();
    }

    private void CheckWinner()
    {
        if (player1Score.Value >= 5)
        {
            EndGameClientRpc("Player 1 Wins!");
        }
        else if (player2Score.Value >= 5)
        {
            EndGameClientRpc("Player 2 Wins!");
        }
    }

    private void EndGame(string message)
    {
        gameEnded = true;
        EndGameClientRpc(message);
    }

    [ClientRpc]
    private void EndGameClientRpc(string message)
    {
        Debug.Log(message);
        winnerText.text = message;
    }
}