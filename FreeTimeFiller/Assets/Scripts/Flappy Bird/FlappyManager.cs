using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlappyManager : MonoBehaviour
{
    private int score;

    private void GameOver()
    {
        Debug.Log("Game Over");
    }

    private void IncreaseScore()
    {
        score++;
    }
}
