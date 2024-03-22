using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.CloudSave;
using UnityEngine;
using Random = System.Random;

public class TaskPlacer : MonoBehaviour
{
    // UI text that tells user they finished all the tasks they can do
    [SerializeField] private TMP_Text completionMessage;
    
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

    private void Awake()
    {
        completionMessage.gameObject.SetActive(false);
    }

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

    ///-///////////////////////////////////////////////////////////
    /// Attempt to allow a new task to become displayable. Return true if it's allowed to become displayable,
    /// otherwise return false.
    /// 
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

    #region Editing & Removing
    ///-///////////////////////////////////////////////////////////
    /// When TaskCategory preferences have changed, check if any displayable tasks need to be removed. The user 
    /// may have decided they don't want to see certain tasks anymore.
    /// 
    public void EditPlacementOnCategoryChange(List<TaskCategory> userChosenCategories)
    {
        foreach (TaskCategory category in _allDisplayableTaskDataByCategory.Keys)
        {
            if (!userChosenCategories.Contains(category))
            {
                Debug.Log($"{category} was removed! Remove all of its tasks from display!");

                // Create a copy of the collection to avoid modifying it while iterating
                List<TaskData> taskDataToRemove = new List<TaskData>(_allDisplayableTaskDataByCategory[category]);

                // Remove each task from display
                foreach (TaskData dataFromRemovedCategory in taskDataToRemove)
                {
                    RemoveTaskFromDisplay(dataFromRemovedCategory);

                    Debug.Log($"Removed by category: {dataFromRemovedCategory.taskName}");
                }
                _allDisplayableTaskDataByCategory[category].Clear();
            }
        }
        // Display more tasks if needed
        if (_amountCurrentlyDisplayed < _amountAllowedToDisplay)
        {
            DisplayAllTasks();
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// When a task has been edited, check to see if its category is no longer apart of the user's chosen
    /// task categories. If it's not, then remove it from all placements.
    public void ExistingTaskDataWasUpdated(string oldTaskName, TaskData taskDataEdited, List<TaskCategory> userChosenCategories)
    {
        // If this edited TaskData was displayable...
        if (_activeTaskData.ContainsKey(taskDataEdited))
        {
            // Delete the old task's name and replace it (if the name was changed)
            if (oldTaskName != taskDataEdited.taskName)
            {
                _allDisplayableTaskDataByName.Remove(oldTaskName);
                _allDisplayableTaskDataByName.Add(taskDataEdited.taskName, taskDataEdited);
            }

            // Update Task button's information displayed (if it's currently displayed)
            if(_tasksDisplayed.ContainsKey(taskDataEdited))
            {
                _tasksDisplayed[taskDataEdited].UpdateTask(taskDataEdited, this);
            }
            
            // If the task was in the pool, but its category was changed to a category that the user doesn't use. Remove it from placement.
            if (!userChosenCategories.Contains(taskDataEdited.category))
            {
                RemoveTaskFromDisplay(taskDataEdited);
            }
            
            SaveTaskPlacement();
            SaveCompletedTasks();
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

        if (_tasksDisplayed.ContainsKey(dataToRemove))
        {
            Destroy((_tasksDisplayed[dataToRemove].gameObject));
            _tasksDisplayed.Remove(dataToRemove);
        }
        
        // Remove task from all containers
        _displayableTasks.Remove(dataToRemove);
        _allDisplayableTaskDataByName.Remove(dataToRemove.taskName);
        _activeTaskData.Remove(dataToRemove);
        _taskDataOnHold.Remove(dataToRemove);
        
        _amountCurrentlyDisplayed--;
        
        // Replace the deleted task with a different one
        ReplaceDeletedTask();
        
        // Task name could have been edited, therefore we must save again
        SaveTaskPlacement();
        SaveTasksOnHold();
        SaveCompletedTasks();
    }

    ///-///////////////////////////////////////////////////////////
    /// After deleting a task, add a new inactive one.
    /// 
    private void ReplaceDeletedTask()
    {
        // TODO: Check if the replaced task was completed, if so then un-complete it!
        TaskData newTaskData = GetInactiveTask();

        SpawnTask(newTaskData);
    }
    #endregion

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
            TaskData inactiveData = GetInactiveTask();
            
            SpawnTask(inactiveData);
        }
        SaveTaskPlacement();
    }

    ///-///////////////////////////////////////////////////////////
    /// Place a new task gameObject on the screen, if we have room to do so.
    /// 
    private void SpawnTask(TaskData data)
    {
        if (data == null) return;
        
        // Don't allow more tasks to be displayed, if we reached the current max amount
        if (_amountCurrentlyDisplayed >= _amountAllowedToDisplay) return;
        
        _activeTaskData[data] = true;
        
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
                if (_taskDataOnHold.TryGetValue(data, out int refreshesLeftUntilReappear))
                {
                    // If this task was previously active and is no longer on hold, it's inactive
                    if (refreshesLeftUntilReappear <= 0)
                        return data;
                }
                // If the task wasn't in the taskDataOnHold dictionary, then it's inactive
                else
                {
                    return data;
                }
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
        
        // Once the user has completed all tasks that could be displayed on screen...
        if (_completedTasks.Count >= _amountCurrentlyDisplayed)
        {
            Debug.Log($"What was count: {_completedTasks.Count} vs. amount currently displayed? {_amountCurrentlyDisplayed}");
            RefreshAllTasksOnFullCompletion();
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
    /// Clear all tasks gameObjects on the screen.
    /// 
    private void RemoveAllTasksFromScreen()
    {
        // Create a copy of the keys to avoid modifying the dictionary while iterating
        List<TaskData> tasksToRemove = new List<TaskData>(_tasksDisplayed.Keys);

        // Remove tasks on screen
        foreach (TaskData taskData in tasksToRemove)
        {
            // TaskData is no longer "active" and can reappear 
            _activeTaskData[taskData] = false;
            
            if (_tasksDisplayed.TryGetValue(taskData, out UserTask.Task task))
            {
                Destroy(task.gameObject);
                _tasksDisplayed.Remove(taskData);
            }
        }

        // Reset the count of displayed tasks
        _amountCurrentlyDisplayed = 0;
    }
    
    ///-///////////////////////////////////////////////////////////
    /// When all on screen are completed, remove them and place a new set of tasks.
    /// TaskData that were temporarily removed from the pool can start returning.
    /// 
    private void RefreshAllTasksOnFullCompletion()
    {
        Debug.Log("Refresh everything on full completion!");
        
        // Previously completed tasks (not this current set), can begin to reappear on the user's task list
        foreach (TaskData taskData in _taskDataOnHold.Keys.Reverse())
        {
            // A refresh has occurred, so this TaskData should be closer to reappearing in pool again
            _taskDataOnHold[taskData]--;

            // Don't let value go below 0
            if (_taskDataOnHold[taskData] < 0)
                _taskDataOnHold[taskData] = 0;
            
            Debug.Log(taskData.taskName + " has " + _taskDataOnHold[taskData] + " refreshes left!");
        }

        // If the tasks weren't on hold before, put them on hold.
        foreach (UserTask.Task task in _completedTasks)
        {
            TaskData dataOfCompletedTask = task.GetCurrentTaskData();

            _taskDataOnHold.TryAdd(dataOfCompletedTask, dataOfCompletedTask.refreshCountdown);
        }

        RemoveAllTasksFromScreen();

        // Lower amount of tasks to display on screen, on each refresh
        _amountAllowedToDisplay--;

        Debug.Log(_amountAllowedToDisplay);
        
        _completedTasks.Clear();

        // Display more tasks after refresh (if we can still display more tasks)
        if(_amountAllowedToDisplay > 0) 
            DisplayAllTasks();
        else
            OnTaskFullCompletion();
        
        SaveCompletedTasks();
        SaveTasksOnHold();
    }

    ///-///////////////////////////////////////////////////////////
    /// When refresh timer occurs, remove all tasks from the screen and place a new set.
    /// 
    public void RefreshAllTasksWithTime()
    {
        // No more tasks are considered on refresh
        _taskDataOnHold.Clear();
        
        // No more tasks are considered complete
        _completedTasks.Clear();

        RemoveAllTasksFromScreen();

        // Max number of tasks can be completed again
        _amountAllowedToDisplay = maxTaskDisplay;

        // Show new set of tasks
        DisplayAllTasks();
        
        SaveCompletedTasks();
        SaveTasksOnHold();
        
        // Remove completion message from screen
        if(completionMessage != null)
            completionMessage.gameObject.SetActive(false);
    }

    ///-///////////////////////////////////////////////////////////
    /// Display a message to the user when they have completed the max number of tasks.
    /// 
    private void OnTaskFullCompletion()
    {
        Debug.Log("All task refreshes have been used up! User must wait 24 hours for a refresh to occur!");

        if(completionMessage != null)
            completionMessage.gameObject.SetActive(true);
    }

    #endregion

    #region Saving & Loading
    ///-///////////////////////////////////////////////////////////
    /// Save the tasks the user has displayed on screen.
    /// 
    private async void SaveTaskPlacement()
    {
        List<string> allDisplayedTasksByName = _tasksDisplayed.Keys.Select(key => key.taskName).ToList();

        await DataManager.SaveData("tasksCurrentlyDisplayed", allDisplayedTasksByName);
        
    }

    ///-///////////////////////////////////////////////////////////
    /// Save all the tasks the user has completed so far.
    /// 
    private async void SaveCompletedTasks()
    {
        if (_amountAllowedToDisplay < 0)
            _amountAllowedToDisplay = 0;
        
        List<string> allCompletedTasksByName =
            _completedTasks.Select(task => task.GetCurrentTaskData().taskName).ToList();

        await DataManager.SaveData("tasksCurrentlyAllowedToDisplay", _amountAllowedToDisplay.ToString());
        await DataManager.SaveData("tasksCurrentlyMarkedOff", allCompletedTasksByName);
    }

    private async void SaveTasksOnHold()
    {
        Debug.Log("Save tasks on hold to reappear!");
        // Convert dictionary to a list of KeyValuePairs
        List<KeyValuePair<TaskData, int>> dataOnHoldList = _taskDataOnHold.ToList();

        // Convert the list to a serializable data structure (e.g., a list of tuples)
        List<Tuple<string, int>> taskDataOnHoldListOfTuples = new List<Tuple<string, int>>();
        foreach (var pair in dataOnHoldList)
        {
            taskDataOnHoldListOfTuples.Add(new Tuple<string, int>(pair.Key.taskName, pair.Value));
            Debug.Log($"Saving {pair.Key.taskName} with {pair.Value} refreshes left to reappear.");
        }
        
        await DataManager.SaveData("tasksCurrentlyOnRefresh", taskDataOnHoldListOfTuples);
    }

    ///-///////////////////////////////////////////////////////////
    /// Begin displaying all tasks that the user had on the screen in a previous session.
    /// 
    public async void LoadTaskPlacement()
    {
        try
        {
            // Load how many tasks the user was allowed to display in the previous session
            string loadedAmountToDisplay = await DataManager.LoadData<string>("tasksCurrentlyAllowedToDisplay");
            
            // Check that the value isn't null, then set the amountAllowedToDisplay to what was loaded
            if (!string.IsNullOrEmpty(loadedAmountToDisplay))
                _amountAllowedToDisplay = int.Parse(loadedAmountToDisplay);
            else
                _amountAllowedToDisplay = maxTaskDisplay;

            // Load all previously completed tasks (by name)
            List<string> loadedCompletedTasks = await DataManager.LoadData<List<string>>("tasksCurrentlyMarkedOff");

            // Load all previously displayed tasks (by name)
            List<string> loadedDisplayedTasks = await DataManager.LoadData<List<string>>("tasksCurrentlyDisplayed");

            List<Tuple<string, int>> taskDataOnHoldListOfTuples =
                await DataManager.LoadData<List<Tuple<string, int>>>("tasksCurrentlyOnRefresh");

            if (loadedDisplayedTasks != null)
            {
                // For each loaded displayed task, place them on the screen
                foreach (string taskDataByName in loadedDisplayedTasks)
                {
                    Debug.Log("Found a previously displayed task data: " + taskDataByName);

                    TaskData taskData = _allDisplayableTaskDataByName[taskDataByName];
                    
                    SpawnTask(taskData);
                }
            }

            if (loadedCompletedTasks != null)
            {
                // For each loaded completed task, re-complete them
                foreach (string nameOfCompletedTask in loadedCompletedTasks)
                {
                    _tasksDisplayed[_allDisplayableTaskDataByName[nameOfCompletedTask]].CompleteOnCommand();
                }
            }

            if (taskDataOnHoldListOfTuples != null)
            {
                // For each task that was on refresh, add them to the taskDataOnHold dictionary so we remember 
                // how many refreshes it had left for it to reappear on the screen.
                foreach (var (taskName, refreshesLeftUntilReappear) in taskDataOnHoldListOfTuples)
                {
                    TaskData dataOfTaskOnHold = _allDisplayableTaskDataByName[taskName];
                    _taskDataOnHold[dataOfTaskOnHold] = refreshesLeftUntilReappear;
                    
                    Debug.Log($"{taskName} was previously on refresh and will reappear after {_taskDataOnHold[dataOfTaskOnHold]} refreshes.");
                }
            }
            
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
        }
        
        // Display all tasks (this is only works when there were no tasks found from a previous session)
        DisplayAllTasks();
        
        // Display completion message if all tasks were completed
        if (completionMessage != null)
        {
            // If there are tasks displayed, don't show the completion message
            if(_tasksDisplayed.Count > 0)
                completionMessage.gameObject.SetActive(false);
            else
                completionMessage.gameObject.SetActive(true);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Delete all data associated with previous task placement.
    /// 
    public async void ClearTaskPlacement()
    {
        await DataManager.DeleteAllDataByName("tasksCurrentlyAllowedToDisplay");
        await DataManager.DeleteAllDataByName("tasksCurrentlyDisplayed");
        await DataManager.DeleteAllDataByName("tasksCurrentlyMarkedOff");
        await DataManager.DeleteAllDataByName("tasksCurrentlyOnRefresh");
    }

    #endregion
}
