using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private void OnEnable()
    {
        // Remove cross-out over this task, and enable its checkbox button
        crossOutImage.SetActive(false);
        checkBoxButton.enabled = true;
        
        checkBoxButton.onClick.AddListener(Complete);
    }

    private void OnDisable()
    {
        checkBoxButton.onClick.RemoveListener(Complete);
    }
    
    private void Complete()
    {
        Debug.Log($"{taskData.taskName} has been completed!");

        // Show a cross-out over this task, and disable its checkbox button
        crossOutImage.SetActive(true);
        checkBoxButton.enabled = false;
        
        // Tell TaskManager that this task has been completed
        TaskManager.Instance.CompleteTask(this);
    }

    public void UpdateTask(TaskData data)
    {
        taskData = data;
        
        // Change task name
        taskName.text = data.taskName;
        
        DisplayStars(data);
        
    }

    /* Remove or add stars being displayed on top of a task */
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
                GameObject newStar =  Instantiate(difficultyStar, difficultyLevelPanel.transform, false);
                _allStars.Add(newStar);

            }
        }
    }

    public TaskData GetCurrentTaskData()
    {
        return taskData;
    }

    
    
}
