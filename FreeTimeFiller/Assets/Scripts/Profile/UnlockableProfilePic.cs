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
        // CURRENT BUG: new GUID savekey being generated each time, so nothing actually saves
        // possible solution: player saves array/list of profile pics they have unlocked, on load: read from the list and update locked status accordingly

        //string uniqueId = System.Guid.NewGuid().ToString();
        //saveKey = "locked_" + uniqueId;
        //Debug.Log("keY: " + saveKey);

        // Generate or retrieve the save key
        saveKey = PlayerPrefs.GetString("SaveKey_" + gameObject.GetInstanceID(), "");

        // If the save key is empty, generate a new one
        if (string.IsNullOrEmpty(saveKey))
        {
            saveKey = "locked_" + System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("SaveKey_" + gameObject.GetInstanceID(), saveKey);
            PlayerPrefs.Save();
        }

        Debug.Log("Save Key for " + name + ": " + saveKey);

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
    }

    ///-///////////////////////////////////////////////////////////
    /// Checks if picture is currently locked and behaves accordingly 
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

        if (isLocked)
        {
            lockedMask.SetActive(true);
        }
        else
        {
            lockedMask.SetActive(false);
        }
    }
}
