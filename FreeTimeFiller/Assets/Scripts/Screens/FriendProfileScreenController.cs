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
    [SerializeField] private TMP_Text username;
    [SerializeField] private UserDatabase userDatabase;
    [SerializeField] private Transform achievementsPanel;
    [SerializeField] private GameObject achievementPrefab;

    private void OnEnable()
    {
        Invoke(nameof(GetPlayerData), 2f);
    }
    /*public async void getPlayerData()
    {
        Debug.Log("We are running the getPlayerData code");
        var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { "currentProfilePictureName" }, new LoadOptions(new PublicReadAccessClassOptions(
               await userDatabase.GetUseridByUsername(username.text))));
        if(playerData.TryGetValue("currentProfilePictureName", out var dataLoaded) )
        {
            Debug.Log($"This is the player profile {dataLoaded.Value.GetAs<string>()}");
        }
        else
        {
            Debug.Log("Found nothing");
        }
    }*/
    public async void GetPlayerData()
    {
        //Debug.Log("We are running the getPlayerData code");
        //var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(
        //    new HashSet<string> { "currentProfilePictureName" }, new LoadOptions(new PublicReadAccessClassOptions(
        //       await userDatabase.GetUseridByUsername(username.text))));

        //foreach (var entry in playerData)
        //{
        //    Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
        //}

        //if (playerData.TryGetValue("currentProfilePictureName", out var dataLoaded))
        //{
        //    Debug.Log($"This is the player profile {dataLoaded.Value.GetAs<string>()}");
        //}
        //else
        //{
        //    Debug.Log("Found nothing");
        //}

        Debug.Log(await UserDatabase.Instance.GetProfilePicture(username.text));
    }
}
