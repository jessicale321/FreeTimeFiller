using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class TaskRefreshWithTime: MonoBehaviour
{
    [SerializeField] private TMP_Text refreshTimerDisplay;
    
    // DateTime for last time user opened or closed the app
    private DateTime _lastTimeAppWasOpened;
    // DateTime for the last time that tasks refreshed
    private DateTime _lastTimeRefreshedWhileOpen;

    // Don't let update and app opening refreshes to occur simultaneously 
    private bool _currentlyRefreshingFromAppOpen;
    
    public Action refreshTimerOccurred;

    private void Awake()
    {
        _currentlyRefreshingFromAppOpen = false;
        
        _lastTimeRefreshedWhileOpen = DateTime.Now;
    }

    private void OnApplicationQuit()
    {
        // Save login time when closing the app
        SaveLoginTime();
    }

    private void Update()
    {
        // Always display time until refresh occurs
        DisplayTimeUntilMidnight();
        
        // Check every frame if the time should refresh
        if(DateTime.Now.Day != _lastTimeRefreshedWhileOpen.Day && !_currentlyRefreshingFromAppOpen)
        {
            _lastTimeRefreshedWhileOpen = DateTime.Now;
            refreshTimerOccurred?.Invoke();
            Debug.Log("App is open and a refresh has occurred!");     
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// When the app is opened, check if there has been a day that has elapsed and if so,
    /// tell listeners that the refresh timer has occurred.
    /// 
    public async void CheckElapsedTimeOnLogin()
    {
        _lastTimeAppWasOpened = await DataManager.LoadData<DateTime>("lastTimeAppWasOpened");

        if (DateTime.Now.Day != _lastTimeAppWasOpened.Day)
        {
            Debug.Log("Day has changed since last login! Refresh!");
            _currentlyRefreshingFromAppOpen = true;
            refreshTimerOccurred?.Invoke();
        }
        else
        {
            Debug.Log("Day has not changed, don't refresh!");
        }

        _currentlyRefreshingFromAppOpen = false;
        
        // Save login time when opening the app
        SaveLoginTime();
    }

    ///-///////////////////////////////////////////////////////////
    /// Display how much time there is left until midnight (when refresh occurs).
    /// 
    private void DisplayTimeUntilMidnight()
    {
        // Get the current time
        DateTime currentTime = DateTime.Now;

        // Get the time remaining until midnight
        DateTime nextMidnight = currentTime.Date.AddDays(1);
        TimeSpan timeUntilMidnight = nextMidnight - currentTime;
        
        // Display the time remaining
        string timeRemainingText = $"{timeUntilMidnight.Hours}hr {timeUntilMidnight.Minutes}m {timeUntilMidnight.Seconds}s";
        refreshTimerDisplay.text = "Next Refresh: " + timeRemainingText;
    }

    ///-///////////////////////////////////////////////////////////
    /// Save the last DateTime in which the user opened the app.
    /// 
    private async void SaveLoginTime()
    {
        // Save login time when opening the app
        _lastTimeAppWasOpened = DateTime.Now;

        await DataManager.SaveData("lastTimeAppWasOpened", _lastTimeAppWasOpened);
    }
}
