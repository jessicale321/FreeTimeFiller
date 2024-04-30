using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnakeFood : MonoBehaviour
{
    [SerializeField] private MinigameScore minigameScore;
    [SerializeField] private CheckmarkAnimation checkmarkAnimation;
    private void Start()
    {
        SetRandomPosition();
    }

    ///-///////////////////////////////////////////////////////////
    /// Food will teleport to a random location on start, and whenever the snake collides with it.
    /// 
    private void SetRandomPosition()
    {
        float x = Random.Range(-2f, 2f);
        float y = Random.Range(-3.5f, 3.5f);

        transform.position = new Vector2(x, y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Update score when collecting this
            minigameScore.ModifyScore(1);
            SetRandomPosition();
            
            if(checkmarkAnimation != null)
                checkmarkAnimation.PlayAnimation();
        }
    }
}
