using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
    [SerializeField] private int coins;
    [SerializeField] private GameObject profilePic;
    public Action OnProfilePicUpdated;

    public void PurchaseProfilePic(int cost)
    {
        if (coins >= cost)
        {
            // unlock profile pic
            coins -= cost;
        }
    }

    public async void SetProfilePic(GameObject _profilePic)
    {
        // todo: swap current and new profile pic in grid
        Image currentProfile = profilePic.GetComponent<Image>();
        Image newProfileImage = _profilePic.GetComponent<Image>();

        currentProfile.sprite = newProfileImage.sprite;

        /*        var data = new Dictionary<string, object> { { "profilePic", currentProfile.sprite} };
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);*/
        Texture2D profileTexture = currentProfile.sprite.texture;
        byte[] imageData = profileTexture.EncodeToPNG(); // Encode the texture as PNG image data

        var data = new Dictionary<string, object> { { "profilePic", imageData } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);

        OnProfilePicUpdated?.Invoke();
    }

    public async void SaveProfilePic()
    {
/*        var data = new Dictionary<string, object> { { "profilePic",  } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);*/
    }
}
