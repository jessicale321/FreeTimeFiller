using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    // Tasks that will show up in the pool
    [SerializeField] private List<TaskData> taskPool = new List<TaskData>();

    /* How many tasks should be created and displayed?
     * If taskCount is greater than the number of inactive TaskData's, 
     * then we will get an error in the console
     */
    [SerializeField, Range(1, 8)] private int taskAmount;

    /* Tasks that are currently being displayed
     * Key: The TaskData (ex. Brushing Teeth)
     * Value: Is currently displayed? True or false
     */
    private Dictionary<TaskData, bool> activeTaskDatas = new Dictionary<TaskData, bool>();

    // Task that will be instaniated and placed on the canvas
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

    /* Load all pre-made tasks found under the resources folder */
    private void FindPremadeTasks()
    {
        TaskData[] tasks =  Resources.LoadAll<TaskData>("TaskDatas/Premade");

        foreach (TaskData data in tasks)
        {
            // Don't add any duplicate task datas
            if (activeTaskDatas.TryAdd(data, false) == false)
            {
                Debug.Log($"{data} is a duplicate in the taskDatas list!");
            }
            else
            {
                taskPool.Add(data);
            }

        }

        // If we found any tasks, start displaying them
        if (taskPool.Count > 0)
        {
            // Once all TaskData's have been added to the dictionary, start displaying them
            DisplayAllTasks();
        }
        else
        {
            Debug.Log("There were no tasks found under Resources/TaskDatas/Premade");
        }
        
    }

    /* Find a TaskData that is not being displayed currently, and create
     a new task using its values */
    private void DisplayAllTasks()
    {
        for(int i = 0; i < taskAmount; i++)
        {
            TaskData inactiveData = GetInactiveTask();

            if(inactiveData != null)
            {
                // Spawn a new task and give it data
                GameObject newTask = Instantiate(taskPrefab, taskPanel);
                newTask.GetComponent<Task>().UpdateTask(inactiveData);
            }
           
        }
    }

    // Return a TaskData that is not being displayed currently
    private TaskData GetInactiveTask()
    {
        foreach(TaskData data in activeTaskDatas.Keys)
        {
            if (activeTaskDatas[data] == false)
            {
                activeTaskDatas[data] = true;

                return data;
            }
        }

        Debug.Log("Could not find a non-active task data!");
        return null;
    }
    
    /* TODO: Need to figure out how we can refresh all displayed tasks after completing a set. 
     * Also should not place back tasks that were previously completed */
    private void RefreshAllTasksOnCompletion()
    {
        
    }
    
    /* TODO: Need to figure out how we can refresh all displayed tasks after a certain 
     amount of time. Also should not place back tasks that were previously completed */
    private void RefreshAllTasksAfterTime()
    {
        
    }
}
