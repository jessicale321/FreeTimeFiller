using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class TaskRefreshWithTime: MonoBehaviour
{
    private DateTime _lastLoginTime;

    private bool _signedIn;

    private async void Awake()
    {
        _signedIn = false;

        // When the user is signed in, begin the task placement process
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += CheckElaspedTimeOnLogin;

        _signedIn = true;
    }

    private void OnApplicationQuit()
    {
        // Save login time when closing the app
        SaveLoginTime();
    }

    private void Update()
    {
        // Check every frame if the time should refresh
        if(_signedIn && DateTime.Now.Minute != _lastLoginTime.Minute)
        {
            Debug.Log("App is open and a refresh has occurred!");
            SaveLoginTime();
        }
    }

    private void CheckElaspedTimeOnLogin()
    {
        if (DateTime.Now.Minute != _lastLoginTime.Minute)
        {
            Debug.Log("Minute has changed since last login! Refresh!");
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
        _lastLoginTime = DateTime.Now;

        await DataManager.SaveData("lastLoginTime", _lastLoginTime);
    }

    // Save login time in start/ on quit
    // when they login, check if the datetime.now day is not the same as the last time logged time
}
