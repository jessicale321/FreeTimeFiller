using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

///-///////////////////////////////////////////////////////////
/// 
public class UnlockableProfilePic : MonoBehaviour
{
    [SerializeField] private bool isLocked;
    [SerializeField] private GameObject lockedMask;
    private int costToUnlock = 100;
    private Button button;
    private Image image;
    private bool isCurrentSelection;
    private string saveKey;

    public Action OnPictureClicked;

    ///-///////////////////////////////////////////////////////////
    /// 
    private void Awake()
    {
        string uniqueId = System.Guid.NewGuid().ToString();
        saveKey = "locked_" + uniqueId;

        image = gameObject.GetComponentInChildren<Image>();
        button = gameObject.GetComponentInChildren<Button>();

        button.onClick.AddListener(OnProfilePicClicked);
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    private void Start()
    {
        LoadLockedStatus();
        Debug.Log(this.gameObject.name + " is locked: " + isLocked);
        if (isLocked)
        {
            lockedMask.SetActive(true);
        }
        else
        {
            lockedMask.SetActive(false);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    public void OnProfilePicClicked()
    {
        if (isLocked)
        {
            Debug.Log("locked");
            if (CoinManager.instance.coins >= costToUnlock)
            {
                isLocked = false;
                SaveLockedStatus();
                lockedMask.SetActive(false);
                SelectProfileController.instance.SetProfilePic(image); 
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

    ///-///////////////////////////////////////////////////////////
    /// 
    private async void SaveLockedStatus()
    {
        var data = new Dictionary<string, object> { { saveKey, isLocked } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    private async void LoadLockedStatus()
    {
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { saveKey });
        if (data.TryGetValue(saveKey, out var lockedValue))
        {
            isLocked = lockedValue.Value.GetAs<bool>();
        }
    }
}
