using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetTaskAndAchievement : MonoBehaviour
{
    private Button _button;

    [Header("Required Scripts")] 
    [SerializeField] private TaskPlacer taskPlacer;
    [SerializeField] private AchievementManager achievementManager;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(ResetOnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(ResetOnClick);
    }

    private void ResetOnClick()
    {
        if(taskPlacer != null)
            taskPlacer.ClearTaskPlacement();
        
        if(achievementManager != null)
            achievementManager.ClearAllAchievementProgress();
    }
}
