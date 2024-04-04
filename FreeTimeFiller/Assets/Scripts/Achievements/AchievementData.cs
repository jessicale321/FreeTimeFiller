using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Task Data", menuName = "ScriptableObjects/Achievement Data")]
public class AchievementData : ScriptableObject
{
    // Name of achievement displayed on screen
    public string achievementName;

    [Space(10)]
    public AchievementConditionType conditionType;

    // What is the value the user needs to reach to earn this achievement?
    [Range(1, 10000), Space(10)] public int targetValue;
    
    [TextArea(2,3), Space(10)]
    public string description;
}

public enum AchievementConditionType
{
    CoinsEarned,
    TasksDeleted,
    TasksCompleted,
    TasksOfTypeCompleted,
    ProfilePicturesEarned
}
