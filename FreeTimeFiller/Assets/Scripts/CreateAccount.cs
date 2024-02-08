using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;

public class TestScript : MonoBehaviour
{
    public TMP_InputField registerUsername;
    public TMP_InputField registerPassword;
    public TMP_InputField registerConfirmedPassword;

    [SerializeField] private TMP_Text logMessage;
    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

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
    public async Task SignUpWithUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
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

            logMessage.text = "Password does not match requirements. Insert at least 1 uppercase, " +
                "1 lowercase, 1 digit and 1 symbol. With minimum 8 characters and a maximum of 30";
        }
    }
}
