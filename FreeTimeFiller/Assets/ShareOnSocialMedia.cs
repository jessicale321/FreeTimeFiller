using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ShareOnSocialMedia : MonoBehaviour
{
    [SerializeField] private string pathName;
    [SerializeField] private string subject;
    [SerializeField] private string description;

    [SerializeField] private Button shareButton;
    
    private bool _isSharing;

    private void OnEnable()
    {
        shareButton.onClick.AddListener(ShareOnSocial);
    }

    private void OnDisable()
    {
        shareButton.onClick.RemoveListener(ShareOnSocial);
    }

    private void ShareOnSocial()
    {
        if(!_isSharing)
            StartCoroutine(TakeScreenShotAndShare());
    }

    ///-///////////////////////////////////////////////////////////
    /// When the share button is clicked, take a screenshot of the screen and share that on social media.
    /// 
    private IEnumerator TakeScreenShotAndShare()
    {
        _isSharing = true;
        
        // Hide share button before screenshotting
        shareButton.gameObject.SetActive(false);
        
        yield return new WaitForEndOfFrame();
        
        // Texture of the entire screen
        Texture2D tx = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tx.Apply();

        // Image name
        string path = Path.Combine(Application.temporaryCachePath, $"{pathName}.png");
        File.WriteAllBytes(path, tx.EncodeToPNG());
        
        Destroy(tx);

        new NativeShare()
            .AddFile(path)
            .SetSubject($"{subject}")
            .SetText($"{description}")
            .Share();

        shareButton.gameObject.SetActive(true);
        _isSharing = false;
    }
}
