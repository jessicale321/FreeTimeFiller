using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using Random = System.Random;

public class TaskPlacer : MonoBehaviour
{
    // Tasks that will show up in the pool
    private List<TaskData> _displayableTasks = new List<TaskData>();

    // Folder path for location of Task Data
    private string _taskDataFolderPath = "Task Data/Premade";

    /* How many tasks should be created and displayed?
     * If taskCount is greater than the number of inactive TaskData's, 
     * then we will get an error in the console (CONSTANT value)
     */
    [SerializeField, Range(1, 8)] private int maxTaskDisplay;

    private int _amountAllowedToDisplay;

    // How many should we display (this is number decrements by 1 on each refresh)
    private int _amountCurrentlyDisplayed = 0;

    /* Tasks that are currently being displayed
     * Key: The TaskData (ex. Brushing Teeth)
     * Value: Is currently displayed? True or false
     */
    private Dictionary<TaskData, bool> _activeTaskData = new Dictionary<TaskData, bool>();

    /* Tasks that are currently in the pool, organized by their task category
     * Key: A Task Category (ex. Chores)
     * Value: A list of task data that are in the pool that belong to the key category (ex. Wash the dishes)
     */
    private Dictionary<TaskCategory, List<TaskData>> _allDisplayableTaskDataByCategory = new Dictionary<TaskCategory, List<TaskData>>();

    private Dictionary<TaskData, UserTask.Task> _tasksDisplayed = new Dictionary<TaskData, UserTask.Task>();

    /* Task Data that are displayable
     * Key: The TaskData
     * Value: The name of the TaskData (ex. Wash the dishes)
     */
    private Dictionary<string, TaskData> _allDisplayableTaskDataByName = new Dictionary<string, TaskData>();

    /* Task Data that have been previously completed
     * Key: The TaskData (ex. Brushing Teeth)
     * Value: Number of refreshes left until it reappears
     */
    private Dictionary<TaskData, int> _taskDataOnHold = new Dictionary<TaskData, int>();

    private List<UserTask.Task> _completedTasks = new List<UserTask.Task>();

    // Task that will be instantiated and placed on the canvas
    [SerializeField] private GameObject taskPrefab;
    [SerializeField] private Transform taskPanel;

    #region Adding

    ///-///////////////////////////////////////////////////////////
    /// Load all pre-made tasks found under the resources folder
    /// 
    public void FindPremadeTasks(List<TaskCategory> userChosenCategories)
    {
        TaskData[] tasks =  Resources.LoadAll<TaskData>(_taskDataFolderPath);

        foreach (TaskData data in tasks)
        {
            // Only add tasks based on the user's Task Category preferences
            TryAddTask(data, userChosenCategories);
        }
    }
    
    public bool TryAddTask(TaskData data, List<TaskCategory> userChosenCategories)
    {
        // Filter out TaskData that the user doesn't prefer to see
        if (!userChosenCategories.Contains(data.category)) return false;
        
        // Don't add any duplicate task data
        if (_activeTaskData.TryAdd(data, false) == false || CheckTaskNameUniqueness(data))
        {
            Debug.Log($"{data.taskName} is a duplicate in the taskData list!");
        }
        else
        {
            _displayableTasks.Add(data);
            _allDisplayableTaskDataByName.Add(data.taskName, data);

            // Check if the category already exists in the dictionary
            if (_allDisplayableTaskDataByCategory.ContainsKey(data.category))
            {
                // If the category exists, add the task data to the existing list
                _allDisplayableTaskDataByCategory[data.category].Add(data);
            }
            else
            {
                // If the category doesn't exist, create a new list and add it to the dictionary
                // Then add the task data to the newly created list
                _allDisplayableTaskDataByCategory[data.category] = new List<TaskData>
                {
                    data
                };
            }
            Debug.Log($"Task Placer can display: {data.taskName}");
            return true;
        }
        
        return false;
    }
    
    ///-///////////////////////////////////////////////////////////
    /// Check if this task is already in the pool. We check by name since the scriptableObjects don't always exist.
    /// 
    private bool CheckTaskNameUniqueness(TaskData taskData)
    {
        return _allDisplayableTaskDataByName.ContainsKey(taskData.taskName);
    }

    #endregion

    ///-///////////////////////////////////////////////////////////
    /// When TaskCategory preferences have changed, check if any displayable tasks need to be removed. The user 
    /// may have decided they don't want to see certain tasks anymore.
    /// 
    public void RemoveTaskDataByCategories(List<TaskCategory> userChosenCategories)
    {
        foreach (TaskCategory category in _allDisplayableTaskDataByCategory.Keys)
        {
            if (!userChosenCategories.Contains(category))
            {
                Debug.Log($"{category} was removed! Remove all of its tasks from display!");

                foreach (TaskData dataFromRemovedCategory in _allDisplayableTaskDataByCategory[category])
                {
                    RemoveTaskFromDisplay(dataFromRemovedCategory);

                    Debug.Log($"Removed by category: {dataFromRemovedCategory.taskName}");
                }
                _allDisplayableTaskDataByCategory[category].Clear();
            }
        } 
    }

    ///-///////////////////////////////////////////////////////////
    /// When a task has been edited, check to see if its category is no longer apart of the user's chosen
    /// task categories. If it's not, then remove it from all placements.
    public void ExistingTaskDataWasUpdated(TaskData taskDataEdited, List<TaskCategory> userChosenCategories)
    {
        // If this edited TaskData was displayable...
        if (_activeTaskData.ContainsKey(taskDataEdited))
        {
            // Update Task button's information displayed
            _tasksDisplayed[taskDataEdited].UpdateTask(taskDataEdited, this);
            
            // If the task was in the pool, but its category was changed to a category that the user doesn't use. Remove it from placement.
            if (!userChosenCategories.Contains(taskDataEdited.category))
            {
                RemoveTaskFromDisplay(taskDataEdited);
            }
        }
        // If the edited TaskData was not previously displayable, try to add it
        else if (userChosenCategories.Contains(taskDataEdited.category))
        {
            TryAddTask(taskDataEdited, userChosenCategories);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Remove a TaskData from any future displaying on the screen.
    /// 
    public void RemoveTaskFromDisplay(TaskData dataToRemove)
    {
        if(_allDisplayableTaskDataByCategory.ContainsKey(dataToRemove.category))
            _allDisplayableTaskDataByCategory[dataToRemove.category].Remove(dataToRemove);
        
        _displayableTasks.Remove(dataToRemove);
        _allDisplayableTaskDataByName.Remove(dataToRemove.taskName);
        _activeTaskData.Remove(dataToRemove);
        _taskDataOnHold.Remove(dataToRemove);
    }

    #region Displaying

    ///-///////////////////////////////////////////////////////////
    /// Find a TaskData that is not being displayed currently, and create
    /// a new task using its values
    /// 
    private void DisplayAllTasks()
    {
        // Randomize all elements in the task pool
        ShuffleTaskList(_displayableTasks);
        
        for(int i = 0; i < _amountAllowedToDisplay; i++)
        {
            // Don't allow more tasks to be displayed, if we reached the current max amount
            if (_amountCurrentlyDisplayed >= _amountAllowedToDisplay) return;

            TaskData inactiveData = GetInactiveTask();
            
            SpawnTask(inactiveData);
        }
        
        SaveTaskPlacement();
    }

    private void SpawnTask(TaskData data)
    {
        if (data == null) return;
        
        // Spawn a new task and give it data
        GameObject newTask = Instantiate(taskPrefab, taskPanel);
        UserTask.Task taskComponent = newTask.GetComponent<UserTask.Task>();
        taskComponent.UpdateTask(data, this);
                
        _tasksDisplayed.TryAdd(data, taskComponent);

        _amountCurrentlyDisplayed++;
        
    }

    ///-///////////////////////////////////////////////////////////
    /// Move elements around in taskPool list
    /// 
    void ShuffleTaskList(List<TaskData> list)
    {
        Random rng = new Random();
        
        int n = list.Count;
        
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    ///-///////////////////////////////////////////////////////////
    /// Return a TaskData that is not being displayed currently
    /// 
    private TaskData GetInactiveTask()
    {
        foreach(TaskData data in _displayableTasks)
        {
            if (_activeTaskData[data] == false)
            {
                _activeTaskData[data] = true;

                return data;
            }
        }
        Debug.Log("Could not find a non-active task data!");
        return null;
    }

    #endregion

    #region Completing

    ///-///////////////////////////////////////////////////////////
    /// Add a Task to a list of completed tasks. When all tasks displayed on screen
    /// have been completed, refresh all the tasks. TaskData that were completed are temporarily
    /// removed from the task pool.
    /// 
    public void CompleteTask(UserTask.Task task)
    {
        _completedTasks.Add(task);

        TaskData dataOfCompletedTask = task.GetCurrentTaskData();
        
        // Reset counter
        _taskDataOnHold[dataOfCompletedTask] = dataOfCompletedTask.refreshCountdown;

        // Once the user has completed all tasks that could be displayed on screen...
        if (_completedTasks.Count >= _amountCurrentlyDisplayed)
        {
            Debug.Log($"What was count: {_completedTasks.Count} vs. amount currently displayed? {_amountCurrentlyDisplayed}");
            RefreshAllTasks();
        }
        SaveCompletedTasks();
    }

    ///-///////////////////////////////////////////////////////////
    /// Remove a task from the list of completed tasks
    /// 
    public void UncompleteTask(UserTask.Task task)
    {
        _completedTasks.Remove(task);
        
        SaveCompletedTasks();
    }

    ///-///////////////////////////////////////////////////////////
    /// When all on screen are completed, remove them and place a new set of tasks.
    /// TaskData that were temporarily removed from the pool can start returning.
    /// 
    private void RefreshAllTasks()
    {
        // Previously completed tasks (not this current refresh), can begin to reappear on the user's task list
        foreach (TaskData taskData in _taskDataOnHold.Keys.Reverse())
        {
            // If a TaskData has 0 or less refreshes left to reappear
            if (_taskDataOnHold[taskData] <= 0)
            {
                // TaskData is no longer "active" and can reappear 
                _activeTaskData[taskData] = false;
                // Reset counter
                _taskDataOnHold[taskData] = taskData.refreshCountdown;
            }
            else
            {
                // A refresh has occurred, so this TaskData should be closer to reappearing in pool again
                _taskDataOnHold[taskData]--;
            }
            Debug.Log(taskData.taskName + " has " + _taskDataOnHold[taskData] + " refreshes left!");
        }

        // Remove tasks on screen
        foreach (UserTask.Task task in _completedTasks)
        {
            _tasksDisplayed.Remove(task.GetCurrentTaskData());
            Destroy(task.gameObject);
        }
        
        Debug.Log("All displayed tasks have been completed!");

        // All tasks on screen were deleted, so reset this back to 0
        _amountCurrentlyDisplayed = 0;

        // Lower amount of tasks to display on screen, on each refresh
        _amountAllowedToDisplay--;

        Debug.Log(_amountAllowedToDisplay);
        
        _completedTasks.Clear();

        // Display more tasks after refresh (if we can still display more tasks)
        if(_amountAllowedToDisplay > 0) 
            DisplayAllTasks();
        else
            Debug.Log("All task refreshes have been used up! User must wait 24 hours for a refresh to occur!");
        
        SaveTaskPlacement();
        SaveCompletedTasks();
    }

    #endregion

    #region Saving
    
    ///-///////////////////////////////////////////////////////////
    /// Save the tasks the user has displayed on screen.
    /// 
    private async void SaveTaskPlacement()
    {
        // Convert dictionary to a list of KeyValuePairs
        List<KeyValuePair<TaskData, int>> dataList = _taskDataOnHold.ToList();

        // Convert the list to a serializable data structure (e.g., a list of tuples)
        List<Tuple<TaskData, int>> serializableList = new List<Tuple<TaskData, int>>();
        foreach (var pair in dataList)
        {
            serializableList.Add(new Tuple<TaskData, int>(pair.Key, pair.Value));
        }

        List<string> allDisplayedTasksByName = _tasksDisplayed.Keys.Select(key => key.taskName).ToList();
        
        // Save list of custom tasks (by task name) to the user's account
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "tasksCurrentlyDisplayed", allDisplayedTasksByName} });

        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "tasksCurrentlyOnRefresh", serializableList} });
    }

    ///-///////////////////////////////////////////////////////////
    /// Save all the tasks the user has completed so far.
    /// 
    private async void SaveCompletedTasks()
    {
        if (_amountAllowedToDisplay < 0)
            _amountAllowedToDisplay = 0;
        
        // Save how many tasks the user was able to display (we convert to string, because integers cannot be null)
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "tasksCurrentlyAllowedToDisplay", _amountAllowedToDisplay.ToString()} });
        
        List<string> allCompletedTasksByName =
            _completedTasks.Select(task => task.GetCurrentTaskData().taskName).ToList();
        
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "tasksCurrentlyMarkedOff", allCompletedTasksByName} });
    }
    
    public async void LoadTaskPlacement()
    {
        try
        {
            string loadedAmountToDisplay = await LoadData<string>("tasksCurrentlyAllowedToDisplay");
            
            // Load how many tasks the user was able to display (string to int)
            if (!string.IsNullOrEmpty(loadedAmountToDisplay))
                _amountAllowedToDisplay = int.Parse(loadedAmountToDisplay);
            else
                _amountAllowedToDisplay = maxTaskDisplay;

            List<string> loadedCompletedTasks = await LoadData<List<string>>("tasksCurrentlyMarkedOff");
            
            // Load all previously displayed tasks (by name)
            List<string> loadedDisplayedTasks = await LoadData<List<string>>("tasksCurrentlyDisplayed");
            
            if (loadedDisplayedTasks != null)
            {
                foreach (string taskDataByName in loadedDisplayedTasks)
                {
                    Debug.Log("Found a previously displayed task data: " + taskDataByName);

                    SpawnTask(_allDisplayableTaskDataByName[taskDataByName]);
                }
            }

            if (loadedCompletedTasks != null)
            {
                // Re-complete any tasks
                foreach (string completedTaskData in loadedCompletedTasks)
                {
                    _tasksDisplayed[_allDisplayableTaskDataByName[completedTaskData]].CompleteOnCommand();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
        }
        DisplayAllTasks();
    }

    ///-///////////////////////////////////////////////////////////
    /// Loads and returns data that was saved with provided string. Otherwise, will display that the data couldn't be loaded/found.
    /// 
    private async Task<T> LoadData<T>(string dataName)
    {
        var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>
        {
            dataName
        });

        if (savedData.TryGetValue(dataName, out var dataLoaded))
        {
            return dataLoaded.Value.GetAs<T>();
        }
        else
        {
            throw new Exception($"Data associated with '{dataName}' not found or retrieval failed.");
        }
    }

    public async void ClearTaskPlacement()
    {
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "tasksCurrentlyAllowedToDisplay", ""} });
        
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "tasksCurrentlyDisplayed", ""} });
        
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "tasksCurrentlyMarkedOff", ""} });
    }

    #endregion
}
