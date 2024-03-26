using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserTask
{
    public class Task : MonoBehaviour//, IPointerUpHandler ,IPointerDownHandler
    {
        // Data that this task is currently using
        [SerializeField] private TaskData taskData;

        private TaskPlacer _myTaskPlacer;

        [Header("UI Components")]
        [SerializeField] private TMP_Text taskName;

        [SerializeField] private Button checkBoxButton;

        [SerializeField] private GameObject crossOutImage;
        private float crossOutAnimationTimer = 0.15f;

        // Panel that stars are parented by
        [SerializeField] private GameObject difficultyLevelPanel;

        // Star that will spawn on the screen
        [SerializeField] private GameObject difficultyStar;
        // All stars currently spawned in
        private List<GameObject> _allStars = new List<GameObject>();

        private bool _isCompleted = false;
        private bool _isRewardable = true;

        private void OnEnable()
        {
            crossOutImage.SetActive(false);
            crossOutImage.transform.localScale = new Vector3(0f, crossOutImage.transform.localScale.y, crossOutImage.transform.localScale.z);
            
            checkBoxButton.onClick.AddListener(CompleteOnClick);

            if (_isCompleted)
            {
                PlayCrossOutAnimation(true);
            }
            else
            {
                PlayCrossOutAnimation(false);
            }
        }

        private void OnDisable()
        {
            checkBoxButton.onClick.RemoveListener(CompleteOnClick);
        }  

        ///-///////////////////////////////////////////////////////////
        /// Place a cross-out image on top of this task if the task hasn't been completed yet.
        /// If the task was already completed, then un-complete the task.
        /// 
        private void CompleteOnClick()
        {
            if (_isCompleted)
            {
                UncompleteOnCommand();
            }
            else
            {
                GiveRewardOnCompletion();
                CompleteOnCommand();
            }
        }

        ///-///////////////////////////////////////////////////////////
        /// Notify the TaskPlacer that this task has been completed, also place a cross-out image.
        /// 
        public void CompleteOnCommand()
        {
            // Show a cross-out over this task, and disable its checkbox button
            PlayCrossOutAnimation(true);
            
            _myTaskPlacer.CompleteTask(this);
            
            _isCompleted = true;

            //_isRewardable = false;
      
        }

        ///-///////////////////////////////////////////////////////////
        /// Notify the TaskPlacer that this task has been un-completed, also remove the cross-out image.
        /// 
        public void UncompleteOnCommand()
        {
            // Remove cross-out over this task, and enable its checkbox button
            PlayCrossOutAnimation(false);

            _myTaskPlacer.UncompleteTask(this);

            _isCompleted = false;

            Debug.Log($"Take away {taskData.GetRewardAmount()} coins from the user.");
        }

        public void GiveRewardOnCompletion()
        {
            //if(_isRewardable)
            Debug.Log($"Give the user {taskData.GetRewardAmount()} coins.");
        }

        ///-///////////////////////////////////////////////////////////
        /// Change displayed values of the task.
        /// 
        public void UpdateTask(TaskData data, TaskPlacer taskPlacer)
        {
            taskData = data;

            // Change task name
            taskName.text = data.taskName;

            _myTaskPlacer = taskPlacer;

            // Remove cross-out over this task
            PlayCrossOutAnimation(false);

            DisplayStars(data);
        }

        ///-///////////////////////////////////////////////////////////
        /// Remove or add stars being displayed on top of a task.
        /// 
        private void DisplayStars(TaskData data)
        {
            int starCount = _allStars.Count;

            // Remove excess stars
            if (starCount > data.difficultyLevel)
            {
                for (int i = 0; i < starCount - data.difficultyLevel; i++)
                {
                    Destroy(_allStars[0]);
                    _allStars.Remove(_allStars[0]);

                }
            }
            // Add missing stars
            else if (starCount < data.difficultyLevel)
            {
                for (int i = 0; i < data.difficultyLevel - starCount; i++)
                {
                    GameObject newStar = Instantiate(difficultyStar, difficultyLevelPanel.transform, false);
                    _allStars.Add(newStar);
                }
            }
        }

        ///-///////////////////////////////////////////////////////////
        /// Show cross out bar over this task when the user clicks on the checkbox.
        /// If this task is already crossed out, remove the cross out bar.
        /// 
        private void PlayCrossOutAnimation(bool isCrossedOut)
        {
            crossOutImage.SetActive(true);

            if (isCrossedOut)
                LeanTween.scaleX(crossOutImage, 1f, crossOutAnimationTimer);
            else
                LeanTween.scaleX(crossOutImage, 0f, crossOutAnimationTimer);
        }

        ///-///////////////////////////////////////////////////////////
        /// Delete this task and place a new one on the screen.
        /// 
        public void ReplaceThisTaskForCurrency()
        {
            // Tell TaskPlacer to remove this task from display (will be replaced by a different task)
            _myTaskPlacer.RemoveTaskFromDisplay(taskData);
        }

        ///-///////////////////////////////////////////////////////////
        /// Return the TaskData of this Task.
        /// 
        public TaskData GetCurrentTaskData()
        {
            return taskData;
        }
    }
}

