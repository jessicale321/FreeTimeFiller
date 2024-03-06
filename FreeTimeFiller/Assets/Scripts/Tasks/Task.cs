using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UserTask
{
    public class Task : MonoBehaviour
    {
        // Data that this task is currently using
        [SerializeField] private TaskData taskData;

        private TaskPlacer _myTaskPlacer;

        [Header("UI Components")]
        [SerializeField] private TMP_Text taskName;

        [SerializeField] private Button checkBoxButton;

        [SerializeField] private GameObject crossOutImage;

        // Panel that stars are parented by
        [SerializeField] private GameObject difficultyLevelPanel;

        // Star that will spawn on the screen
        [SerializeField] private GameObject difficultyStar;
        // All stars currently spawned in
        private List<GameObject> _allStars = new List<GameObject>();

        private bool _isCompleted = false;

        private void Awake()
        {
            // Remove cross-out over this task
            crossOutImage.SetActive(false);
        }

        private void OnEnable()
        {
            checkBoxButton.onClick.AddListener(CompleteOnClick);
        }

        private void OnDisable()
        {
            checkBoxButton.onClick.RemoveListener(CompleteOnClick);
        }

        ///-///////////////////////////////////////////////////////////
        /// Place a cross-out image on top of this task if the task hasn't been completed yet.
        /// If the task was already completed, then uncomplete the task.
        /// 
        private void CompleteOnClick()
        {
            if (_isCompleted)
            {
                // Remove cross-out over this task, and enable its checkbox button
                crossOutImage.SetActive(false);

                _myTaskPlacer.UncompleteTask(this);

                _isCompleted = false;
            }
            else
            {
                MarkOff();
                // Tell TaskManager that this task has been completed
                _myTaskPlacer.CompleteTask(this);
            }
        }

        public void MarkOff()
        {
            // Show a cross-out over this task, and disable its checkbox button
            crossOutImage.SetActive(true);
            
            _isCompleted = true;
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
            crossOutImage.SetActive(false);

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
        /// Return the TaskData of this Task.
        /// 
        public TaskData GetCurrentTaskData()
        {
            return taskData;
        }
    }
}

