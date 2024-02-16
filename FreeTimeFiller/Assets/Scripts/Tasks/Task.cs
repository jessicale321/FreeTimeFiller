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

        private void OnEnable()
        {
            checkBoxButton.onClick.AddListener(Complete);
        }

        private void OnDisable()
        {
            checkBoxButton.onClick.RemoveListener(Complete);
        }

        private void Start()
        {
            // Remove cross-out over this task
            crossOutImage.SetActive(false);
        }

        ///-///////////////////////////////////////////////////////////
        /// Place a cross-out image on top of this task if the task hasn't been completed yet.
        /// If the task was already completed, then uncomplete the task.
        /// 
        private void Complete()
        {
            if (_isCompleted)
            {
                // Remove cross-out over this task, and enable its checkbox button
                crossOutImage.SetActive(false);

                TaskPlacer.Instance.UncompleteTask(this);

                _isCompleted = false;
            }
            else
            {
                Debug.Log($"{taskData.taskName} has been completed!");

                // Show a cross-out over this task, and disable its checkbox button
                crossOutImage.SetActive(true);


                // Tell TaskManager that this task has been completed
                TaskPlacer.Instance.CompleteTask(this);

                _isCompleted = true;
            }
        }

        ///-///////////////////////////////////////////////////////////
        /// Change displayed values of the task.
        /// 
        public void UpdateTask(TaskData data)
        {
            taskData = data;

            // Change task name
            taskName.text = data.taskName;

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

