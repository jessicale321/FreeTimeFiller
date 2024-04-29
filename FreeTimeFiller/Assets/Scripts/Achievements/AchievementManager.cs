using System;
using System.Collections;
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

    private List<AchievementData> _allEarnedAchievements = new List<AchievementData>();

    [SerializeField] private Transform achievementPanel;
    [SerializeField] private GameObject displayableAchievementPrefab;

    // Is there an achievement currently being displayed?
    private bool _displayingAchievement;
    
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

        LoadAchievements();
    }

    ///-///////////////////////////////////////////////////////////
    /// Load previous achievement progress and any achievements that have no progress yet.
    /// 
    private async void LoadAchievements()
    {
        //ClearAllAchievementProgress();
        
        // Load all previous achievement progress
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

                // If the achievement was already completed, add it to a list
                if(achievementProgress.completed)
                    _allEarnedAchievements.Add(data);
            }
        }
    }
    
    ///-///////////////////////////////////////////////////////////
    /// Track player actions and update tracked values.
    /// 
    public void UpdateProgress(AchievementConditionType conditionType, int amount = 1, TaskCategory? completedTaskCategory = null)
    {
        // Don't allow progress amount to be 0 or negative
        if (amount <= 0)
        {
            Debug.Log("Can't lose progress or make an update with zero progress!");
            return;
        }

        if (_achievementsByConditionType.TryGetValue(conditionType, out List<AchievementData> conditionAchievements))
        {
            foreach (AchievementData achievement in conditionAchievements)
            {
                /* Check if the condition matches the achievement's condition type
                If the condition type is TasksOfTypeCompleted, then...
                Only make progress for achievements of the correct task category type (i.e. Completing a sports task doesn't progress achievements for other achievements with different categories such as School) */
                if (_allAchievementProgress.ContainsKey(achievement) && (conditionType != AchievementConditionType.TasksOfTypeCompleted || completedTaskCategory == achievement.taskCategory))
                {
                    AchievementProgress achievementProgress = _allAchievementProgress[achievement];
                    
                    // If an achievement hasn't been completed and it has reached its target value, it is complete
                    // Also, don't progress achievements that are already completed
                    if (!achievementProgress.completed)
                    {
                        achievementProgress.currentValue += amount;

                        if (achievementProgress.currentValue >= achievement.targetValue)
                            StartCoroutine(CompleteAchievement(achievementProgress, achievement));
                    }

                    // Save after making any progress
                    // TODO: Might try to find a way to make this less slow
                    SaveAllAchievementProgress();
                }
            }
        }
    }
    
    ///-///////////////////////////////////////////////////////////
    /// Mark an achievement as completed.
    /// 
    private IEnumerator CompleteAchievement(AchievementProgress achievementProgress, AchievementData achievementData)
    {
        // Don't allow more than one achievement to appear at a time
        while (_displayingAchievement)
            yield return null;
        
        achievementProgress.completed = true;

        _allEarnedAchievements.Add(achievementData);

        Debug.Log($"Achievement unlocked: {achievementData.achievementName}");

        _displayingAchievement = true;
        
        // Spawn an achievement pop up on the screen
        GameObject achievementGameObject = Instantiate(displayableAchievementPrefab, achievementPanel.transform);
        DisplayableAchievement achievement =
            achievementGameObject.GetComponent<DisplayableAchievement>();
    
        achievement.DisplayAchievementPopUp(achievementData);

        // Wait for the achievement pop up to get closed, then allow another achievement to pop up
        achievement.onAchievementClosed += AchievementPopUpWasClosed;

    }

    private void AchievementPopUpWasClosed(DisplayableAchievement achievement)
    {
        _displayingAchievement = false;
        achievement.onAchievementClosed -= AchievementPopUpWasClosed;
    }

    public List<AchievementData> GetAllEarnedAchievements()
    {
        return _allEarnedAchievements;
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

    public async void ClearAllAchievementProgress()
    {
        foreach (AchievementProgress achievementProgress in _allAchievementProgress.Values)
        {
            achievementProgress.currentValue = 0;
            achievementProgress.completed = false;
        }
        
        _allEarnedAchievements.Clear();

        SaveAllAchievementProgress();
    }
}