using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text myScoreText;
    public Text enemyScoreText;

    private void OnEnable()
    {
        GameManager.OnScoreUpdated += OnScoreUpdate;
    }

    private void OnScoreUpdate(bool isMineDead)
    {
        if(isMineDead)
        {
            enemyScoreText.text = GameManager.GetEnemyScore().ToString();
        }
        else
        {
            myScoreText.text = GameManager.GetMyScore().ToString();
        }
    }
}
