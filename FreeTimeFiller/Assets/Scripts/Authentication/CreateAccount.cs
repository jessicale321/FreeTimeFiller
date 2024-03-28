using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;

public class TestScript : MonoBehaviour
{
    // Create fields to grab username, password, and confirmed password for registration
    [SerializeField] private TMP_InputField registerUsername;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private TMP_InputField registerConfirmedPassword;

    [SerializeField] private TMP_Text logMessage;

    public UserDatabase userDatabase;
    async void Start()
    {
        await UnityServices.InitializeAsync();

        userDatabase = FindObjectOfType<UserDatabase>();
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

        if (userDatabase != null)
        {
            // Check if username is available
            CheckUsernameAvailability(username, async (isAvailable) =>
            {
                if (isAvailable)
                {
                    if (password == confirmedPassword)
                    {
                        await SignUpWithUsernamePassword(username, confirmedPassword);
                    }
                    else
                    {
                        Debug.Log("Passwords do not match.");
                    }
                }
                else
                {
                    Debug.Log("Username is already taken. Please choose a different username.");
                    // Notify the user that the username is unavailable
                }
            });
        }
        else
        {
            Debug.Log("FirestoreWriter not found!");
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
            // add username and userid pair to database
            userDatabase.AddUser(username, AuthenticationService.Instance.PlayerId);
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

    /// </summary>
    /// CheckUsernameAvailability() queries firestore database and returns isAvailable back to Create Account
    /// If there are no conflicting usernames. 
    /// If that username is already in use then don't allow the user to create an account
    /// <param name="username"></param> user's username
    /// <param name="callback"></param> invokes false or true for CreateAccount if statement
    private void CheckUsernameAvailability(string username, System.Action<bool> callback)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        // Query Firestore to check if username exists
        db.Collection("user_data").WhereEqualTo("username", username).GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    QuerySnapshot snapshot = task.Result;
                    bool isAvailable = snapshot.Documents.Count() == 0; // Check if no documents returned
                    callback?.Invoke(isAvailable);
                }
                else
                {
                    Debug.Log("Error checking username availability: " + task.Exception);
                    callback?.Invoke(false); // Assume username is not available on error
                }
            });
    }
}
