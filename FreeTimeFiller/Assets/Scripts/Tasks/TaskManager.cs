using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    private TaskPool _taskPool;
    private TaskPlacer _taskPlacer;
    private CustomTaskCreator _customTaskCreator;
    
    private async void Awake()
    {
        _taskPool = GetComponent<TaskPool>();
        _taskPlacer = GetComponent<TaskPlacer>();
        _customTaskCreator = GetComponent<CustomTaskCreator>();
        
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += BeginProcess;
        
        // TODO: REMOVE THIS SOON
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void BeginProcess()
    {
        _taskPool.LoadCategoriesFromCloud();
        
        // Create filter, then create custom tasks, when all custom tasks are done, give them to taskplacer, which will filter them out from there
        _taskPool.TaskCategoriesChanged += LoadCustomTasks;
        _customTaskCreator.CustomTaskWasCreatedWithoutLoad += AddOneCustomTask;
        _customTaskCreator.AllCustomTaskWereLoaded += AddMultipleCustomTasks;
    }
    
    private void LoadCustomTasks(List<TaskCategory> obj)
    {
        _customTaskCreator.LoadAllCustomTasks();
    }
    
    private void AddOneCustomTask(TaskData obj)
    {
        _taskPlacer.AddNewTaskToPool(obj, _taskPool._chosenTaskCategories);
    }
    
    private void AddMultipleCustomTasks(List<TaskData> obj)
    {
        foreach (TaskData data in obj)
        {
            _taskPlacer.AddNewTaskToPool(data, _taskPool._chosenTaskCategories);
        }
        
        _taskPlacer.FindPremadeTasks(_taskPool._chosenTaskCategories);
    }
}
