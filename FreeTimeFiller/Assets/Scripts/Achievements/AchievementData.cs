using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement Data", menuName = "ScriptableObjects/Achievement Data/Generic Achievement")]
public class AchievementData : ScriptableObject
{
    // Name of achievement displayed on screen
    public string achievementName;

    [Space(10)]
    public AchievementConditionType conditionType;

    // What is the value the user needs to reach to earn this achievement?
    [Range(1, 10000), Space(10)] public int targetValue;

    public Sprite iconSprite;
    
    [TextArea(2,3), Space(10)]
    public string description;

    // Category that needs to be completed to progress this achievement (only for TasksOfTypeCompleted)
    public TaskCategory taskCategory;
}

public enum AchievementConditionType
{
    CoinsEarned,
    TasksDeleted,
    TasksCompleted,
    TasksOfTypeCompleted,
    ProfilePicturesEarned
}
