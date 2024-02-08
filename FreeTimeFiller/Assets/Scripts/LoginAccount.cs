using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System.Threading.Tasks;

public class LoginAccount : MonoBehaviour
{
    public TMP_InputField loginUsername;
    public TMP_InputField loginPassword;

    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    public async void Login()
    {
        string username = loginUsername.text;
        string password = loginPassword.text;

        await SignInWithUsernamePasswordAsync(username, password);
    }
    async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
}
