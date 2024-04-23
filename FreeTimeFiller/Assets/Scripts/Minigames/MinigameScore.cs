using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinigameScore : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private int _currentScore;

    private void Start()
    {
        DisplayScore();
    }

    public void ModifyScore(int amount)
    {
        _currentScore += amount;
        
        DisplayScore();
    }

    private void DisplayScore()
    {
        scoreText.text = _currentScore.ToString();
    }
}
