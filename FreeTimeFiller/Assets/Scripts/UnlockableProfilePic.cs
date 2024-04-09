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
    private string saveKey;

    public Action OnPictureClicked;

    // Start is called before the first frame update
    private void Awake()
    {
        saveKey = "locked_" + gameObject.name;

        image = gameObject.GetComponentInChildren<Image>();
        button = gameObject.GetComponentInChildren<Button>();

        button.onClick.AddListener(OnProfilePicClicked);
    }

    private void OnEnable()
    {
        LoadLockedStatus();
        Debug.Log("locked status: " + isLocked);
        if (isLocked)
        {
            image.color = new Color32(56, 56, 56, 80);
        }
        else
        {
            image.color = new Color32(255, 255, 255, 255);
        }
    }

    public void OnProfilePicClicked()
    {
        if (isLocked)
        {
            Debug.Log("locked");
            if (CoinManager.instance.coins >= costToUnlock)
            {
                isLocked = false;
                SaveLockedStatus();
                image.color = new Color32(255, 255, 255, 255);
                SelectProfileController.instance.SetProfilePic(image); // swap profile pic with the new one
                CoinManager.instance.SpendCoins(costToUnlock);
                isCurrentSelection = true;
            }
        }

        else
        {
            Debug.Log("actually not locked");
            SelectProfileController.instance.SetProfilePic(image);
            isCurrentSelection = true;
        }

    }

    private async void SaveLockedStatus()
    {
        var data = new Dictionary<string, object> { { saveKey, isLocked } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    private async void LoadLockedStatus()
    {
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { saveKey });
        if (data.TryGetValue(saveKey, out var lockedValue))
        {
            isLocked = lockedValue.Value.GetAs<bool>();
        }
    }
}
