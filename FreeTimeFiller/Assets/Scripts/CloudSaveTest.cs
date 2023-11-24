using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;
using System.Linq;

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

    public async void LoadData()
    {
        Dictionary<string, string> serverData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "firstData" });

        if (serverData.ContainsKey("firstData"))
        {
            inpf.text = serverData["firstData"];
        }
        else
        {
            Debug.Log("Key not found");
        }
    }
}
