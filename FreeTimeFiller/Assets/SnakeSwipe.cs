using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SnakeSwipe : MonoBehaviour, IPointerUpHandler, IPointerDownHandler ,IDragHandler
{
    [SerializeField] private SnakeMovement snakeMovement;
    
    private bool _isDragging;
    private Vector2 _pointerStartPosition;
    
    // How much does user need to swipe this?
    [SerializeField, Range(5, 50f)] private float swipeThreshold = 5f;
    
    // Called when the pointer is pressed down on the button
    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
        _pointerStartPosition = eventData.position;
    }
    
    // Called when the pointer is released from the button
    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
    }

    // Called when the button is being dragged
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        Vector2 direction = eventData.position - _pointerStartPosition;
        
        // Get length of the swipe
        float distance = direction.magnitude;

        // Is the drag length high enough (i.e. did the user swipe far enough?)
        if (distance >= swipeThreshold)
        {
            // Calculate the angle of the swipe direction vector
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Determine the swipe direction based on the angle (makes swiping in directions not too sensitive)
            // Ex. Fixes swiping left when trying to swipe down
            if (angle > -45 && angle <= 45)
            {
                Debug.Log("Snake swipe right");
                snakeMovement.ChangeDirection(SnakeMovement.SnakeMovementDirection.Right);
            }
            else if (angle > 45 && angle <= 135)
            {
                Debug.Log("Snake swipe up");
                snakeMovement.ChangeDirection(SnakeMovement.SnakeMovementDirection.Up);
            }
            else if (angle > -135 && angle <= -45)
            {
                Debug.Log("Snake swipe down");
                snakeMovement.ChangeDirection(SnakeMovement.SnakeMovementDirection.Down);
            }
            else
            {
                Debug.Log("Snake swipe left");
                snakeMovement.ChangeDirection(SnakeMovement.SnakeMovementDirection.Left);
            }

            // Reset isDragging after detecting a swipe
            _isDragging = false;
        }

    }
}
