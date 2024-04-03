using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TaskDeletionOnSwipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private UserTask.Task _myTask;
    private TaskData _myTaskData;
    
    private Button _deleteButton;
    [SerializeField] private GameObject trashCanImage;
    [SerializeField] private TMP_Text deletePriceText;
    
    [Header("Delete Button Animation")]
    [SerializeField, Range(2, 8)] private float deleteButtonStretchX;

    [SerializeField, Range(0.1f, .5f)] private float stretchTimer = 0.25f;
    // Set the delete button's X scale to 0 when we swipe right
    private float _rightSwipeScaleX = 0f;
    [SerializeField] private Vector3 errorShakeDirection;

    [SerializeField, Range(0.05f, 0.5f)] private float errorShakeDuration; 

    private Vector2 _startPosition;
    private bool _isDragging;

    private bool _swipingLeft;
    private bool _swipingRight;

    private bool _canDelete = true;
    
    // How much does user need to swipe this?
    [SerializeField, Range(5, 50f)] private float swipeThreshold = 5f;

    private void Awake()
    {
        _deleteButton = GetComponentInChildren<Button>();
        _myTask = GetComponentInParent<UserTask.Task>();
    }

    private void OnEnable()
    {
        _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        
        // Don't allow button click until swiping left;
        _deleteButton.interactable = false;
        trashCanImage.SetActive(false);
        deletePriceText.gameObject.SetActive(false);
        // Shrink the delete button 
        _deleteButton.transform.localScale = new Vector3(_rightSwipeScaleX, _deleteButton.transform.localScale.y, _deleteButton.transform.localScale.z);

        trashCanImage.SetActive(false);
        _swipingLeft = false;
        _swipingLeft = false;

        _myTask.onCompletion += OnTaskCompleted;
        _myTask.onUncompletion += OnTaskUncompleted;
    }

    private void OnDisable()
    {
        _deleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
        
        _deleteButton.interactable = false;
        trashCanImage.SetActive(false);
        _swipingLeft = false;
        _swipingLeft = false;

        _myTask.onCompletion -= OnTaskCompleted;
        _myTask.onUncompletion -= OnTaskUncompleted;
    }

    private void Start()
    {
        _myTaskData = _myTask.GetCurrentTaskData();
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
        // Magnitude of the direction means length
        float distance = direction.magnitude;

        // Is the drag length high enough (i.e. did the user swipe far enough?)
        if (distance >= swipeThreshold)
        {
            // Check if user swiped right
            if (direction.x > 0 && !_swipingRight)
            {
                OnSwipeRight();
            }
            // Check if user swiped left
            else if (direction.x < 0 && !_swipingLeft)
            {
                OnSwipeLeft();
            }

            // Reset isDragging after detecting a swipe
            _isDragging = false;
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// When the user swipes left, show the deletion button.
    /// 
    private void OnSwipeLeft()
    {
        if (!_canDelete) return;

        Debug.Log("Swiped left on delete button");
        _swipingLeft = true;
        _swipingRight = false;
        
        _deleteButton.interactable = true;

        // Show trash can image when delete button is finished stretching out
        LeanTween.scaleX(_deleteButton.gameObject, deleteButtonStretchX, stretchTimer).setOnComplete(() => {
            trashCanImage.SetActive(true);
            deletePriceText.gameObject.SetActive(true);
        });

        deletePriceText.text = "$" + _myTaskData.GetDeletePrice().ToString();
        Debug.Log($"{_myTaskData.taskName} costs {_myTaskData.GetDeletePrice()} to delete!");
    }

    ///-///////////////////////////////////////////////////////////
    /// When the user swipes right, hide the deletion button.
    /// 
    private void OnSwipeRight()
    {
        Debug.Log("Swiped right on delete button");
        _swipingRight = true;
        _swipingLeft = false;
        
        _deleteButton.interactable = false;
        trashCanImage.SetActive(false);
        deletePriceText.gameObject.SetActive(false);

        LeanTween.scaleX(_deleteButton.gameObject, _rightSwipeScaleX, stretchTimer);
    }

    ///-///////////////////////////////////////////////////////////
    /// When user clicks on delete button, then ask for currency. If the user has enough currency,
    /// tell the task to replace itself.
    /// 
    private void OnDeleteButtonClicked()
    {
        // TODO: Need to check for currency!
        if (_myTask != null)
        {
            Debug.Log($"Task delete button was clicked for: {_myTask.GetCurrentTaskData().taskName}");
            Vector3 originalPosition = _myTask.gameObject.transform.position;

            // Move the task button around to showcase that an error occurred
            LeanTween.move(_myTask.gameObject, originalPosition + errorShakeDirection, errorShakeDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setLoopPingPong(3)
                .setOnComplete(() => {
                    // Return to original position
                    Vector3 returnPosition = originalPosition;
                    LeanTween.move(_myTask.gameObject, returnPosition, 0f);
                });
        }
        
        //AchievementManager.Instance.UpdateProgress(AchievementConditionType.TasksDeleted);
    }

    private void OnTaskCompleted()
    {
        _canDelete = false;
        OnSwipeRight();
    }

    private void OnTaskUncompleted()
    {
        _canDelete = true;
    }
}
