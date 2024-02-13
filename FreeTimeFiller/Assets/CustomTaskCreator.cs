using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CustomTaskCreator : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField taskNameInputField;
    [SerializeField] private TMP_InputField taskDescriptionInputField;
    [Header("Dropdown")]
    [SerializeField] private CategoryDropdown categoryDropdown;
    [Header("Sliders")]
    [SerializeField] private DifficultySlider difficultySlider;
    [Header("Buttons")]
    [SerializeField] private Button createButton;
    
    // Folder path for location of Task Data
    private string _taskDataFolderPath = "Task Data";

    private void OnEnable()
    {
        createButton.onClick.AddListener(AttemptCreation);
    }

    private void OnDisable()
    {
        createButton.onClick.RemoveListener(AttemptCreation);
    }

    // Check that the values the user entered for their custom task are valid. If so, allow the creation
    private void AttemptCreation()
    {
        string taskName = taskNameInputField.text;

        // Don't allow task names that have no actual text, or already exist
        if ((string.IsNullOrEmpty(taskName) || taskName == "") || !TaskNameIsUnique(taskName))
        {
            Debug.Log("Task name is empty or already exists!");
            return;
        }

        CreateCustomTask();
    }

    ///-///////////////////////////////////////////////////////////
    /// Check if a task name does not already exist
    /// 
    private bool TaskNameIsUnique(string nameToCheck)
    {
        TaskData[] tasks =  Resources.LoadAll<TaskData>(_taskDataFolderPath);

        foreach (TaskData data in tasks)
        {
            if (nameToCheck == data.taskName)
            {
                return false;
            }

        }
        return true;
    }

    ///-///////////////////////////////////////////////////////////
    /// Make a new custom task based off the values the user entered. Save the custom task to the user's account
    /// 
    private void CreateCustomTask()
    {
        string taskName = taskNameInputField.text;
        string taskDescription = taskDescriptionInputField.text;

        TaskData newCustomTask = ScriptableObject.CreateInstance<TaskData>();

        newCustomTask.taskName = taskName;
        newCustomTask.description = taskDescription;
        newCustomTask.difficultyLevel = difficultySlider.GetDifficultyValue();
        newCustomTask.category = categoryDropdown.GetSelectedTaskCategory();
        
        // TESTING CUSTOM TASK CREATION
        Debug.Log($"User entered -> Name: {newCustomTask.taskName}, Difficulty: {newCustomTask.difficultyLevel}, " +
            $"Category: {newCustomTask.category}, Description: {newCustomTask.description}");

        string path = $"Assets/Resources/Task Data/Custom/{taskName} Custom.asset";
        AssetDatabase.CreateAsset(newCustomTask, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newCustomTask;
        
        TaskManager.Instance.AddNewTaskToPool(newCustomTask);
    }
}
