using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

///-///////////////////////////////////////////////////////////
/// 
public class SelectProfileController : MonoBehaviour
{
    public static SelectProfileController instance { get; private set; }

    [SerializeField] private Image tempProfilePic;
    [SerializeField] private GameObject unlockablePicPrefab;
    [SerializeField] private Transform picPanel;
    private string resourceDirectory = "Profile Pic Data";
    private Dictionary<ProfilePicData, bool> profilePicStates = new Dictionary<ProfilePicData, bool>();


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
        LoadCurrentPicFromCloudSave(); // use Russell's data manager?
        LoadProfilePicUnlockedStatus();
        DisplayProfilePics();
    }

    ///-///////////////////////////////////////////////////////////
    /// Called from UnlockableProfilePic instances when button is clicked 
    ///
    public void PicClicked(Image picture)
    {
        Debug.Log("clicked");

        ProfilePicData picData = GetProfilePicDataFromImage(picture.sprite);
        if (picData != null)
        {
            bool isUnlocked = profilePicStates[picData];

            if (isUnlocked)
            {
                Debug.Log("unlocked");
            }
            else
            {
                Debug.Log("locked");
            }
        }
    }

    private ProfilePicData GetProfilePicDataFromImage(Sprite sprite)
    {
        ProfilePicData[] loadedPics = Resources.LoadAll<ProfilePicData>(resourceDirectory);
        foreach (ProfilePicData picData in loadedPics)
        {
            if (picData.sprite == sprite)
            {
                return picData;
            }
        }
        return null;
    }

    ///-///////////////////////////////////////////////////////////
    /// 
/*    public async void SetProfilePic(Image newProfilePic)
    {
        // current bug: might have duplicate with temp and in the grid
        Image currentProfile = tempProfilePic.GetComponent<Image>(); // might not need this line?

        tempProfilePic.sprite = newProfilePic.sprite;

        Texture2D texture = ImageToTexture2D(currentProfile);

        byte[] imageData = texture.EncodeToPNG();

        // Save the byte array as a cloud save file
        try
        {
            await CloudSaveService.Instance.Files.Player.SaveAsync("profileImage.png", imageData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save image as cloud save file: {e.Message}");
        }
    }*/

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
        try
        {
            // Load the cloud save file data
            byte[] imageData = await CloudSaveService.Instance.Files.Player.LoadBytesAsync("profileImage.png");

            // Convert the byte array to a Texture2D
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageData);

            // Set the loaded texture to the Image component
            tempProfilePic.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load image from cloud save file: {e.Message}");
        }
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

    private async void LoadProfilePicUnlockedStatus()
    {
        try
        {
            // Load the cloud save file data
            byte[] savedData = await CloudSaveService.Instance.Files.Player.LoadBytesAsync("profilePicStates.dat");

            // Deserialize the saved data back into a list of tuples
            List<Tuple<string, bool>> serializedData = ByteArrayToObject<List<Tuple<string, bool>>>(savedData);

            // Convert the deserialized data back into the dictionary format
            foreach (var item in serializedData)
            {
                ProfilePicData picData = Resources.Load<ProfilePicData>(item.Item1);
                profilePicStates[picData] = item.Item2;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load profile pic unlocked status from cloud save file: {e.Message}");
        }
    }

    private async void SaveProfilePicUnlockedStatus()
    {
        try
        {
            // Convert the dictionary into a list of tuples for serialization
            List<Tuple<string, bool>> serializedData = new List<Tuple<string, bool>>();
            foreach (var kvp in profilePicStates)
            {
                serializedData.Add(new Tuple<string, bool>(kvp.Key.name, kvp.Value));
            }

            // Serialize the data
            byte[] data = ObjectToByteArray(serializedData);

            // Save the serialized data to Unity Cloud Save
            await CloudSaveService.Instance.Files.Player.SaveAsync("profilePicStates.dat", data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save profile pic unlocked status to cloud save file: {e.Message}");
        }
    }

    // Convert object to byte array for serialization
    private byte[] ObjectToByteArray(object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    // Convert byte array to object for deserialization
    private T ByteArrayToObject<T>(byte[] bytes)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream(bytes))
        {
            return (T)bf.Deserialize(ms);
        }
    }
}

