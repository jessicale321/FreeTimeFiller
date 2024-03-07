using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class TaskRefreshWithTime: MonoBehaviour
{
    private DateTime _lastTimeAppWasOpened;

    private DateTime _lastTimeRefreshedWhileOpen;

    public Action RefreshTimerOccurred;

    private void Awake()
    {
        _lastTimeRefreshedWhileOpen = DateTime.Now;
    }

    //private void OnApplicationFocus(bool focus)
    //{
    //    // Save login time when the app regains focus
    //    if(focus)
    //        SaveLoginTime();
    //}

    private void OnApplicationQuit()
    {
        // Save login time when closing the app
        SaveLoginTime();
    }

    private void Update()
    {
        // Check every frame if the time should refresh
        if(DateTime.Now.Minute != _lastTimeRefreshedWhileOpen.Minute)
        {
            _lastTimeRefreshedWhileOpen = DateTime.Now;
            RefreshTimerOccurred?.Invoke();
            Debug.Log("App is open and a refresh has occurred!");     
        }
    }

    public async void CheckElaspedTimeOnLogin()
    {
        _lastTimeAppWasOpened = await DataManager.LoadData<DateTime>("lastTimeAppWasOpened");

        if (DateTime.Now.Minute != _lastTimeAppWasOpened.Minute)
        {
            Debug.Log("Minute has changed since last login! Refresh!");
            RefreshTimerOccurred?.Invoke();
        }
        else
        {
            Debug.Log("Minute has not changed, don't refresh!");
        }

        // Save login time when opening the app
        SaveLoginTime();
    }

    private async void SaveLoginTime()
    {
        // Save login time when opening the app
        _lastTimeAppWasOpened = DateTime.Now;

        await DataManager.SaveData("lastTimeAppWasOpened", _lastTimeAppWasOpened);
    }
}
