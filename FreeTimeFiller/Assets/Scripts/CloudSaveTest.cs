using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.UI;
using TMPro;

public class CloudSaveTest : MonoBehaviour
{
    public static TextAlignment status;
    public TMP_InputField inpf;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void SaveData()
    {
        var data = new Dictionary<string, object> { { "firstData", inpf.text } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

}
