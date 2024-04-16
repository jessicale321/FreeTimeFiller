using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

///-///////////////////////////////////////////////////////////
/// 
public class SelectProfileController : MonoBehaviour
{
    public static SelectProfileController instance { get; private set; }

    [SerializeField] private Image tempProfilePic;
    private Image[] profilePics;

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
        LoadImageFromCloudSave(); // use Russell's data manager?
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    public async void SetProfilePic(Image newProfilePic)
    {
        // current bug: might have duplicate with temp and in the grid
        Image currentProfile = tempProfilePic.GetComponent<Image>(); // might not need this line?

        //var tempPicToSwap = tempProfilePic.sprite;
        tempProfilePic.sprite = newProfilePic.sprite;
        //newProfilePic.sprite = tempPicToSwap;

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
    public async void LoadImageFromCloudSave()
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
}
