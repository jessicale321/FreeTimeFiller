using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Task Data", menuName = "ScriptableObjects/Task Data/Regular")]
public class TaskData : ScriptableObject
{
    // Name of task displayed on screen
    public string taskName;

    [Space(10)]
    // Category that the task belongs to
    public TaskCategory category;
    
    [Range(0, 5)]
    // How many stars appear on screen?
    public int difficultyLevel;

    [Range(1, 5)] 
    // How many refreshes (after this task is completed) until this task reappears in the pool
    public int refreshCountdown = 1;
    
    [TextArea(2,3), Space(10)]
    public string description;

    public int GetDeletePrice()
    {
        return difficultyLevel * 5;
    }

    public int GetRewardAmount()
    {
        return difficultyLevel * 3;
    }
}

public enum TaskCategory
{
    Sports,
    Chores,
    Hobby,
    Fun,
    Healthy,
    School,
    Workout,
    Other
}
