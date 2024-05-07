using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class FriendProfileScreenController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image profilePicture;
    [SerializeField] private Transform achievementsPanel;
    [SerializeField] private GameObject reactableAchievementPrefab;

    // Locally saved data for user we're viewing
    private string _currentUserName;
    private int _achievementReactionCount;

    private List<Button> _allReactableAchievementButtons = new List<Button>();

    private void OnEnable()
    {
        ClearAchievementIcons();
        _currentUserName = string.Empty;
        _achievementReactionCount = 0;
    }

    public async void LoadFriendData(string userName)
    {
        _currentUserName = userName;
        _achievementReactionCount = await UserDatabase.Instance.GetDataFromUserName<int>(userName, "number_of_achievement_reactions");

        // Load the other user's profile picture
        string profilePictureName = await UserDatabase.Instance.GetDataFromUserName<string>(userName, "current_profile_picture_name");
        profilePicture.sprite = ProfilePictureManager.instance.GetProfilePictureByString(profilePictureName);

        // Load the other user's completed achievements and place them on the panel
        List<string> completedAchievements = await UserDatabase.Instance.GetDataFromUserName<List<string>>(userName, "completed_achievements");

        foreach(string achievementName in completedAchievements)
        {
            AchievementData achievementData = AchievementManager.Instance.GetAchievementByName(achievementName);

            GameObject achievementObj = Instantiate(reactableAchievementPrefab, achievementsPanel);

            Button achievementButton = achievementObj.GetComponent<Button>();

            achievementButton.onClick.AddListener(ReactToFriendAchievement);

            _allReactableAchievementButtons.Add(achievementButton);

            // Get Image component from the prefab
            Image achievementImage = achievementObj.GetComponent<Image>();

            // Set image sprite to the iconSprite of the achievement
            achievementImage.sprite = achievementData.iconSprite;
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// When the user clicks on a friend's achievement, they will increment their friend's achievement reaction count by 1.
    /// The new reaction count will be saved to the friend's account.
    ///
    private void ReactToFriendAchievement()
    {
        Debug.Log("Reacted to friend's achievement!");
        UserDatabase.Instance.SaveDataToUsername(_currentUserName, "number_of_achievement_reactions", ++_achievementReactionCount);
    }

    ///-///////////////////////////////////////////////////////////
    ///
    private void ClearAchievementIcons()
    {
        foreach (Button button in _allReactableAchievementButtons)
        {
            button.onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }
    }
}
