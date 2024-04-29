using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    
    [Header("Required Scripts")]
    private CategoryManager _taskPool;
    private TaskPlacer _taskPlacer;
    private CustomTaskCreator _customTaskCreator;
    private TaskRefreshWithTime _taskRefreshWithTime;

    [SerializeField] private GameObject categoriesScreen;
    
    private async void Awake()
    {
        Instance = this;
        
        // Fetch references of required scripts
        _taskPool = GetComponent<CategoryManager>();
        _taskPlacer = GetComponent<TaskPlacer>();
        _customTaskCreator = GetComponent<CustomTaskCreator>();
        _taskRefreshWithTime = GetComponent<TaskRefreshWithTime>();
        
        // When the user is signed in, begin the task placement process
        await UnityServices.InitializeAsync();
        //AuthenticationService.Instance.SignedIn += BeginTaskPlacementProcess;
        BeginTaskPlacementProcess();
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
        _taskRefreshWithTime.refreshTimerOccurred -= NotifyTaskPlacerOfRefresh;
    }

    private void OnDestroy()
    {
        AuthenticationService.Instance.SignedIn -= BeginTaskPlacementProcess;
    }

    ///-///////////////////////////////////////////////////////////
    /// Start the task placement process by loading the user's saved task category preferences and custom tasks
    /// 
    private async void BeginTaskPlacementProcess()
    {
        Task[] methodsToWait = {
            // Load task categories from the user's account first
            _taskPool.LoadCategoriesFromCloud(),
            
            // Load all custom tasks the user created in the past
            _customTaskCreator.LoadAllCustomTasks(),
        };

        // Wait for task categories and custom tasks to be loaded in before placement
        await Task.WhenAll(methodsToWait);

        // If no categories have been chosen yet, display the choose category screen
        if (_taskPool.ChosenTaskCategories == null || _taskPool.ChosenTaskCategories.Count <= 0)
        {
            categoriesScreen.SetActive(true);
        }

        // All multiple custom tasks to the TaskPlacer and send it the category filter
        foreach (TaskData data in _customTaskCreator.LoadedCustomTasks)
        {
            _taskPlacer.TryAddTask(data, _taskPool.ChosenTaskCategories);
        }

        // Try to add all tasks (won't do anything if the user hasn't saved any task categories ever before)
        UpdatePremadeTasks(_taskPool.ChosenTaskCategories);
        
        // Loaded previously displayed tasks
        _taskPlacer.LoadTaskPlacement();

        _taskRefreshWithTime.refreshTimerOccurred += NotifyTaskPlacerOfRefresh;

        _taskRefreshWithTime.CheckElapsedTimeOnLogin();

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
    private void UpdateExistingTaskOnScreen(string oldCustomTaskName, TaskData customTaskUpdated)
    {
        _taskPlacer.ExistingTaskDataWasUpdated(oldCustomTaskName, customTaskUpdated, _taskPool.ChosenTaskCategories);
    }

    ///-///////////////////////////////////////////////////////////
    /// When the user has deleted an existing task, tell the TaskPlacer to remove it from the screen
    /// and replace it with an inactive task.
    /// 
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
        _taskPlacer.EditPlacementOnCategoryChange(chosenTaskCategories);
    }

    ///-///////////////////////////////////////////////////////////
    /// When refresh timer has occurred, tell TaskPlacer to replace all tasks on screen
    /// with new ones.
    /// 
    private void NotifyTaskPlacerOfRefresh()
    {
        _taskPlacer.RefreshAllTasksWithTime();
    }
    
    
}
