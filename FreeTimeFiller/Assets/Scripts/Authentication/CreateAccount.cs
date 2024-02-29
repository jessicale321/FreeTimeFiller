using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Services.CloudSave;

public class TestScript : MonoBehaviour
{
    // Create fields to grab username, password, and confirmed password for registration
    [SerializeField] private TMP_InputField registerUsername;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private TMP_InputField registerConfirmedPassword;

    [SerializeField] private TMP_Text logMessage;
    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    /// <summary>
    /// CreateAccount() Will be activated when the user presses the create account button.
    /// It will take the data typed by the user, make sure the passwords match, and call 
    /// SignUpWithUsernamePassword
    /// </summary>
    public async void CreateAccount()
    {
        string username = registerUsername.text;
        string password = registerPassword.text;
        string confirmedPassword = registerConfirmedPassword.text;

        if (password == confirmedPassword)
        {
            await SignUpWithUsernamePassword(username, confirmedPassword);
        }
        else
        {
            Debug.Log("Passwords do not match.");
        }
    }

    /// <summary>
    /// SignUpWithUsernamePassword() takes the username and password typed by the user and attempts to create an account
    /// in the database with that data. If the password is invalid it will tell the user how to create a password that 
    /// fits the requirements. It the user is already logged in it will not create an account and tell the user they are 
    /// already logged in.
    /// </summary>
    public async Task SignUpWithUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            await AuthenticationService.Instance.UpdatePlayerNameAsync(username);
            SaveCredentials();
            Debug.Log("SignUp is successful.");
            SceneManager.LoadScene(1);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);

            logMessage.text = ex.ToString();
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);

            logMessage.text = "Please include 1 uppercase, 1 lowercase, 1 digit and 1 symbol in password with min 8 characters and max of 30";
            logMessage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Saves the user's credentials when a new account is made. Will be displayed on Unity dashboard under their ID > Cloud Save > Data
    /// </summary>
    public async void SaveCredentials()
    {
        var credentials = new Dictionary<string, object>
        {
            {"username", registerUsername.text },
            {"password", registerPassword.text }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(credentials);
    }
}
