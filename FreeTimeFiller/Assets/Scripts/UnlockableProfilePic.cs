using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

public class UnlockableProfilePic : MonoBehaviour
{
    [SerializeField] private bool isLocked = true;
    private int costToUnlock = 100;
    private Button button;
    private Image image;
    private bool isCurrentSelection;

    public Action OnPictureClicked;

    // Start is called before the first frame update
    private void Awake()
    {
        image = gameObject.GetComponentInChildren<Image>();
        button = gameObject.GetComponentInChildren<Button>();

        button.onClick.AddListener(OnProfilePicClicked);
    }

    private void OnEnable()
    {
        if (isLocked)
        {
            image.color = new Color32(56, 56, 56, 80);
        }
        else
        {
            image.color = new Color32(255, 255, 255, 255);
        }
    }

    private void OnProfilePicClicked()
    {
        if (isLocked)
        {
            if (CoinManager.instance.coins >= costToUnlock)
            {
                isLocked = false;
                SaveLockedStatus();
                image.color = new Color32(255, 255, 255, 255);
                SelectProfileController.instance.SetProfilePic(image); // swap profile pic with the new one
                CoinManager.instance.SpendCoins(costToUnlock);
                isCurrentSelection = true;
                // todo: save coins and unlocked pic
            }
        }

        else
        {
            SelectProfileController.instance.SetProfilePic(image);
            isCurrentSelection = true;
        }

    }

    private async void SaveLockedStatus()
    {
        var data = new Dictionary<string, object> { { "locked", isLocked } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }
}
