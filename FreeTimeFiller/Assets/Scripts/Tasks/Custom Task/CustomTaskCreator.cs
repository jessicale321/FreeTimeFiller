using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine.Serialization;

public class CustomTaskCreator : MonoBehaviour
{
    // Two different modes that the user can interact with, when dealing with custom tasks
    public enum MenuMode
    {
        // User is writing information to create a new, unique custom task
        Create,
        
        // User is editing an existing custom task to override it
        Edit
    }
    
    private MenuMode _currentMenuMode;
    
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField taskNameInputField;
    [SerializeField] private TMP_InputField taskDescriptionInputField;
    [Header("Dropdown")]
    [SerializeField] private CategoryDropdown categoryDropdown;
    [Header("Sliders")]
    [SerializeField] private DifficultySlider difficultySlider;
    [Header("Buttons")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button changeModeButton;
    private TMP_Text _changeModeButtonText;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button deleteButton;

    [SerializeField] private Transform creationPanel;
    [SerializeField] private Transform editPanel;
    [SerializeField] private GameObject existingCustomTaskPrefab;

    // The current custom task the user is editing
    private CustomTaskButton _currentCustomTaskEditing;

    // Folder path for location of Task Data
    private string _taskDataFolderPath = "Task Data";

    // The list of custom tasks that the user has created 
    private List<string> _customTasksAsJson;

    public List<TaskData> LoadedCustomTasks
    {
        get;
        private set;
    }

    private Dictionary<TaskData, CustomTaskButton> _loadedCustomTaskButtons =
        new Dictionary<TaskData, CustomTaskButton>();
    
    public event Action<TaskData> CustomTaskWasCreatedWithoutLoad;
    // When an existing custom task has had its information changed (passes reference to old name, and new task data)
    public event Action<string, TaskData> ExistingCustomTaskWasEdited;
    public event Action<TaskData> CustomTaskWasDeleted;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();

        _changeModeButtonText = changeModeButton.GetComponentInChildren<TMP_Text>();

        // Initialize lists
        _customTasksAsJson = new List<string>();
        LoadedCustomTasks = new List<TaskData>();
    }

    private void OnEnable()
    {
        // Add button functionality
        createButton.onClick.AddListener(AttemptCreation);
        changeModeButton.onClick.AddListener(ChangeModeOnButtonClick);
        resetButton.onClick.AddListener(ClearAllCustomTaskData);
        deleteButton.onClick.AddListener(DeleteOneCustomTaskData);
    }

    private void OnDisable()
    {
        // Remove button functionality
        createButton.onClick.RemoveListener(AttemptCreation);
        changeModeButton.onClick.RemoveListener(ChangeModeOnButtonClick);
        resetButton.onClick.RemoveListener(ClearAllCustomTaskData);
        deleteButton.onClick.RemoveListener(DeleteOneCustomTaskData);
    }

    private void Start()
    {
        SwitchToCreateMode();
    }

    #region Creation

    ///-///////////////////////////////////////////////////////////
    /// Check that the values the user entered for their custom task are valid. If so, allow the creation.
    /// 
    private void AttemptCreation()
    {
        string taskName = taskNameInputField.text;
        
        if (string.IsNullOrEmpty(taskName) || taskName == "")
        {
            Debug.Log("Task name is empty!");
            return;
        }
        
        switch (_currentMenuMode)
        {
            // Don't allow overriding when creating a new task
            case MenuMode.Create:
                if (!TaskNameIsUnique(taskName, null)) return;     
                CreateNewCustomTask();    
                break;
                
            // Allow overriding with the same name, but don't create a new task
            case MenuMode.Edit:
                if (_currentCustomTaskEditing == null)
                {
                    Debug.Log("There is no custom task currently being edited! Please select one!");
                    return;
                }
                
                if (!TaskNameIsUnique(taskName, _currentCustomTaskEditing.TaskData)) return;    
                
                OverrideExistingCustomTask(_currentCustomTaskEditing.TaskData);
                break;
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Check if a task name does not already exist.
    /// 
    private bool TaskNameIsUnique(string nameToCheck, TaskData taskData)
    {
        TaskData[] tasks = Resources.LoadAll<TaskData>(_taskDataFolderPath);

        foreach (TaskData data in tasks)
        {
            if (nameToCheck == data.taskName && taskData != data)
            {
                return false;
            }
        }
        return true;
    }
    
    ///-///////////////////////////////////////////////////////////
    /// Make a new custom task based off the values the user entered.
    /// 
    private void CreateNewCustomTask()
    {
        // Create a new TaskData and add user's input to it
        TaskData newCustomTask = ScriptableObject.CreateInstance<TaskData>();
        newCustomTask.taskName = taskNameInputField.text;
        newCustomTask.description = taskDescriptionInputField.text;
        newCustomTask.difficultyLevel = difficultySlider.GetDifficultyValue();
        newCustomTask.category = categoryDropdown.GetSelectedTaskCategory();
        
        // Add the new task to the task pool
        CustomTaskWasCreatedWithoutLoad?.Invoke(newCustomTask);

        SaveToAssetFolder(newCustomTask);

        SaveData(newCustomTask);
    }

    ///-///////////////////////////////////////////////////////////
    /// Change the contents of the passed in TaskData. 
    /// 
    private void OverrideExistingCustomTask(TaskData newTaskData)
    {
        string oldTaskName = newTaskData.taskName;

        newTaskData.taskName = taskNameInputField.text;
        newTaskData.description = taskDescriptionInputField.text;
        newTaskData.difficultyLevel = difficultySlider.GetDifficultyValue();
        newTaskData.category = categoryDropdown.GetSelectedTaskCategory();

        UpdateCustomTaskData(oldTaskName, newTaskData);

        // Update custom task button on screen to have a new name
        SpawnCustomTaskButton(newTaskData);
    }

    #endregion

    #region Loading

    ///-///////////////////////////////////////////////////////////
    /// Load all custom tasks and add them back to the task pool.
    /// 
    public async Task LoadAllCustomTasks()
    {
        List<string> savedCustomTasks = await DataManager.LoadData<List<string>>("customTasks");
        
        if(savedCustomTasks != null)
        {
            foreach(string customTask in savedCustomTasks)
            {
                // Don't allow duplicates to load in
                if (_customTasksAsJson.Contains(customTask)) return;

                else
                    _customTasksAsJson.Add(customTask);
            }
        }

        if (_customTasksAsJson.Count > 0)
        {
            // Create an asset (temporary, will disappear when app is closed) for each custom task the user made
            foreach (string str in _customTasksAsJson)
            {
                TaskData newCustomTask = ScriptableObject.CreateInstance<TaskData>();

                JsonUtility.FromJsonOverwrite(str, newCustomTask);

                LoadedCustomTasks.Add(newCustomTask);
                
                Debug.Log($"Custom Task Loaded From Cloud: {newCustomTask.taskName}");

                SaveToAssetFolder(newCustomTask);
            }
        }
        else
        {
            Debug.Log("Could not find any saved custom tasks.");
        }
    }

    #endregion

    #region Saving

    ///-///////////////////////////////////////////////////////////
    /// Save the custom task to the user's account.
    /// Override an existing custom task if the user is editing one, otherwise save a new one.
    /// 
    private async void SaveData(TaskData taskDataToSave)
    {
        // Convert the contents of the new TaskData to a json string
        string jsonText = JsonUtility.ToJson(taskDataToSave);
        
        // Don't add duplicates
        if (_customTasksAsJson.Contains(jsonText)) return;

        _customTasksAsJson.Add(jsonText);
        LoadedCustomTasks.Add(taskDataToSave);

        // Save list of custom tasks to the user's account
        await DataManager.SaveData("customTasks", _customTasksAsJson);
    }
    
    private async void UpdateCustomTaskData(string oldTaskName, TaskData newTaskData)
    {
        // Find the index of the JSON element containing the specified string
        int index = _customTasksAsJson.FindIndex(json => json.Contains(oldTaskName));

        // Check if the JSON element was found
        if (index != -1)
        {
            // Convert the new TaskData to a JSON string
            string updatedJsonText = JsonUtility.ToJson(newTaskData);

            // Update the JSON element at the found index with the new JSON data
            _customTasksAsJson[index] = updatedJsonText;

            Debug.Log($"Updated custom task data: {oldTaskName} to {newTaskData.taskName}");

            // Save the updated custom tasks list to Unity Cloud
            await DataManager.SaveData("customTasks", _customTasksAsJson);

            LoadedCustomTasks.Add(newTaskData);
            
            // Tell all listeners that the contents of an existing custom task has been edited
            ExistingCustomTaskWasEdited?.Invoke(oldTaskName, newTaskData);
        }
        else
        {
            Debug.Log("Couldn't find that custom task to update");
        }
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
    
    #endregion

    #region Deleting

    private async void DeleteOneCustomTaskData()
    {
        // Don't let user delete a custom task that doesn't exist
        if(_currentCustomTaskEditing == null)
        {
            Debug.Log("Cannot delete a custom task because there isn't an existing one selected!");
            return;
        }

        // Convert the contents of the new TaskData to a json string
        string jsonText = JsonUtility.ToJson(_currentCustomTaskEditing.TaskData);
        
        RemoveCustomTaskButton(_currentCustomTaskEditing.TaskData);

        _customTasksAsJson.Remove(jsonText);

        LoadedCustomTasks.Remove(_currentCustomTaskEditing.TaskData);

        // Save custom tasks that no longer contains the deleted task
        await DataManager.SaveData("customTasks", _customTasksAsJson);

        // Tell all listeners that a TaskData was deleted
        CustomTaskWasDeleted?.Invoke(_currentCustomTaskEditing.TaskData);

        // Go back to creation mode after deleting
        ChangeModeOnButtonClick();
    }

    ///-///////////////////////////////////////////////////////////
    /// Delete all custom task data from the user's account.
    /// 
    private async void ClearAllCustomTaskData()
    {
        // Remove all edit custom task buttons from edit screen
        foreach (TaskData deletedCustomTask in LoadedCustomTasks)
        {
            RemoveCustomTaskButton(deletedCustomTask);
        }
        
        _customTasksAsJson.Clear();
        LoadedCustomTasks.Clear();

        // Overwrite the data with an empty string to "delete" it
        await DataManager.DeleteAllDataByName("customTasks");

        Debug.Log("Custom task data was reset!");
    }

    ///-///////////////////////////////////////////////////////////
    /// When a custom task has been deleted, remove its button from the edit mode screen.
    /// 
    private void RemoveCustomTaskButton(TaskData taskDataDeleted)
    {
        if (_loadedCustomTaskButtons.ContainsKey(taskDataDeleted))
        {
            // Destroy the button found
            Destroy(_loadedCustomTaskButtons[taskDataDeleted].gameObject);
            
            _loadedCustomTaskButtons.Remove(taskDataDeleted);
        }
    }

    #endregion

    ///-///////////////////////////////////////////////////////////
    /// When clicking on the change mode button, switch to the other mode (Create -> Edit / Edit -> Create).
    /// 
    private void ChangeModeOnButtonClick()
    {
        switch (_currentMenuMode)
        {
            // Switch to edit mode, if in create mode
            case MenuMode.Create:
                SwitchToEditMode();
                break;
            // Switch to create mode, if in edit mode
            case MenuMode.Edit:
                SwitchToCreateMode();
                break;
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Manually switch to Create mode.
    /// 
    private void SwitchToCreateMode()
    {
        _currentMenuMode = MenuMode.Create;

        _changeModeButtonText.text = "Edit Mode";
        
        creationPanel.gameObject.SetActive(true);
        editPanel.gameObject.SetActive(false);
        // Don't allow user to delete a custom task in creation mode
        deleteButton.gameObject.SetActive(false);

        taskNameInputField.text = string.Empty;
        taskDescriptionInputField.text = string.Empty;
    }

    ///-///////////////////////////////////////////////////////////
    /// Manually switch to Edit mode.
    /// 
    private void SwitchToEditMode()
    {
        _currentMenuMode = MenuMode.Edit;

        _changeModeButtonText.text = "Creation Mode";
        
        creationPanel.gameObject.SetActive(false);
        editPanel.gameObject.SetActive(true);

        // For each custom task that the user has saved, make a button for it that they can click on to edit
        foreach (TaskData customTaskData in LoadedCustomTasks)
        {
            SpawnCustomTaskButton(customTaskData);
        }
    }

    private void SpawnCustomTaskButton(TaskData customTaskData)
    {
        if (!_loadedCustomTaskButtons.ContainsKey(customTaskData))
        {
            // Spawn a new button in, and place it on the edit panel
            GameObject newButton = Instantiate(existingCustomTaskPrefab, editPanel);
            CustomTaskButton customTaskButtonComponent = newButton.GetComponent<CustomTaskButton>();
            // Map the CustomTaskButton component to a TaskData (ex. Wash the dishes is mapped to a specific button)
            _loadedCustomTaskButtons[customTaskData] = customTaskButtonComponent;
        }
        _loadedCustomTaskButtons[customTaskData].UpdateCustomTaskButton(customTaskData, this);
    }

    ///-///////////////////////////////////////////////////////////
    /// Populate creation panel with information of the existing custom task that 
    /// the user selected. Also, allow them to delete the custom task.
    /// 
    public void EditExistingTask(CustomTaskButton customTaskButton)
    {
        _currentCustomTaskEditing = customTaskButton;
        TaskData currentCustomTaskDataEditing = customTaskButton.TaskData;
        
        creationPanel.gameObject.SetActive(true);
        editPanel.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(true);

        taskNameInputField.text = currentCustomTaskDataEditing.taskName;
        taskDescriptionInputField.text = currentCustomTaskDataEditing.description;
        categoryDropdown.SetSelectedCategory(currentCustomTaskDataEditing.category);
        difficultySlider.SetDifficulty(currentCustomTaskDataEditing.difficultyLevel);
    }
}
