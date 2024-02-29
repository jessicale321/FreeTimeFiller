using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
    [SerializeField] private int coins;
    [SerializeField] private GameObject profilePic;

    public void PurchaseProfilePic(int cost)
    {
        if (coins >= cost)
        {
            // unlock profile pic
            coins -= cost;
        }
    }

    public void SetProfilePic(GameObject _profilePic)
    {
        // todo: swap current and new profile pic in grid
        Image currentProfile = profilePic.GetComponent<Image>();
        Image newProfileImage = _profilePic.GetComponent<Image>();
        currentProfile.sprite = newProfileImage.sprite;
    }
}
