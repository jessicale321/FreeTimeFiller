using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using UnityEngine.UI;


public class FriendProfileScreenController : MonoBehaviour
{
    [SerializeField] private Image profilePicture;
    [SerializeField] private Transform achievementsPanel;
    [SerializeField] private GameObject achievementPrefab;

    public async void LoadFriendData(string userName)
    {
        string profilePictureName = await UserDatabase.Instance.GetProfilePicture(userName);
        profilePicture.sprite = ProfilePictureManager.instance.GetProfilePictureByString(profilePictureName);
    }
}
