using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;
using UserTask;

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

    private Dictionary<TaskData, Task> _tasksDisplayed = new Dictionary<TaskData, Task>();

    // A hash set of all of the names of the task datas in the current pool
    private HashSet<string> _allDisplayableTaskDataByName = new HashSet<string>();

    /* Task Data that have been previously completed
     * Key: The TaskData (ex. Brushing Teeth)
     * Value: Number of refreshes left until it reappears
     */
    private Dictionary<TaskData, int> _taskDataOnHold = new Dictionary<TaskData, int>();

    private List<Task> _completedTasks = new List<Task>();

    // Task that will be instantiated and placed on the canvas
    [SerializeField] private GameObject taskPrefab;
    [SerializeField] private Transform taskPanel;
    
    private void Awake()
    {
        _amountAllowedToDisplay = maxTaskDisplay;
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

        // If we found any tasks, start displaying them
        if (_displayableTasks.Count > 0)
        {
            // Once all TaskData's have been added to the dictionary, start displaying them
            DisplayAllTasks();
        }
        else
        {
            Debug.Log($"There were no tasks found under Resources/{_taskDataFolderPath}. Or no task categories were loaded in.");
        }
    }
    
    public void TryAddTask(TaskData data, List<TaskCategory> userChosenCategories)
    {
        // Filter out TaskData that the user doesn't prefer to see
        if (!userChosenCategories.Contains(data.category)) return;
        
        // Don't add any duplicate task data
        if (_activeTaskData.TryAdd(data, false) == false || CheckTaskNameUniqueness(data))
        {
            Debug.Log($"{data.taskName} is a duplicate in the taskData list!");
        }
        else
        {
            _displayableTasks.Add(data);
            _allDisplayableTaskDataByName.Add(data.taskName);

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
            Debug.Log($"Task Manager has loaded in: {data.taskName}");
        }
    }
    
    ///-///////////////////////////////////////////////////////////
    /// Check if this task is already in the pool. We check by name since the scriptableObjects don't always exist.
    /// 
    private bool CheckTaskNameUniqueness(TaskData taskData)
    {
        return _allDisplayableTaskDataByName.Contains(taskData.taskName);
    }

    #endregion
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
    public void ExistingTaskDataWasUpdated(TaskData taskDataEdited, List<TaskCategory> userChoseCategories)
    {
        if (_activeTaskData.ContainsKey(taskDataEdited))
        {
            // Update Task button's information displayed
            _tasksDisplayed[taskDataEdited].UpdateTask(taskDataEdited, this);
            
            // If the task was in the pool, but its category was changed to a category that the user doesn't use. Remove it from placement.
            if (!userChoseCategories.Contains(taskDataEdited.category))
            {
                RemoveTaskFromDisplay(taskDataEdited);
            }
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Remove a TaskData from any future displaying on the screen.
    /// 
    private void RemoveTaskFromDisplay(TaskData dataToRemove)
    {
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

            if(inactiveData != null)
            {
                // Spawn a new task and give it data
                GameObject newTask = Instantiate(taskPrefab, taskPanel);
                Task taskComponent = newTask.GetComponent<Task>();
                taskComponent.UpdateTask(inactiveData, this);
                
                _tasksDisplayed.Add(inactiveData, taskComponent);

                _amountCurrentlyDisplayed++;
            }
        }
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
    public void CompleteTask(Task task)
    {
        _completedTasks.Add(task);

        TaskData dataOfCompletedTask = task.GetCurrentTaskData();
        
        // Reset counter
        _taskDataOnHold[dataOfCompletedTask] = dataOfCompletedTask.refreshCountdown;

        // Once the user has completed all tasks that could be displayed on screen...
        if (_completedTasks.Count >= _amountCurrentlyDisplayed)
        {
            RefreshAllTasks();
        }  
    }

    ///-///////////////////////////////////////////////////////////
    /// Remove a task from the list of completed tasks
    /// 
    public void UncompleteTask(Task task)
    {
        _completedTasks.Remove(task);
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
        foreach (Task task in _completedTasks)
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
        
        // Randomize all elements in the task pool
        ShuffleTaskList(_displayableTasks);
        
        DisplayAllTasks();
    }

    #endregion
}
