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

    [Header("Close Animation")] 
    [SerializeField] private Vector2 targetScale = new Vector3(2f, 2f, 2f); // Scale to grow to
    [SerializeField] private float growDuration = 1f;  // Time taken to grow the object
    [SerializeField] private float pauseDuration = 1f; // Time to pause after growing before shrinking
    [SerializeField] private float shrinkDuration = 1f; // Time taken to shrink the object

    public Action<DisplayableAchievement> onAchievementClosed;
    
    private void OnEnable()
    {
        if(exitButton != null)
            exitButton.onClick.AddListener(DestroyOnExit);
    }

    private void OnDisable()
    {
        if(exitButton != null)
            exitButton.onClick.RemoveListener(DestroyOnExit);
        
        onAchievementClosed?.Invoke(this);
    }

    ///-///////////////////////////////////////////////////////////
    /// Show the details of an earned achievement on the screen.
    /// 
    public void DisplayAchievementPopUp(AchievementData achievementData)
    {
        if(icon != null && achievementData.iconSprite != null)
            icon.sprite = achievementData.iconSprite;
        
        if(achievementName != null)
            achievementName.text = achievementData.achievementName;
        
        if(description != null)
            description.text = achievementData.description;
    }

    ///-///////////////////////////////////////////////////////////
    /// When the user hits the exit button, shrink this gameObject and then destroy it.
    /// 
    private void DestroyOnExit()
    {
        // Grow the object
        LeanTween.scale(gameObject, targetScale, growDuration)
            .setEase(LeanTweenType.easeOutExpo)
            .setOnComplete(() =>
            {
                // Pause for a moment
                LeanTween.delayedCall(pauseDuration, () =>
                {
                    // Shrink the object
                    LeanTween.scale(gameObject, Vector3.zero, shrinkDuration)
                        .setEase(LeanTweenType.easeInExpo)
                        .setOnComplete(() => Destroy(gameObject));
                });
            });
    }
}
