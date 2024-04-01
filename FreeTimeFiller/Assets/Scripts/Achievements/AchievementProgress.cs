using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementProgress
{
    public string achievementName;
    public int currentValue;
    public bool completed;

    public AchievementProgress(string nameOfAchievement)
    {
        achievementName = nameOfAchievement;
        currentValue = 0;
        completed = false;
    }
}
