using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    // Singleton reference for AchievementManager
    public static AchievementManager Instance;
    
    private string _resourceDirectory = "Achievement Data";
    // Key: Achievement Condition Type (ex. Coins earned)
    // Value: List of all achievement data that belong to the condition type (ex. 50 coins earned, 100 coins earned, etc.)
    private Dictionary<AchievementConditionType, List<AchievementData>> _achievementsByConditionType = 
        new Dictionary<AchievementConditionType, List<AchievementData>>();

    // Key: Name of the achievement
    // Value: The achievement's scriptableObject
    private Dictionary<string, AchievementData> _allAchievementsByName = new Dictionary<string, AchievementData>();

    // Key: The data of an achievement (ex. 50 coins earned)
    // How close the user is to completing the achievement (ex. is it completed?)
    private Dictionary<AchievementData, AchievementProgress> _allAchievementProgress =
        new Dictionary<AchievementData, AchievementProgress>();

    private async void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += LoadAchievements;

        //ClearAllAchievementProgress();
    }

    private void OnDestroy()
    {
        AuthenticationService.Instance.SignedIn -= LoadAchievements;
    }

    private async void LoadAchievements()
    {
        // TODO: load all achievement progress first
        AchievementData[] loadedAchievementData = Resources.LoadAll<AchievementData>(_resourceDirectory);
        
        foreach (AchievementData achievementData in loadedAchievementData)
        {
            Debug.Log($"Achievement loaded: {achievementData.achievementName}");

            // Add name of the achievement to a dictionary
            _allAchievementsByName.Add(achievementData.achievementName, achievementData);
            
            // If we don't already have an achievement condition type, then make a list of achievement data for it
            if (!_achievementsByConditionType.ContainsKey(achievementData.conditionType))
            {
                _achievementsByConditionType.Add(achievementData.conditionType, new List<AchievementData>());
            }
            // Add the achievement to the list of achievements (list by condition type)
            _achievementsByConditionType[achievementData.conditionType].Add(achievementData);
            
            // Create progression for the achievement
            _allAchievementProgress.Add(achievementData, new AchievementProgress(achievementData.achievementName));
        }
        
        List<AchievementProgress> achievementProgressLoaded = await LoadPreviousAchievementProgress();

        // If we loaded any previously saved achievement progress
        if (achievementProgressLoaded != null)
        {
            foreach (AchievementProgress achievementProgress in achievementProgressLoaded)
            {
                // Find the AchievementData scriptableObject by its name
                AchievementData data = _allAchievementsByName[achievementProgress.achievementName];
                
                // Override the old achievement progress with the previously saved one
                _allAchievementProgress[data] = achievementProgress;
                
                Debug.Log($"Found and loaded {achievementProgress.achievementName} from previous achievement progress save! \n It has {achievementProgress.currentValue} / {data.targetValue}");
            }
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Track player actions and update tracked values.
    /// 
    public void UpdateProgress(AchievementConditionType conditionType, int amount = 1)
    {
        if (amount <= 0)
        {
            Debug.Log("Can't lose or make an update with zero progress!");
            return;
        }
        
        if (_achievementsByConditionType.TryGetValue(conditionType, out List<AchievementData> conditionAchievements))
        {
            foreach (AchievementData achievement in conditionAchievements)
            {
                AchievementProgress achievementProgress = _allAchievementProgress[achievement];

                achievementProgress.currentValue += amount;
                
                if (!achievementProgress.completed && achievementProgress.currentValue >= achievement.targetValue)
                {
                    CompleteAchievement(achievementProgress, achievement);
                }
                
                // Save after making any progress
                // TODO: Might try to find a way to make this less slow
                SaveAllAchievementProgress();
            }
        }
    }

    public void UpdateTaskOfTypeProgress(TaskCategory category, int amount = 1)
    {
        if (_achievementsByConditionType.TryGetValue(AchievementConditionType.TasksOfTypeCompleted, out List<AchievementData> conditionAchievements))
        {
            foreach (AchievementData achievement in conditionAchievements)
            {
                AchievementProgress achievementProgress = _allAchievementProgress[achievement];

                achievementProgress.currentValue += amount;
                
                if (!achievementProgress.completed && achievementProgress.currentValue >= achievement.targetValue)
                {
                    CompleteAchievement(achievementProgress, achievement);
                }
                
                // Save after making any progress
                // TODO: Might try to find a way to make this less slow
                SaveAllAchievementProgress();
            }
        }
    }
    
    ///-///////////////////////////////////////////////////////////
    /// Mark an achievement as completed.
    /// 
    private void CompleteAchievement(AchievementProgress achievementProgress, AchievementData achievementData)
    {
        achievementProgress.completed = true;
        Debug.Log("Achievement unlocked: " + achievementData.description);
    }

    ///-///////////////////////////////////////////////////////////
    /// Save all achievement progress made by the user so far.
    /// 
    private async void SaveAllAchievementProgress()
    {
        List<AchievementProgress> achievementProgressToSave = new List<AchievementProgress>();

        // Only save data for achievements that the user has some progress for
        foreach (AchievementProgress achievementProgress in _allAchievementProgress.Values)
        {
            if(achievementProgress.currentValue > 0)
                achievementProgressToSave.Add(achievementProgress);
        }
        
        if (achievementProgressToSave.Count > 0)
        {
            await DataManager.SaveData("allAchievementProgress", achievementProgressToSave);
        
            Debug.Log("Saved achievement progress");
        }
        else
        {
            Debug.Log("No achievement progress was saved because there is none.");
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Return all achievement progress saved to the user's account.
    /// 
    private async Task<List<AchievementProgress>> LoadPreviousAchievementProgress()
    { 
        return await DataManager.LoadData<List<AchievementProgress>>("allAchievementProgress");
    }

    private async void ClearAllAchievementProgress()
    {
        await DataManager.DeleteAllDataByName("allAchievementProgress");
    }
}