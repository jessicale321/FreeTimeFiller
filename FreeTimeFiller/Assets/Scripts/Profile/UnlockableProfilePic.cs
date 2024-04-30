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
    //[SerializeField] private bool isLocked;
    public GameObject lockedMask;
    [SerializeField] private Button button;

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
        //Debug.Log("clicked");
        Image image = this.gameObject.GetComponentInChildren<Image>();
        SelectProfileController.instance.PicClicked(image);
    }
}
