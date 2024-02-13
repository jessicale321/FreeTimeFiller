using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;
using UserTask;
using UnityEditor.U2D.Aseprite;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    // Tasks that will show up in the pool
    private List<TaskData> _taskPool = new List<TaskData>();

    // Folder path for location of Task Data
    private string _taskDataFolderPath = "Task Data";

    /* How many tasks should be created and displayed?
     * If taskCount is greater than the number of inactive TaskData's, 
     * then we will get an error in the console
     */
    [SerializeField, Range(1, 8)] private int taskAmountToDisplay = 1;

    private int _taskAmountCurrentlyDisplayed = 0;

    /* Tasks that are currently being displayed
     * Key: The TaskData (ex. Brushing Teeth)
     * Value: Is currently displayed? True or false
     */
    private Dictionary<TaskData, bool> _activeTaskData = new Dictionary<TaskData, bool>();
    
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
        // Create singleton instance for TaskManager
        Instance = this;
    }

    private void Start()
    {
        FindPremadeTasks();
    }

    ///-///////////////////////////////////////////////////////////
    /// Load all pre-made tasks found under the resources folder
    /// 
    private void FindPremadeTasks()
    {
        TaskData[] tasks =  Resources.LoadAll<TaskData>(_taskDataFolderPath);

        foreach (TaskData data in tasks)
        {
            TryAddTask(data);
        }

        // If we found any tasks, start displaying them
        if (_taskPool.Count > 0)
        {
            // Randomize all elements in the task pool
            ShuffleTaskList(_taskPool);
            
            // Once all TaskData's have been added to the dictionary, start displaying them
            DisplayAllTasks();
        }
        else
        {
            Debug.Log($"There were no tasks found under Resources/{_taskDataFolderPath}");
        }
    }
    
    public void AddNewTaskToPool(TaskData newTaskData)
    {
        TryAddTask(newTaskData);
        
        // Randomize all elements in the task pool
        ShuffleTaskList(_taskPool);
    }
    
    private void TryAddTask(TaskData data)
    {
        // Don't add any duplicate task data
        if (_activeTaskData.TryAdd(data, false) == false)
        {
            Debug.Log($"{data} is a duplicate in the taskData list!");
        }
        else
        {
            _taskPool.Add(data);
            Debug.Log($"Task Manager has loaded in: {data.taskName}");
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Find a TaskData that is not being displayed currently, and create
    /// a new task using its values
    /// 
    private void DisplayAllTasks()
    {
        for(int i = 0; i < taskAmountToDisplay; i++)
        {
            TaskData inactiveData = GetInactiveTask();

            if(inactiveData != null)
            {
                // Spawn a new task and give it data
                GameObject newTask = Instantiate(taskPrefab, taskPanel);
                newTask.GetComponent<Task>().UpdateTask(inactiveData);

                _taskAmountCurrentlyDisplayed++;
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
        foreach(TaskData data in _taskPool)
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
    

    ///-///////////////////////////////////////////////////////////
    /// Add a Task to a list of completed tasks. When all tasks displayed on screen
    /// have been completed, refresh all the tasks. TaskData that were completed are temporarily
    /// removed from the task pool.
    /// 
    public void CompleteTask(Task task)
    {
        _completedTasks.Add(task);

        TaskData dataOfCompletedTask = task.GetCurrentTaskData();
        
        if (!_taskDataOnHold.ContainsKey(dataOfCompletedTask))
        {
            //Debug.Log("add");
            _taskDataOnHold.Add(dataOfCompletedTask, dataOfCompletedTask.refreshCountdown);
        }
        else
        {
            // Reset counter
            _taskDataOnHold[dataOfCompletedTask] = dataOfCompletedTask.refreshCountdown;
        }

        // Once the user has completed all tasks that could be displayed on screen...
        if (_completedTasks.Count >= _taskAmountCurrentlyDisplayed)
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

                //Debug.Log($"{taskData} is closer to reappearing!");
            }
            Debug.Log(taskData.taskName + " has " + _taskDataOnHold[taskData] + " refreshes left!");
        }

        // Remove tasks on screen
        foreach (Task task in _completedTasks)
        {

            Destroy(task.gameObject);
        }
        
        Debug.Log("All displayed tasks have been completed!");

        // All tasks on screen were deleted, so reset this back to 0
        _taskAmountCurrentlyDisplayed = 0;
        
        _completedTasks.Clear();
        
        // Randomize all elements in the task pool
        ShuffleTaskList(_taskPool);
        
        DisplayAllTasks();
    }
}
