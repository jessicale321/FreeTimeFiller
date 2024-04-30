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

    private ProfilePicData _myPicData;

    [SerializeField] private Image image;

    public Action OnPictureClicked;

    ///-///////////////////////////////////////////////////////////
    /// 
    private void Awake()
    {
        button.onClick.AddListener(OnProfilePicClicked);
    }

    ///-///////////////////////////////////////////////////////////
    /// Checks if picture is currently locked and behaves accordingly 
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
}
