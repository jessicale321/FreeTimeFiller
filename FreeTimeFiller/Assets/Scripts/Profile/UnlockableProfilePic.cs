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
    public GameObject lockedMask;
    [SerializeField] private Button button;

    public ProfilePicData _myPicData;

    [SerializeField] private Image image;

    ///-///////////////////////////////////////////////////////////
    /// 
    private void Awake()
    {
        button.onClick.AddListener(OnProfilePicClicked);
    }

    ///-///////////////////////////////////////////////////////////
    ///
    public void OnProfilePicClicked()
    {
        ProfilePictureManager.instance.PicClicked(_myPicData);
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    public void SetProfilePictureData(ProfilePicData profilePicData)
    {
        _myPicData = profilePicData;
        image.sprite = profilePicData.sprite;
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    public void SetLockedMask(bool isUnlocked)
    {
        if (isUnlocked == false)
        {
            lockedMask.SetActive(true);
        }
        else
        {
            lockedMask.SetActive(false);
        }
    }
}
