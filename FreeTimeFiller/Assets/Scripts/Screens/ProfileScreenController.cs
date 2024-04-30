using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

///-///////////////////////////////////////////////////////////
/// 
public class ProfileScreenController : MonoBehaviour
{
    [SerializeField] private Image profilePicture;
    [SerializeField] private TMP_Text username;
    [SerializeField] private UserDatabase userDatabase;
    [SerializeField] private Transform achievementsPanel;
    [SerializeField] private GameObject achievementPrefab;

    ///-///////////////////////////////////////////////////////////
    /// 
    private void OnEnable()
    {
        LoadImageFromCloudSave();
        SetUsername();
        ClearAchievementIcons();
        DisplayAchievements();
    }
    
    private async void SetUsername()
    {
        username.text = await userDatabase.GetUsernameByUserid(AuthenticationService.Instance.PlayerId);
    }

    ///-///////////////////////////////////////////////////////////
    /// Load the image from the cloud save file
    ///
    public void LoadImageFromCloudSave()
    {
        Sprite loadedSprite = ProfilePictureManager.instance.GetCurrentProfilePicture();
        profilePicture.sprite = loadedSprite;
    }

    ///-///////////////////////////////////////////////////////////
    /// Display completed achievements
    ///
    private void DisplayAchievements()
    {
        List<AchievementData> completedAchievements = AchievementManager.Instance.GetAllEarnedAchievements();

        foreach (AchievementData achievement in completedAchievements)
        {
            GameObject achievementObj = Instantiate(achievementPrefab, achievementsPanel);

            // Get Image component from the prefab
            Image achievementImage = achievementObj.GetComponent<Image>();

            // Set image sprite to the iconSprite of the achievement
            achievementImage.sprite = achievement.iconSprite;
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
