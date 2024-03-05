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
    private CategoryManager _taskPool;
    private TaskPlacer _taskPlacer;
    private CustomTaskCreator _customTaskCreator;
    
    private async void Awake()
    {
        Instance = this;
        
        // Fetch references of required scripts
        _taskPool = GetComponent<CategoryManager>();
        _taskPlacer = GetComponent<TaskPlacer>();
        _customTaskCreator = GetComponent<CustomTaskCreator>();
        
        // When the user is signed in, begin the task placement process
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += BeginTaskPlacementProcess;
        
        // TODO: REMOVE THIS SOON
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        //_taskPlacer.ClearTaskPlacement();
    }

    private void OnEnable()
    {
        // When the custom task creator has finished loading or has created a task, add a custom task
        _customTaskCreator.CustomTaskWasCreatedWithoutLoad += AddOneCustomTask;
        _customTaskCreator.ExistingCustomTaskWasEdited += UpdateExistingTaskOnScreen;
        _customTaskCreator.CustomTaskWasDeleted += DeleteExistingTaskOnScreen;
    }

    private void OnDisable()
    {
        _customTaskCreator.CustomTaskWasCreatedWithoutLoad -= AddOneCustomTask;
        _taskPool.TaskCategoriesChanged -= UpdatePremadeTasks;
    }

    ///-///////////////////////////////////////////////////////////
    /// Start the task placement process by loading the user's saved task category preferences and custom tasks
    /// 
    private async void BeginTaskPlacementProcess()
    {
        Task[] methodsToWait = {
            // Load task categories from the user's account first
            _taskPool.LoadCategoriesFromCloud(),
            
            // Loaded previously displayed tasks
            _taskPlacer.LoadTaskPlacement(_taskPool.ChosenTaskCategories),

            // Load all custom tasks the user created in the past
            _customTaskCreator.LoadAllCustomTasks()
        };

        // Wait for task categories and custom tasks to be loaded in before placement
        await Task.WhenAll(methodsToWait);

        // All multiple custom tasks to the TaskPlacer and send it the category filter
        foreach (TaskData data in _customTaskCreator.LoadedCustomTasks)
        {
            _taskPlacer.TryAddTask(data, _taskPool.ChosenTaskCategories);
        }

        // Try to add all tasks (won't do anything if the user hasn't saved any task categories ever before)
        UpdatePremadeTasks(_taskPool.ChosenTaskCategories);

        // If task categories are changed again, add the tasks again
        _taskPool.TaskCategoriesChanged += UpdatePremadeTasks;
        
        Debug.Log(("Finish loading custom tasks and categories"));
    }
    
    ///-///////////////////////////////////////////////////////////
    /// When one custom task was created (not loaded in from cloud), add it on the screen.
    /// 
    private void AddOneCustomTask(TaskData customTask)
    {
        _taskPlacer.TryAddTask(customTask, _taskPool.ChosenTaskCategories);
    }

    ///-///////////////////////////////////////////////////////////
    /// When the contents of a custom task has been updated, tell the TaskPlacer that 
    /// one of its tasks (if its displayable) will need to change its text on screen and
    /// may get removed depending on the category.
    /// 
    private void UpdateExistingTaskOnScreen(TaskData customTaskUpdated)
    {
        _taskPlacer.ExistingTaskDataWasUpdated(customTaskUpdated, _taskPool.ChosenTaskCategories);
    }

    private void DeleteExistingTaskOnScreen(TaskData taskDeleted)
    {
        _taskPlacer.RemoveTaskFromDisplay(taskDeleted);
    }

    ///-///////////////////////////////////////////////////////////
    /// After all task categories and custom tasks have finished loading,
    /// tell the TaskPlacer to place all pre-made tasks on the screen.
    /// 
    private void UpdatePremadeTasks(List<TaskCategory> chosenTaskCategories)
    {      
        // Add pre-made tasks to TaskPlacer after all custom tasks have been added
        _taskPlacer.FindPremadeTasks(chosenTaskCategories);

        // When task categories have changed, 
        _taskPlacer.RemoveTaskDataByCategories(chosenTaskCategories);
    }
    
}
