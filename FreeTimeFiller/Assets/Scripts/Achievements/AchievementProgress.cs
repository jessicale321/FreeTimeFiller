using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementProgress
{
    public string achievementName;
    // What's the progress value at? ex. 3 out of 5 tasks completed
    public int currentValue;
    // Is the achievement completed?
    public bool completed;

    public AchievementProgress(string nameOfAchievement)
    {
        achievementName = nameOfAchievement;
        currentValue = 0;
        completed = false;
    }
}
