using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using System.Xml.Linq;

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
    [SerializeField] private Button loadButton;
    
    // Folder path for location of Task Data
    private string _taskDataFolderPath = "Task Data";

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void OnEnable()
    {
        createButton.onClick.AddListener(AttemptCreation);
        loadButton.onClick.AddListener(LoadData);
    }

    private void OnDisable()
    {
        createButton.onClick.RemoveListener(AttemptCreation);
        loadButton.onClick.RemoveListener(LoadData);
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
    /// Make a new custom task based off the values the user entered.
    /// 
    private void CreateCustomTask()
    {
        string taskName = taskNameInputField.text;
        string taskDescription = taskDescriptionInputField.text;

        // Create a new TaskData and add user's input to it
        TaskData newCustomTask = ScriptableObject.CreateInstance<TaskData>();
        newCustomTask.taskName = taskName;
        newCustomTask.description = taskDescription;
        newCustomTask.difficultyLevel = difficultySlider.GetDifficultyValue();
        newCustomTask.category = categoryDropdown.GetSelectedTaskCategory();
    
        // Convert the contents of the new TaskData to a json string
        string json = JsonUtility.ToJson(newCustomTask);

        // The new task will start appearing in the task pool
        TaskManager.Instance.AddNewTaskToPool(newCustomTask);

        SaveToAssetFolder(newCustomTask);

        SaveData(json);
    }

    ///-///////////////////////////////////////////////////////////
    /// Save the custom task to the user's account.
    /// 
    public async void SaveData(string jsonText)
    {
        var playerData = new Dictionary<string, object>{
          {"customTask", jsonText}
        };
        await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
        Debug.Log($"Saved data {string.Join(',', playerData)}");
    }

    ///-///////////////////////////////////////////////////////////
    /// Load all custom tasks and add them back to the task pool.
    /// 
    public async void LoadData()
    {
        var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "customTask" });

        string keyValue = "";

        if (playerData.TryGetValue("customTask", out var keyName))
        {
            keyValue = keyName.Value.GetAs<string>();
        }

        TaskData newCustomTask = ScriptableObject.CreateInstance<TaskData>();

        JsonUtility.FromJsonOverwrite(keyValue, newCustomTask);

        TaskManager.Instance.AddNewTaskToPool(newCustomTask);

        SaveToAssetFolder(newCustomTask);
    }

    ///-///////////////////////////////////////////////////////////
    /// Add custom task to the asset folder (will dissappear when builds are closed).
    /// 
    private void SaveToAssetFolder(TaskData dataToSave)
    {
        string path = $"Assets/Resources/Task Data/Custom/{dataToSave.taskName} Custom.asset";
        AssetDatabase.CreateAsset(dataToSave, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
