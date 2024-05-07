using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UserDatabase : MonoBehaviour
{
    public static UserDatabase Instance { get; private set; }

    private FirebaseFirestore db;

    ///-///////////////////////////////////////////////////////////
    /// 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    //DatabaseReference databaseReference;
    private void Start()
    {
        Debug.Log("Trying to initialize database.");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                InitializeFirestore();
            }
            else
            {
                Debug.Log("Failed to initialize Firebase: " + task.Exception);
            }
        });
    }

    void InitializeFirestore()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void AddUser(string username, string userId)
    {
        // Create a dictionary with user data
        var userData = new Dictionary<string, object>
        {
            { "username", username },
            { "user_id", userId }
        };

        // Add user document to Firestore
        db.Collection("user_data").AddAsync(userData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("User document added successfully.");
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("Error adding user document: " + task.Exception);
                }
            });
    }

    /// SearchUsers() Searches the database for a specific user
    /// 
    /// </summary>
    /// <param name="username"></param> username that will be searched
    /// <returns></returns> A Task<PlayerProfile> that will either contain a player's profile or be null
    public async Task<PlayerProfile> SearchUsers(string username)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                    .WhereEqualTo("username", username)
                                    .GetSnapshotAsync();

        if (snapshot != null)
        {
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                string name = document.GetValue<string>("username");
                string id = document.GetValue<string>("user_id");
                return new PlayerProfile(name, id);
            }
        }
        // User not found
        return null;
    }

    /// </summary>
    /// CheckUsernameAvailability() queries firestore database and returns isAvailable back to Create Account
    /// If there are no conflicting usernames. 
    /// If that username is already in use then don't allow the user to create an account
    /// <param name="username"></param> user's username
    /// <param name="callback"></param> invokes false or true for CreateAccount if statement
    public void CheckUsernameAvailability(string username, System.Action<bool> callback)
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

    public async Task<string> GetUseridByUsername(string username)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                        .WhereEqualTo("username", username)
                                        .GetSnapshotAsync();
        Debug.LogFormat("The username we are looking for: {0}", username);

        if (snapshot != null)
        {
            DocumentSnapshot document = snapshot.Documents.ElementAt(0);
            return document.GetValue<string>("user_id");
        }
        else { return null; }
    }

    public async Task<string> GetUsernameByUserid(string userID)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                        .WhereEqualTo("user_id", userID)
                                        .GetSnapshotAsync();

        if (snapshot != null)
        {
            DocumentSnapshot document = snapshot.Documents.ElementAt(0);
            return document.GetValue<string>("username");
        }
        else { return null; }
    }

    public async void AddProfilePicture(string userId, string pictureName)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                    .WhereEqualTo("user_id", userId)
                                    .GetSnapshotAsync();
        // Check if the query returned any documents
        if (snapshot != null && snapshot.Count > 0)
        {
            // Update the profile picture for the first user document found (assuming username is unique)
            DocumentReference userRef = snapshot.Documents.ElementAt(0).Reference;
            await userRef.UpdateAsync("current_profile_picture_name", pictureName);
            Debug.Log("Profile picture updated for user: " + userId);
        }
        else
        {
            Debug.LogError("User not found: " + userId);
        }
    }
    public async Task<string> GetProfilePicture(string userName)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                        .WhereEqualTo("username", userName)
                                        .GetSnapshotAsync();

        if (snapshot != null)
        {
            DocumentSnapshot document = snapshot.Documents.ElementAt(0);
            return document.GetValue<string>("current_profile_picture_name");
        }
        else { return null; }
    }
}