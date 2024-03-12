using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

public class ProfileScreenController : MenuScreen
{
    [SerializeField] private GameObject profilePicture;
    [SerializeField] private Profile userProfile; // reference this better

    private void OnEnable()
    {
        LoadProfilePic();
        userProfile.OnProfilePicUpdated += GetNewProfilePic;
    }

    private async void GetNewProfilePic()
    {
        var loadedProfilePic = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "profilePic" });
        Image currentProfile = profilePicture.GetComponent<Image>();
        if (loadedProfilePic.TryGetValue("profilePic", out var profilePic))
        {
            /*            var loadedSprite = profilePic.Value.GetAs<Sprite>();
                        currentProfile.sprite = loadedSprite;*/
            Texture2D loadedTexture = profilePic.Value.GetAs<Texture2D>();
            Sprite loadedSprite = Sprite.Create(loadedTexture, new Rect(0, 0, loadedTexture.width, loadedTexture.height), Vector2.zero);
            currentProfile.sprite = loadedSprite;
        }

        /*var data = new Dictionary<string, object> { { "mainProfilePic", currentProfile.sprite.texture } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);*/
        Texture2D profileTexture = currentProfile.sprite.texture;
        byte[] imageData = profileTexture.EncodeToPNG(); // Encode the texture as PNG image data

        var data = new Dictionary<string, object> { { "mainProfilePic", imageData } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    private async void LoadProfilePic()
    {
        var loadedProfilePic = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "mainProfilePic" });
        Image currentProfile = profilePicture.GetComponent<Image>();
        if (loadedProfilePic.TryGetValue("mainProfilePic", out var mainProfilePic))
        {
            byte[] imageData = mainProfilePic.Value.GetAs<byte[]>();
            Texture2D loadedTexture = new Texture2D(1, 1); // Create a new texture
            loadedTexture.LoadImage(imageData); // Load the image data into the texture
            Sprite loadedSprite = Sprite.Create(loadedTexture, new Rect(0, 0, loadedTexture.width, loadedTexture.height), Vector2.zero);
            //Image currentProfile = profilePicture.GetComponent<Image>();
            currentProfile.sprite = loadedSprite;
        }
    }

    private async void SaveProfilePic()
    {
/*        var data = new Dictionary<string, object> { { "profilePic", currentProfile.sprite } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);*/
    }
}
