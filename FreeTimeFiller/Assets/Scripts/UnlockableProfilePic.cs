using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockableProfilePic : MonoBehaviour
{
    private bool isLocked = true;
    public int costToUnlock { get; private set; }
    private Button button;
    private Image image;

    public Action OnPictureClicked;

    // Start is called before the first frame update
    private void Awake()
    {
        image = gameObject.GetComponentInChildren<Image>();
        button = gameObject.GetComponentInChildren<Button>();

        button.onClick.AddListener(PurchasePic);
    }

    private void OnEnable()
    {
        if (isLocked)
        {
            image.color = new Color32(56, 56, 56, 255);
        }
    }

    private void PurchasePic()
    {
        if (CoinManager.instance.coins >= costToUnlock)
        {
            isLocked = false;
            image.color = new Color32(255, 255, 255, 255);
            ProfileSelectController.instance.SetProfilePic(image); // swap profile pic with the new one
            CoinManager.instance.coins -= costToUnlock;
            // todo: save coins and unlocked pic
        }

    }
}
