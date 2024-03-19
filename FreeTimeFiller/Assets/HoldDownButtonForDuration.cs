using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class HoldDownButtonForDuration : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerExitHandler
{
    // How long does user need to hold down the pointer on this button?
    [SerializeField, Range(0.5f, 3f)] private float holdTimer = 1f;
    private float _timeButtonHeld;

    private bool _isHoldingDown;

    private bool _heldLongEnough = false;
    private void Update()
    {
        // Check how long user has been holding down on this button
        if (_isHoldingDown && !_heldLongEnough)
        {
            _timeButtonHeld += Time.deltaTime;
        }
        else if(!_isHoldingDown)
        {
            _timeButtonHeld = 0f;
        }

        if (_timeButtonHeld >= holdTimer && !_heldLongEnough)
        {
            Debug.Log($"User held button for long enough! {holdTimer} seconds.");
            _heldLongEnough = true;
        }
    }
        
    // Called when the pointer is pressed down on the button
    public void OnPointerDown(PointerEventData eventData)
    {
        _isHoldingDown = true;
    }

    // Called when the pointer is released from the button
    public void OnPointerUp(PointerEventData eventData)
    {
        _isHoldingDown = false;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _isHoldingDown = false;
    }
}
