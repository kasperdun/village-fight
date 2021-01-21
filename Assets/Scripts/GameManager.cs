using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private static int _enemyScore = 0;
    private static int _myScore = 0;
    public static string nickName;
    public static int myTeamLayer;
    public static int enemyTeamLayer;

    public delegate void ScoreUpdatedAction(bool isMineDead);
    public static event ScoreUpdatedAction OnScoreUpdated;

    public delegate void GameOverAction();
    public static event GameOverAction OnGameOver;

    public static bool gameOver = false;

    protected GameManager() { }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        UpdateNickName();
        myTeamLayer = LayerMask.NameToLayer("Team1");
        enemyTeamLayer = LayerMask.NameToLayer("Team2");
    }
    public static void GameOver()
    {
        gameOver = true;
        Time.timeScale = 0;
        VictoryDialog victoryDialog = Resources.Load<VictoryDialog>("Prefabs/UI/VictoryDialog");
        Instantiate(victoryDialog);

        if (OnGameOver != null)
            OnGameOver();
    }

    public static void UpdateNickName(string newNick = null)
    {
        if (!string.IsNullOrEmpty(newNick)) {
            nickName = newNick;
            PlayerPrefs.SetString("NickName", nickName);
        }
        else
        {
            if (PlayerPrefs.HasKey("NickName"))
            {
                nickName = PlayerPrefs.GetString("NickName");
            }
            else
            {
                nickName = $"Player_{Random.Range(1000, 9999)}";
                PlayerPrefs.SetString("NickName", nickName);
            }
        }

        PhotonNetwork.NickName = nickName;
    }
    public static int GetEnemyScore() => _enemyScore;
    public static int GetMyScore() => _myScore;
    public static void AddScore(bool isMineDead)
    {
        if(isMineDead)
        {
            _enemyScore++;
        }
        else
        {
            _myScore++;
        }

        if (OnScoreUpdated != null)
            OnScoreUpdated(isMineDead);
    }
}
