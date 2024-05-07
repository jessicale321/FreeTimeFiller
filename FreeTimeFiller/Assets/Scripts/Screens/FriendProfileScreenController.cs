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
    [SerializeField] private Image profilePicture;
    [SerializeField] private Transform achievementsPanel;
    [SerializeField] private GameObject achievementPrefab;

    private void OnEnable()
    {
        ClearAchievementIcons();
    }

    public async void LoadFriendData(string userName)
    {
        // Load the other user's profile picture
        string profilePictureName = await UserDatabase.Instance.GetDataFromUserName<string>(userName, "current_profile_picture_name");
        profilePicture.sprite = ProfilePictureManager.instance.GetProfilePictureByString(profilePictureName);

        // Load the other user's completed achievements and place them on the panel
        List<string> completedAchievements = await UserDatabase.Instance.GetDataFromUserName<List<string>>(userName, "completed_achievements");

        foreach(string achievementName in completedAchievements)
        {
            AchievementData achievementData = AchievementManager.Instance.GetAchievementByName(achievementName);

            GameObject achievementObj = Instantiate(achievementPrefab, achievementsPanel);

            // Get Image component from the prefab
            Image achievementImage = achievementObj.GetComponent<Image>();

            // Set image sprite to the iconSprite of the achievement
            achievementImage.sprite = achievementData.iconSprite;
        }
    }

    ///-///////////////////////////////////////////////////////////
    ///
    private void ClearAchievementIcons()
    {
        foreach (Transform child in achievementsPanel)
        {
            Destroy(child.gameObject);
        }
    }
}
