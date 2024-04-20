using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnakeFood : MonoBehaviour
{
    private void Start()
    {
        SetRandomPosition();
    }

    private void SetRandomPosition()
    {
        int x = Random.Range(-8, 8);
        int y = Random.Range(-8, 6);

        transform.position = new Vector2(x, y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SetRandomPosition();
    }
}
