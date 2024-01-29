using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Task : MonoBehaviour
{
    [SerializeField] private TaskData taskData;

    [Header("UI Components")] 
    [SerializeField] private TMP_Text taskName;

    [SerializeField] private GameObject difficultyLevelPanel;
    [SerializeField] private GameObject difficultyStar;
    
    private void Start()
    {
        if(taskData != null)
            UpdateTask(taskData);
    }

    private void UpdateTask(TaskData data)
    {
        taskName.text = data.taskName;

        for (int i = 0; i < data.difficultyLevel; i++)
        {
            Instantiate(difficultyStar, difficultyLevelPanel.transform, false);
        }
    }
}
