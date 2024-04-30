using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering;

///-///////////////////////////////////////////////////////////
/// 
public class ProfilePictureManager : MonoBehaviour
{
    public static ProfilePictureManager instance { get; private set; }

    [SerializeField] private Image tempProfilePic;
    [SerializeField] private GameObject unlockablePicPrefab;
    [SerializeField] private Transform picPanel;

    private string resourceDirectory = "Profile Pic Data";

    private Dictionary<string, ProfilePicData> _profilePicsByName = new Dictionary<string, ProfilePicData>();
    private Dictionary<Sprite, ProfilePicData> _profilePicsBySprite = new Dictionary<Sprite, ProfilePicData>();
    private Dictionary<string, bool> _profilePicStates = new Dictionary<string, bool>();

    // The current profile picture the user has equipped
    private Sprite _currentProfilePicture;

    ///-///////////////////////////////////////////////////////////
    /// 
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        LoadProfilePictures();
        LoadCurrentPicFromCloudSave();
    }


    private async void LoadProfilePictures()
    {
        ProfilePicData[] profilePicDatas = Resources.LoadAll<ProfilePicData>(resourceDirectory);

        foreach (ProfilePicData profilePicData in profilePicDatas)
        {
            Debug.Log("Profile picture loaded: " + profilePicData.pictureName);

            _profilePicStates.Add(profilePicData.pictureName, false);
            _profilePicsByName.Add(profilePicData.pictureName, profilePicData);
            _profilePicsBySprite.Add(profilePicData.sprite, profilePicData);

            DisplayProfilePic(profilePicData);
        }

        LoadProfilePictureUnlocks();
    }

    public void PicClicked(ProfilePicData picData)
    {
        Debug.Log($"Clicked on {picData.pictureName} picture");

        // TODO: UNLOCKS PIC FOR DEBUGGING PURPOSES
        _profilePicStates[picData.pictureName] = true;
        SaveProfilePictureUnlocks();

        SetProfilePic(picData);
    }

    private async void SaveProfilePictureUnlocks()
    {
        // Convert the list to a serializable data structure (e.g., a list of tuples)
        List<Tuple<string, bool>> statesListOfTuples = new List<Tuple<string, bool>>();

        // Add each key value pair to the list of tuples
        foreach (KeyValuePair<string, bool> pair in _profilePicStates)
        {
            statesListOfTuples.Add(new Tuple<string, bool>(pair.Key, pair.Value));
        }

        // Save list of tuples
        await DataManager.SaveData("profilePicStates", _profilePicStates);
    }

    private async void LoadProfilePictureUnlocks()
    {
        // Find previously saved profile pic state data
        List<Tuple<string, bool>> statesListOfTuples =  
            await DataManager.LoadData<List<Tuple<string, bool>>>("profilePicStates");

        if(statesListOfTuples != null)
        {
            foreach (Tuple<string, bool> pair in statesListOfTuples)
            {
                _profilePicStates[pair.Item1] = pair.Item2;
            }
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    public async void SetProfilePic(ProfilePicData newPicData)
    {
        _currentProfilePicture = newPicData.sprite;

        tempProfilePic.sprite = _currentProfilePicture;

        await DataManager.SaveData("currentProfilePictureName", newPicData.pictureName);
    }

    ///-///////////////////////////////////////////////////////////
    /// Same as ProfileScreenController.cs -- maybe use Russell's data manager to avoid redundancy
    ///
    public async void LoadCurrentPicFromCloudSave()
    {
        string currentProfilePicString = await DataManager.LoadData<string>("currentProfilePictureName");

        if (!String.IsNullOrEmpty(currentProfilePicString))
        {
            _currentProfilePicture = _profilePicsByName[currentProfilePicString].sprite;

            tempProfilePic.sprite = _currentProfilePicture;

            Debug.Log($"The user's current profile pic is {currentProfilePicString}");
        }
        else
        {
            Debug.LogError("Current profile picture was null or empty!");
        }
    }

    private void DisplayProfilePic(ProfilePicData picData)
    {
        GameObject picInstance = Instantiate(unlockablePicPrefab, picPanel);

        picInstance.GetComponent<UnlockableProfilePic>().SetProfilePictureData(picData);
    }

    public Sprite GetCurrentProfilePicture()
    {
        return _currentProfilePicture;
    }
}

