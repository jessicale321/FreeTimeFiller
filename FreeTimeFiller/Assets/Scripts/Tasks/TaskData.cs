using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Task Data", menuName = "ScriptableObjects/Task Data")]
public class TaskData : ScriptableObject
{
    public string taskName;

    [Space(10)]
    public Category category;
    
    [Range(0, 5)]
    public int difficultyLevel;
    
    [TextArea(2,3), Space(10)]
    public string description;
}

public enum Category
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
