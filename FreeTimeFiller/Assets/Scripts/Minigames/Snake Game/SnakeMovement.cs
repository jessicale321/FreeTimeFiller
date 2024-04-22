using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class SnakeMovement : MonoBehaviour
{
    private Vector2 _direction;

    [SerializeField, Range(1, 50f)] private float speed = 3f;

    public enum SnakeMovementDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        GetKeyboardInput();
    }

    private void FixedUpdate()
    {
        MoveSnake();
    }

    ///-///////////////////////////////////////////////////////////
    /// Reset the position of the snake.
    /// 
    private void Reset()
    {
        transform.position = new Vector2(-2, 0);
        transform.rotation = Quaternion.Euler(0, 0, -90);
        _direction = Vector2.right;
    }
    
    private void GetKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangeDirection(SnakeMovementDirection.Up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeDirection(SnakeMovementDirection.Down);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ChangeDirection(SnakeMovementDirection.Right);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeDirection(SnakeMovementDirection.Left);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Calculate a new position using speed for the snake to move towards.
    /// 
    private void MoveSnake()
    {
        float moveDistance = speed * Time.fixedDeltaTime;
        
        Vector2 newPosition = (Vector2) transform.position + _direction * moveDistance;
        
        transform.position = newPosition;
    }

    public void ChangeDirection(SnakeMovementDirection direction)
    {
        switch (direction)
        {
            case SnakeMovementDirection.Up:
                _direction = Vector2.up;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case SnakeMovementDirection.Down:
                _direction = Vector2.down;
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case SnakeMovementDirection.Left:
                _direction = Vector2.left;
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case SnakeMovementDirection.Right:
                _direction = Vector2.right;
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
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
