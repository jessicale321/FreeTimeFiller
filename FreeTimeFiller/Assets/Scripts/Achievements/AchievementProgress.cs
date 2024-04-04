using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementProgress
{
    public AchievementData achievement;
    public int currentValue;
    public bool completed;

    public AchievementProgress(AchievementData achievement)
    {
        this.achievement = achievement;
        currentValue = 0;
        completed = false;
    }
}
