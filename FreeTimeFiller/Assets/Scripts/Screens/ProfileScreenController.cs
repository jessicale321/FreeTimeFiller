using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

///-///////////////////////////////////////////////////////////
/// 
public class ProfileScreenController : MonoBehaviour
{
    [SerializeField] private Image profilePicture;

    ///-///////////////////////////////////////////////////////////
    /// 
    private void OnEnable()
    {
        LoadImageFromCloudSave();
    }

    ///-///////////////////////////////////////////////////////////
    /// Load the image from the cloud save file
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
            profilePicture.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load image from cloud save file: {e.Message}");
        }
    }
}
