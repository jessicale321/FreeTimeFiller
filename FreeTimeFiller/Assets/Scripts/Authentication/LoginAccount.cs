using System.Collections;
using System.Collections.Generic;
using TMPro;
// To use Unity's official account authentication service, go to Window < Package Manager < Unity Registry < then download and import Authentication into your project.
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;

public class LoginAccount : MonoBehaviour
{
    // create input fields to grab the username and password from user
    [SerializeField] private TMP_InputField loginUsername;
    [SerializeField] private TMP_InputField loginPassword;

    // text field that will be updated with information for the user if their account username or password are incorrect
    [SerializeField] private TMP_Text logMessage;

    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    /// <summary>
    /// Login() is called when the user presses the login button. It will grab the username and password
    /// that was typed and then call SignInWithUsernamePassowrdAsync() to talk to database
    /// </summary>
    public async void Login()
    {
        string username = loginUsername.text;
        string password = loginPassword.text;

        await SignInWithUsernamePasswordAsync(username, password);
    }

    /// <summary>
    /// Hardcoded credentials for quick login bc typing is annoying
    /// </summary>
    public async void QuickLoginA()
    {
        await SignInWithUsernamePasswordAsync("jessica3", "Password3!");
    }

    public async void QuickLoginB()
    {
        await SignInWithUsernamePasswordAsync("jessica4", "Password4!");
    }

    public async void QuickLoginC()
    {
        await SignInWithUsernamePasswordAsync("jessica5", "Password5!");
    }

    /// <summary>
    /// SignInWithUsernamePassword() 
    /// Attempts to loggin with username and password, notifies user if username and/or login were incorrect
    /// </summary>
    async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            await AuthenticationService.Instance.UpdatePlayerNameAsync(username);
            Debug.Log("SignIn is successful.");
            SceneManager.LoadScene(1);
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
            logMessage.text = "Username or Password was incorrect";
            logMessage.gameObject.SetActive(true);
        }
    }
}
