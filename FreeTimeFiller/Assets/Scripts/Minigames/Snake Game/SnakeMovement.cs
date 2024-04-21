using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    private Vector2 _direction;

    [SerializeField, Range(1, 100f)] private float speed = 5f;

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        GetUserInput();
    }

    private void FixedUpdate()
    {
        MoveSnake();
    }

    private void Reset()
    {
        transform.position = new Vector2(-2, 0);
        transform.rotation = Quaternion.Euler(0, 0, -90);
        _direction = Vector2.right;
        Time.timeScale = 0.05f;
    }

    private void GetUserInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            _direction = Vector2.up;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _direction = Vector2.down;
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _direction = Vector2.right;
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            _direction = Vector2.left;
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    private void MoveSnake()
    {
        float x = transform.position.x + _direction.x;
        float y = transform.position.y + _direction.y;

        transform.position = new Vector2(x, y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // When snake hits an obstacle, the game ends
        if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Snake collided with obstacle! End the game.");
            Time.timeScale = 0f;
        }
    }
}
