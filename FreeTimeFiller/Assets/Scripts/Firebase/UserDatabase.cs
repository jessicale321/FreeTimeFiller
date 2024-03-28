using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class UserDatabase : MonoBehaviour
{
    private FirebaseFirestore db;


    private void Start()
    {
        // Initialize Firestore instance
        db = FirebaseFirestore.DefaultInstance;

        //AddUser("username", "idtestexample");
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
        db.Collection("user_data").Document("users").SetAsync(userData)
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
}
