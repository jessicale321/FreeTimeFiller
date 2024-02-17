using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    
    [Header("Required Scripts")]
    private TaskPool _taskPool;
    private TaskPlacer _taskPlacer;
    private CustomTaskCreator _customTaskCreator;
    
    private async void Awake()
    {
        Instance = this;
        
        // Fetch references of required scripts
        _taskPool = GetComponent<TaskPool>();
        _taskPlacer = GetComponent<TaskPlacer>();
        _customTaskCreator = GetComponent<CustomTaskCreator>();
        
        // When the user is signed in, begin the task placement process
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += BeginTaskPlacementProcess;
        
        // TODO: REMOVE THIS SOON
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void OnEnable()
    {
        // When the custom task creator has finished loading or has created a task, add a custom task
        _customTaskCreator.CustomTaskWasCreatedWithoutLoad += AddOneCustomTask;
    }

    private void OnDisable()
    {
        _taskPool.TaskCategoriesChanged -= AddAllTasks;
    }

    ///-///////////////////////////////////////////////////////////
    /// Start the task placement process by loading the user's saved task category preferences and custom tasks
    /// 
    private async void BeginTaskPlacementProcess()
    {
        Task[] methodsToWait = {
            // Load task categories from the user's account first
            _taskPool.LoadCategoriesFromCloud(),

            _customTaskCreator.LoadAllCustomTasks()
        };

        // Wait for task categories and custom tasks to be loaded in before placement
        await Task.WhenAll(methodsToWait);
        
        // Try to add all tasks (won't do anything if the user hasn't saved any task categories ever before)
        AddAllTasks(_taskPool.ChosenTaskCategories);

        // If task categories are changed again, add the tasks again
        _taskPool.TaskCategoriesChanged += AddAllTasks;
        
        Debug.Log(("Finish loading custom tasks and categories"));
    }
    
    ///-///////////////////////////////////////////////////////////
    /// When one custom task was created (not loaded in from cloud), add it on the screen.
    /// 
    private void AddOneCustomTask(TaskData customTask)
    {
        _taskPlacer.AddNewTaskToScreen(customTask, _taskPool.ChosenTaskCategories);
    }

    private void AddAllTasks(List<TaskCategory> chosenTaskCategories)
    {
        // All multiple custom tasks to the TaskPlacer and send it the category filter
        foreach (TaskData data in _customTaskCreator.LoadedCustomTasks)
        {
            _taskPlacer.AddNewTaskToScreen(data, chosenTaskCategories);
        }
        
        // Add pre-made tasks to TaskPlacer after all custom tasks have been added
        _taskPlacer.FindPremadeTasks(chosenTaskCategories);
    }

}
