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

    [SerializeField] private ProfilePicData defaultProfilePicture;
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

        // Load all profile pictures, then user saved data
        LoadProfilePictures();
        LoadCurrentPicFromCloudSave();

        //ClearProfilePictureUnlocks();
    }


    private async void LoadProfilePictures()
    {
        // Find all profile picture scriptableObjects created
        ProfilePicData[] profilePicDatas = Resources.LoadAll<ProfilePicData>(resourceDirectory);

        foreach (ProfilePicData profilePicData in profilePicDatas)
        {
            Debug.Log("Profile picture loaded: " + profilePicData.pictureName);

            // Map the picture's name to whether or not it has been unlocked
            _profilePicStates.Add(profilePicData.pictureName, false);
            // Map the picture's name to it's scriptableObject
            _profilePicsByName.Add(profilePicData.pictureName, profilePicData);
            // Map the picture's sprite to it's scriptableObject
            _profilePicsBySprite.Add(profilePicData.sprite, profilePicData);

            // Spawn in the profile picture 
            DisplayProfilePic(profilePicData);
        }

        // Load in all previously unlocked profile pictures
        LoadProfilePictureUnlocks();
    }

    private async void LoadProfilePictureUnlocks()
    {
        // Find previously saved profile pic state data
        List<Tuple<string, bool>> statesListOfTuples =
            await DataManager.LoadData<List<Tuple<string, bool>>>("profilePicStates");

        if (statesListOfTuples != null)
        {
            // Override unlock progress with previously saved unlocks (i.e. re-unlock profile pictures that were unlocked)
            foreach (var (pictureName, isUnlocked) in statesListOfTuples)
            {
                _profilePicStates[pictureName] = isUnlocked;

                if (isUnlocked)
                    Debug.Log($"{pictureName} was already unlocked!");
            }
        }
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
        // Convert dictionary to a list of KeyValuePairs
        List<KeyValuePair<string, bool>> picStatesList = _profilePicStates.ToList();

        // Convert the list to a serializable data structure (e.g., a list of tuples)
        List<Tuple<string, bool>> statesListOfTuples = new List<Tuple<string, bool>>();
        foreach (var pair in picStatesList)
        {
            statesListOfTuples.Add(new Tuple<string, bool>(pair.Key, pair.Value));
        }

        // Save list of tuples
        await DataManager.SaveData("profilePicStates", statesListOfTuples);
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
            Debug.LogError("Current profile picture was null or empty! Will use default profile picture");
            _currentProfilePicture = defaultProfilePicture.sprite;
        }
    }

    private async void ClearProfilePictureUnlocks()
    {
        await DataManager.DeleteAllDataByName("profilePicStates");
        await DataManager.DeleteAllDataByName("currentProfilePictureName");
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

