using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Snake : MonoBehaviour
{
    [SerializeField] private MinigameManager minigameManager;

    [Header("Animations")]
    [SerializeField] private GameObject respawnText;
    
    private Vector2 _direction;
    private SnakeMovementDirection _currentDirectionType;

    [SerializeField] private GameObject segmentPrefab;
    private List<GameObject> _segments = new List<GameObject>();

    // Besides head, how many segments should snake start with?
    private int _startingSegmentCount = 1;
    
    private bool _canMove;
    private bool _snakeIsMoving;

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

    private void OnDisable()
    {
        // Destroy all segments of the snake when disabled
        foreach(GameObject segment in _segments)
        {
            Destroy(segment);
        }

        _segments.Clear();
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
        _snakeIsMoving = false;
        transform.position = new Vector2(-1.75f, 0.25f);
        transform.rotation = Quaternion.Euler(0, 0, -90);
        _direction = Vector2.zero;
        
        _currentDirectionType = SnakeMovementDirection.Right;

        ResetSegments();
        
        _canMove = true;
    }

    ///-///////////////////////////////////////////////////////////
    /// Remove all segments from the snake.
    /// 
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
        for (int i = 0; i < _startingSegmentCount; i++)
        {
            AddSegment();
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Spawn a segment behind the snake.
    /// 
    private void AddSegment()
    {
        GameObject newSegment = Instantiate(segmentPrefab);

        newSegment.transform.position = _segments[_segments.Count - 1].transform.position;
        
        _segments.Add(newSegment);
    }

    ///-///////////////////////////////////////////////////////////
    /// WASD controls for the snake.
    /// 
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

        // Move the snake
        float x = transform.position.x + _direction.x;
        float y = transform.position.y + _direction.y;
        transform.position = new Vector2(x, y);
    }

    ///-///////////////////////////////////////////////////////////
    /// The segments of the snake will lag behind the snake.
    /// 
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
                _direction = new Vector2(0f, .5f);
                _currentDirectionType = SnakeMovementDirection.Up;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case SnakeMovementDirection.Down:
                if (_currentDirectionType == SnakeMovementDirection.Up) return;
                _direction = new Vector2(0f, -.5f);
                _currentDirectionType = SnakeMovementDirection.Down;
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case SnakeMovementDirection.Left:
                if (_currentDirectionType == SnakeMovementDirection.Right) return;
                _direction = new Vector2(-.5f, 0f);
                _currentDirectionType = SnakeMovementDirection.Left;
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case SnakeMovementDirection.Right:
                if (_currentDirectionType == SnakeMovementDirection.Left) return;
                _direction = new Vector2(.5f, 0f);
                _currentDirectionType = SnakeMovementDirection.Right;
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
        
        _snakeIsMoving = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // When snake hits an obstacle, the game ends
        if (other.gameObject.CompareTag("Obstacle") && _snakeIsMoving)
        {
            Debug.Log("Snake collided with obstacle! Reset the user.");

            StartCoroutine(LoseDelay());
        }
        // When snake touches collectable, add a segment
        else if (other.gameObject.CompareTag("Collectable"))
        {
            AddSegment();
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// When the snake collides with an obstacle, activate a small delay before it resets.
    /// 
    private IEnumerator LoseDelay()
    {
        _canMove = false;

        // Show "Respawning text" with animation
        if (respawnText != null)
            respawnText.SetActive(true);
                
        yield return new WaitForSecondsRealtime(2f);
        Reset();
    }
}
