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

public class UserDatabase : MonoBehaviour
{
    private FirebaseFirestore db;

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

    // NOOO FIRESTORE
    void InitializeFirestore()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    /*void InitializeDatabase()
    {
        // Get the root reference location of the database
        FirebaseDatabase database = FirebaseDatabase.DefaultInstance;
        if (database == null)
        {
            Debug.Log("Failed to get Firebase Realtime Database instance.");
            return;
        }

        databaseReference = database.RootReference;

        if (databaseReference == null)
        {
            Debug.Log("Failed to get database reference.");
        }
        else
        {
            Debug.Log("Firebase Realtime Database initialized successfully.");
        }
    }*/

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

    /*public void AddUser(string username, string userId)
    {
        if (databaseReference == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }
        // Create a new user object with the provided user ID and username
        // Create a dictionary to hold the user data
        Debug.Log("Adding user to database");
        Dictionary<string, object> userData = new Dictionary<string, object>();
        userData["username"] = username;
        userData["user_id"] = userId;

        // Add the user data to the "users" node in the database with the user ID as the key
        databaseReference.Child("users").Child(userId).SetValueAsync(userData)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error adding user to database: " + task.Exception);
                }
                else
                {
                    Debug.Log("User added successfully to database");
                }
            });
    }*/

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

    /*public void SearchUsers(string substring, Action<List<PlayerProfile>> onComplete)
    {
        if (databaseReference == null)
        {
            Debug.Log("Database reference is not initialized.");
            return;
        }
        databaseReference.Child("users").OrderByChild("username").StartAt(substring).EndAt(substring + "\uf8ff").GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("Error searching users by substring: " + task.Exception);
                    onComplete?.Invoke(new List<PlayerProfile>());
                    return;
                }

                DataSnapshot snapshot = task.Result;
                List<PlayerProfile> foundUsernames = new List<PlayerProfile>();
                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    string username = userSnapshot.Child("username").GetValue(true).ToString();
                    string userId = userSnapshot.Child("user_id").GetValue(true).ToString();
                    foundUsernames.Add(new PlayerProfile(username, userId));
                }

                onComplete?.Invoke(foundUsernames);
            });
    }*/

    /*public void CheckUsernameAvailability(string username, System.Action<bool> callback)
    {
        if (databaseReference == null)
        {
            Debug.Log("Database reference is not initialized.");
            return;
        }
        Debug.Log("Checking username availability");
        databaseReference.Child("users").OrderByChild("username").EqualTo(username).GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("Error checking username availability: " + task.Exception);
                    callback?.Invoke(false); // Assume username is not available on error
                    return;
                }
                Debug.Log("Found no issues: Sign in allowed");
                DataSnapshot snapshot = task.Result;
                bool isAvailable = snapshot.ChildrenCount == 0; // Check if no documents returned
                callback?.Invoke(isAvailable);
            });
    }*/

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
}