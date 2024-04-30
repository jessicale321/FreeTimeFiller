using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

///-///////////////////////////////////////////////////////////
/// 
public class SelectProfileController : MonoBehaviour
{
    public static SelectProfileController instance { get; private set; }

    [SerializeField] private Image tempProfilePic;
    [SerializeField] private GameObject unlockablePicPrefab;
    [SerializeField] private Transform picPanel;

    private string resourceDirectory = "Profile Pic Data";

    private Dictionary<string, ProfilePicData> _profilePicsByName = new Dictionary<string, ProfilePicData>();
    private Dictionary<Sprite, ProfilePicData> _profilePicsBySprite = new Dictionary<Sprite, ProfilePicData>();
    private Dictionary<string, bool> _profilePicStates = new Dictionary<string, bool>();

    // The current profile picture the user has equipped
    private Image _currentProfilePicture;

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
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    private void OnEnable()
    {
        LoadProfilePictures();
        LoadCurrentPicFromCloudSave(); // use Russell's data manager?
        //LoadProfilePicUnlockedStatus();
        DisplayProfilePics();
    }

    private async void LoadProfilePictures()
    {
        ProfilePicData[] profilePicDatas = Resources.LoadAll<ProfilePicData>(resourceDirectory);

        foreach (var profilePicData in profilePicDatas)
        {
            Debug.Log("Profile picture loaded: " + profilePicData.pictureName);

            _profilePicStates.Add(profilePicData.pictureName, false);
            _profilePicsByName.Add(profilePicData.pictureName, profilePicData);
            _profilePicsBySprite.Add(profilePicData.sprite, profilePicData);
        }

        LoadProfilePictureUnlocks();
    }

    public void PicClicked(Image picture)
    {
        ProfilePicData picData = _profilePicsBySprite[picture.sprite];

        Debug.Log($"Clicked on {picData.pictureName} picture");
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
        await DataManager.SaveData("currentProfilePicture", newPicData.pictureName);
        // current bug: might have duplicate with temp and in the grid
        //Image currentProfile = tempProfilePic.GetComponent<Image>(); // might not need this line?

        //tempProfilePic.sprite = newProfilePic.sprite;

        //Texture2D texture = ImageToTexture2D(currentProfile);

        //byte[] imageData = texture.EncodeToPNG();

        //// Save the byte array as a cloud save file
        //try
        //{
        //    await CloudSaveService.Instance.Files.Player.SaveAsync("profileImage.png", imageData);
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError($"Failed to save image as cloud save file: {e.Message}");
        //}
    }

    ///-///////////////////////////////////////////////////////////
    /// Convert Image to Texture2D 
    ///
    private Texture2D ImageToTexture2D(Image image)
    {
        Texture2D texture = new Texture2D((int)image.rectTransform.rect.width, (int)image.rectTransform.rect.height);
        RenderTexture renderTexture = new RenderTexture((int)image.rectTransform.rect.width, (int)image.rectTransform.rect.height, 32);
        Graphics.Blit(image.sprite.texture, renderTexture);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        return texture;
    }

    ///-///////////////////////////////////////////////////////////
    /// Same as ProfileScreenController.cs -- maybe use Russell's data manager to avoid redundancy
    ///
    public async void LoadCurrentPicFromCloudSave()
    {
        string currentProfilePicString = await DataManager.LoadData<string>("currentProfilePicture");

        Debug.Log($"The user's current profile pic is {currentProfilePicString}");

        //    // Set the loaded texture to the Image component
        //    tempProfilePic.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError($"Failed to load image from cloud save file: {e.Message}");
        //}
    }

    private void DisplayProfilePics()
    {
        ProfilePicData[] loadedPics = Resources.LoadAll<ProfilePicData>(resourceDirectory);
        foreach (ProfilePicData pic in loadedPics)
        {
            GameObject picInstance = Instantiate(unlockablePicPrefab, picPanel);

            Image picImage = picInstance.GetComponentInChildren<Image>();
            picImage.sprite = pic.sprite;
        }
    }
}

