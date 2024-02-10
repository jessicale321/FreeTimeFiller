using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Task Data", menuName = "ScriptableObjects/Task Data")]
public class TaskData : ScriptableObject
{
    public string taskName;

    [Space(10)]
    public TaskCategory category;
    
    [Range(0, 5)]
    public int difficultyLevel;

    [Range(1, 5)] 
    public int refreshCountdown = 1;
    
    [TextArea(2,3), Space(10)]
    public string description;
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
