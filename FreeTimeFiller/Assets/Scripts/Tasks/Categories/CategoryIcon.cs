using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Category Icons", menuName = "ScriptableObjects/Task Category Icons")]
public class CategoryIcon : ScriptableObject
{
    public Sprite sports;
    public Sprite chores;
    public Sprite hobby;
    public Sprite fun;
    public Sprite healthy;
    public Sprite school;
    public Sprite workout;
    public Sprite other;

    ///-///////////////////////////////////////////////////////////
    /// Return a sprite that represents the task category.
    /// 
    public Sprite GetCategoryIcon(TaskCategory category)
    {
        switch (category)
        {
            case TaskCategory.Sports:
                return sports;
            case TaskCategory.Chores:
                return chores;
            case TaskCategory.Hobby:
                return hobby;
            case TaskCategory.Fun:
                return fun;
            case TaskCategory.Healthy:
                return healthy;
            case TaskCategory.School:
                return school;
            case TaskCategory.Workout:
                return workout;
            case TaskCategory.Other:
                return other;
        }
        return null;
    }
}
