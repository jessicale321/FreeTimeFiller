using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    private string _resourceDirectory = "Achievement Data";
    // Key: Achievement Condition Type (ex. Coins earned)
    // Value: List of all achievement data that belong to the condition type (ex. 50 coins earned, 100 coins earned, etc.)
    private Dictionary<AchievementConditionType, List<AchievementData>> _achievementsByConditionType = 
        new Dictionary<AchievementConditionType, List<AchievementData>>();

    private List<AchievementProgress> _achievementProgresses = new List<AchievementProgress>();

    private void Awake()
    {
        // TODO: load all achievement progress!
        
        AchievementData[] loadedAchievements = Resources.LoadAll<AchievementData>(_resourceDirectory);

        foreach (AchievementData achievementData in loadedAchievements)
        {
            Debug.Log($"Achievement loaded: {achievementData.achievementName}");

            // If we don't already have an achievement condition type, then make a list of achievement data for it
            if (!_achievementsByConditionType.ContainsKey(achievementData.conditionType))
            {
                _achievementsByConditionType.Add(achievementData.conditionType, new List<AchievementData>());
            }
            _achievementsByConditionType[achievementData.conditionType].Add(achievementData);
        }
    }
    
    // Track player actions and update tracked values
    // public void UpdateProgress(AchievementConditionType conditionType, int amount = 1)
    // {
    //     if (_achievementsByConditionType.TryGetValue(conditionType, out List<AchievementData> conditionAchievements))
    //     {
    //         foreach (AchievementData achievement in conditionAchievements)
    //         {
    //             achievement.currentValue += amount;
    //             if (!achievement.completed && achievement.currentValue >= achievement.targetValue)
    //             {
    //                 CompleteAchievement(achievement);
    //             }
    //         }
    //     }
    // }
    //
    // // Mark an achievement as completed
    // private void CompleteAchievement(AchievementData achievement)
    // {
    //     achievement.completed = true;
    //     NotifyAchievementCompleted(achievement);
    // }
    //
    // // Notify the player when an achievement is completed
    // private void NotifyAchievementCompleted(AchievementData achievement)
    // {
    //     Debug.Log("Achievement unlocked: " + achievement.description);
    //     // Notify the player in-game
    // }
}