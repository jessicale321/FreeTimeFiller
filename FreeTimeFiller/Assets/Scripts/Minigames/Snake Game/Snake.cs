using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Snake : MonoBehaviour
{
    [SerializeField] private MinigameManager minigameManager;
    
    private Vector2 _direction;
    private SnakeMovementDirection _currentDirectionType;

    [SerializeField] private GameObject segmentPrefab;
    private List<GameObject> _segments = new List<GameObject>();

    [SerializeField, Range(1, 50f)] private float speed = 3f;

    private bool _canMove;
    
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
        MoveSegments();
        MoveSnake();
    }

    ///-///////////////////////////////////////////////////////////
    /// Reset the position of the snake.
    /// 
    private void Reset()
    {
        transform.position = new Vector2(-1, 0);
        transform.rotation = Quaternion.Euler(0, 0, -90);
        ChangeDirection(SnakeMovementDirection.Right);
        
        ResetSegments();
        
        _canMove = true;
    }

    private void ResetSegments()
    {
        for (int i = 1; i < _segments.Count; i++) 
        {
            Destroy(_segments[i].gameObject);
        }
        
        _segments.Clear();
        // Add head
        _segments.Add(gameObject);

        // Put initial segments on top of the head
        for (int i = 0; i < 3; i++)
        {
            AddSegment();
        }
    }

    private void AddSegment()
    {
        GameObject newSegment = Instantiate(segmentPrefab);

        newSegment.transform.position = _segments[_segments.Count - 1].transform.position;
        
        _segments.Add(newSegment);
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
        if (!_canMove) return;
        
        float moveDistance = speed * Time.fixedDeltaTime;
        
        Vector2 newPosition = (Vector2) transform.position + _direction * moveDistance;
        
        transform.position = newPosition;
    }

    private void MoveSegments()
    {
        if (!_canMove) return;
        
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            // Calculate the position of the current segment based on the position of the previous segment
            Vector2 newPosition = _segments[i - 1].transform.position;

            // Move the segment to the new position
            _segments[i].transform.position = newPosition;
        }
    }

    
    ///-///////////////////////////////////////////////////////////
    /// Move the snake in a different direction, but don't allow changing to the opposite direction.
    /// 
    public void ChangeDirection(SnakeMovementDirection direction)
    {
        if (!_canMove) return;
        
        switch (direction)
        {
            case SnakeMovementDirection.Up:
                if (_currentDirectionType == SnakeMovementDirection.Down) return;
                _direction = Vector2.up;
                _currentDirectionType = SnakeMovementDirection.Up;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case SnakeMovementDirection.Down:
                if (_currentDirectionType == SnakeMovementDirection.Up) return;
                _direction = Vector2.down;
                _currentDirectionType = SnakeMovementDirection.Down;
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case SnakeMovementDirection.Left:
                if (_currentDirectionType == SnakeMovementDirection.Right) return;
                _direction = Vector2.left;
                _currentDirectionType = SnakeMovementDirection.Left;
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case SnakeMovementDirection.Right:
                if (_currentDirectionType == SnakeMovementDirection.Left) return;
                _direction = Vector2.right;
                _currentDirectionType = SnakeMovementDirection.Right;
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
            _canMove = false;
            
            // TODO: Add delay or fade for this
            minigameManager.OnMinigameConcluded();
        }
        // When snake touches collectable, add a segment
        else if (other.gameObject.CompareTag("Collectable"))
        {
            AddSegment();
        }
    }
}
