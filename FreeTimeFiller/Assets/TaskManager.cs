using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;

    // Tasks that will show up in the pool
    [SerializeField] private List<TaskData> taskDatas = new List<TaskData>();

    // How many tasks should be created and displayed?
    [SerializeField, Range(1, 8)] private int taskCount;

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
        instance = this;
    }

    private void Start()
    {
        InitializeActiveTasks();
    }
    private void InitializeActiveTasks()
    {
        // If we have some tasks to spawn in...
        if (taskDatas.Count > 0)
        {
            foreach (TaskData taskData in taskDatas)
            {
                if (activeTaskDatas.TryAdd(taskData, false) == false)
                    Debug.Log($"{taskData} is a duplicate in the taskDatas list!");
            }

            DisplayAllTasks();
        }
        else
        {
            Debug.Log("Please enter atleast one task data in the list!");
        }
    }

    private void DisplayAllTasks()
    {
        for(int i = 0; i < taskCount; i++)
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
}
