using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class UserDatabase : MonoBehaviour
{
    private FirebaseFirestore db;


    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirestore();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
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
}