using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;

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
    [SerializeField] private Button deleteButton;

    // Folder path for location of Task Data
    private string _taskDataFolderPath = "Task Data";

    private List<string> _customTasks = new List<string>();

    private async void Awake()
    {
        // Sign in to account annoymously (* should use actual account login *)
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        LoadAllCustomTasks();
    }

    private void OnEnable()
    {
        // Add button functionality
        createButton.onClick.AddListener(AttemptCreation);
        loadButton.onClick.AddListener(LoadAllCustomTasks);
        deleteButton.onClick.AddListener(DeleteData);
    }

    private void OnDisable()
    {
        // Remove button functionality
        createButton.onClick.RemoveListener(AttemptCreation);
        loadButton.onClick.RemoveListener(LoadAllCustomTasks);
        deleteButton.onClick.RemoveListener(DeleteData);
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

        TaskManager.Instance.AddNewTaskToPool(newCustomTask);

        SaveToAssetFolder(newCustomTask);

        SaveData(json);
    }

    ///-///////////////////////////////////////////////////////////
    /// Save the custom task to the user's account.
    /// 
    public async void SaveData(string jsonText)
    {
        _customTasks.Add(jsonText);

        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "customTasks", _customTasks} });
    }

    ///-///////////////////////////////////////////////////////////
    /// Load all custom tasks and add them back to the task pool.
    /// 
    public async void LoadAllCustomTasks()
    {
        var savedList = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>
        {
            "customTasks"
        });
        
        // If there's data loaded, deserialize it back into a list of strings
        if (savedList.TryGetValue("customTasks", out var data))
        {
            List<string> stringList = data.Value.GetAs<List<string>>();

            // Append the loaded list to the existing list
            _customTasks.AddRange(stringList);
        }

        // Create an asset (temporary) for each custom task the user made
        foreach (string str in _customTasks)
        {
            TaskData newCustomTask = ScriptableObject.CreateInstance<TaskData>();

            JsonUtility.FromJsonOverwrite(str, newCustomTask);

            TaskManager.Instance.AddNewTaskToPool(newCustomTask);

            SaveToAssetFolder(newCustomTask);
        }

        
    }
    private async void DeleteData()
    {
        _customTasks.Clear();
        
        // Overwrite the data with an empty string to "delete" it
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
        { "customTasks", "" }
    });

            Debug.Log("Delete attempted");
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
