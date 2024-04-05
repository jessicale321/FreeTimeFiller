using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async Task<Dictionary<string, string>> SearchUsers(string searchString)
    {
        Dictionary<string, string> results = new Dictionary<string, string>();

        // Query Firestore collection for usernames containing the search string
        QuerySnapshot querySnapshot = await db.Collection("user_data")
            .WhereArrayContains("username", searchString)
            .GetSnapshotAsync();

        // Iterate through the results and add username and userid pairs to the dictionary
        foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
        {
            Dictionary<string, object> userData = documentSnapshot.ToDictionary();
            if (userData.ContainsKey("username") && userData.ContainsKey("user_id"))
            {
                string username = userData["username"].ToString();
                string userId = userData["user_id"].ToString();
                results.Add(username, userId);
            }
        }

        return results;
    }
}
