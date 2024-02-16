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

    // The list of custom tasks that the user has created 
    private List<string> _customTasks = new List<string>();

    private async void Awake()
    {
        // Sign in to account anonymously (* should use actual account login *)
        await UnityServices.InitializeAsync();
        //await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        // Load and then give all existing custom tasks to the TaskManager
        AuthenticationService.Instance.SignedIn +=  LoadAllCustomTasks;
        
    }

    private void OnEnable()
    {
        // Add button functionality
        createButton.onClick.AddListener(AttemptCreation);
        loadButton.onClick.AddListener(LoadAllCustomTasks);
        deleteButton.onClick.AddListener(ClearAllCustomTaskData);
    }

    private void OnDisable()
    {
        // Remove button functionality
        createButton.onClick.RemoveListener(AttemptCreation);
        loadButton.onClick.RemoveListener(LoadAllCustomTasks);
        deleteButton.onClick.RemoveListener(ClearAllCustomTaskData);
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
            // If we find a duplicate task name, don't allow the creation
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

        // Add the new task to the task pool
        //TaskManager.Instance.AddNewTaskToPool(newCustomTask);

        SaveToAssetFolder(newCustomTask);

        SaveData(json);
    }

    ///-///////////////////////////////////////////////////////////
    /// Save the custom task to the user's account.
    /// 
    public async void SaveData(string jsonText)
    {
        // Don't add duplicates
        if (_customTasks.Contains(jsonText)) return;
        
        _customTasks.Add(jsonText);

        // Save list of custom tasks to the user's account
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "customTasks", _customTasks} });
    }

    ///-///////////////////////////////////////////////////////////
    /// Load all custom tasks and add them back to the task pool.
    /// 
    public async void LoadAllCustomTasks()
    {
        // Load the list of custom tasks created by the user from their cloud account
        var savedList = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>
        {
            "customTasks"
        });
        
        // If there's data loaded, deserialize it back into a list of strings
        if (savedList.TryGetValue("customTasks", out var data))
        {
            // Don't allow duplicates to load in
            if (_customTasks.Contains(data.Value.GetAsString())) return;
            
            List<string> stringList = data.Value.GetAs<List<string>>();

            // Append the loaded list to the existing list
            _customTasks.AddRange(stringList);
        }

        // Create an asset (temporary, will disappear when app is closed) for each custom task the user made
        foreach (string str in _customTasks)
        {
            TaskData newCustomTask = ScriptableObject.CreateInstance<TaskData>();

            JsonUtility.FromJsonOverwrite(str, newCustomTask);

            //TaskManager.Instance.AddNewTaskToPool(newCustomTask);

            SaveToAssetFolder(newCustomTask);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Delete all custom task data from the user's account.
    /// 
    private async void ClearAllCustomTaskData()
    {
        _customTasks.Clear();
        
        // Overwrite the data with an empty string to "delete" it
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
        { "customTasks", "" }
    });

            Debug.Log("Delete attempted");
    }

    ///-///////////////////////////////////////////////////////////
    /// Add custom task to the asset folder (will disappear when builds are closed).
    /// 
    private void SaveToAssetFolder(TaskData dataToSave)
    {
        // Find location of where Task Data are saved, then place the data inside
        string path = $"Assets/Resources/Task Data/Custom/{dataToSave.taskName} Custom.asset";
        AssetDatabase.CreateAsset(dataToSave, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
