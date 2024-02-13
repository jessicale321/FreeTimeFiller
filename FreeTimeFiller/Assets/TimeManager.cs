using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private DateTime _currentTime;

    private void Update()
    {
        _currentTime = DateTime.Now;
    }

    ///-///////////////////////////////////////////////////////////
    /// Get the current time of day (hour: minute: seconds)
    /// 
    private DateTime GetCurrentTime()
    {
        return _currentTime;
    }

}
