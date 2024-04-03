using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayableAchievement : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    
    [Header("Achievement Details")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text achievementName;
    [SerializeField] private TMP_Text description;

    private void OnEnable()
    {
        if(exitButton != null)
            exitButton.onClick.AddListener(DestroyOnExit);
    }

    private void OnDisable()
    {
        if(exitButton != null)
            exitButton.onClick.RemoveListener(DestroyOnExit);
    }

    ///-///////////////////////////////////////////////////////////
    /// Show the details of an earned achievement on the screen.
    /// 
    public void DisplayAchievementPopUp(AchievementData achievementData)
    {
        if(icon != null)
            icon.sprite = achievementData.iconSprite;
        
        if(achievementName != null)
            achievementName.text = achievementData.achievementName;
        
        if(description != null)
            description.text = achievementData.description;
    }

    ///-///////////////////////////////////////////////////////////
    /// Destroy this button when the user hits the exit button
    /// 
    private void DestroyOnExit()
    {
        Destroy(gameObject);
    }
}
