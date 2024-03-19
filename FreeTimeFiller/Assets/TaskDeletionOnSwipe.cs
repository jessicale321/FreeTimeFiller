using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TaskDeletionOnSwipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Vector2 _startPosition;
    private bool _isDragging;

    private bool _swipingLeft;
    private bool _swipingRight;
    
    // How much does user need to swipe this?
    [SerializeField, Range(5, 50f)] private float swipeThreshold = 5f;

    private void OnEnable()
    {
        _swipingLeft = false;
        _swipingLeft = false;
    }

    // Called when the pointer is pressed down on the button
    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
        _startPosition = eventData.position;
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

        Vector2 direction = eventData.position - _startPosition;
        float distance = direction.magnitude;

        if (distance >= swipeThreshold)
        {
            // Check if user swiped right
            if (direction.x > 0 && !_swipingRight)
            {
                Debug.Log("Swiped right on delete button");
                _swipingRight = true;
                _swipingLeft = false;
            }
            // Check if user swiped left
            else if (direction.x < 0 && !_swipingLeft)
            {
                Debug.Log("Swiped left on delete button");
                _swipingLeft = true;
                _swipingRight = false;
            }

            // Reset isDragging after detecting a swipe
            _isDragging = false;
        }
    }
}
