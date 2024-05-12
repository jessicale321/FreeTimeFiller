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
/// <summary>
/// How to set up Firebase with Unity
/// https://firebase.google.com/docs/unity/setup
/// Step 1: Create a Firebase project
/// 
/// Step 2: Register your app with Firebase
/// 1. Go to the Firebase console.
/// 2. In the center of the project overview page, click the Unity icon (plat_unity) to launch the setup workflow.If you've already added an app to your Firebase project, click Add app to display the platform options.
/// 3. Select which build target of your Unity project that you’d like to register, or you can even select to register both targets now at the same time.
/// 4. Enter your Unity project’s platform-specific ID(s).
///     For iOS — Enter your Unity project’s iOS ID in the iOS bundle ID field
///     For Android — Enter your Unity project’s Android ID in the Android package name field
///     The terms package name and application ID are often used interchangeably.
/// 5. (Optional) Enter your Unity project’s platform-specific nickname(s). These nicknames are internal, convenience identifiers and are only visible to you in the Firebase console.
/// 6. Click Register app.
/// 
/// Step 3: Add Firebase configuration files
/// 
/// 1. Obtain your platform-specific Firebase configuration file(s) in the Firebase console setup workflow.
/// 2. Open the Project window of your Unity project, then move your config file(s) into the Assets folder.
/// 3. Back in the Firebase console, in the setup workflow, click Next.
/// 
/// Step 4: Add Firebase Unity SDKs
/// 1. In the Firebase console, click Download Firebase Unity SDK, then unzip the SDK somewhere convenient.
/// 2. In your open Unity project, navigate to Assets > Import Package > Custom Package
/// 3. From the unzipped SDK, select the supported Firebase products that you want to use in your app.
/// 4. In the Import Unity Package window, click Import.
/// 5. Back in the Firebase console, in the setup workflow, click Next.
/// 
/// Step 5: Confirm Google Play services version requirements
/// </summary>

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

    ///-///////////////////////////////////////////////////////////
    /// Save an item to the userId under dataName (ex. saving a list of completed achievements to "completed_achievements)
    ///
    public async void SaveDataToUserId<T>(string userId, string dataName, T dataToSave)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                    .WhereEqualTo("user_id", userId)
                                    .GetSnapshotAsync();
        // Check if the query returned any documents
        if (snapshot != null && snapshot.Count > 0)
        {
            // Update the profile picture for the first user document found (assuming username is unique)
            DocumentReference userRef = snapshot.Documents.ElementAt(0).Reference;
            await userRef.UpdateAsync(dataName, dataToSave);
        }
        else
        {
            Debug.LogError("User not found: " + userId);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Save an item to the userId under dataName (ex. saving a list of completed achievements to "completed_achievements)
    ///
    public async void SaveDataToUsername<T>(string userName, string dataName, T dataToSave)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                    .WhereEqualTo("username", userName)
                                    .GetSnapshotAsync();
        // Check if the query returned any documents
        if (snapshot != null && snapshot.Count > 0)
        {
            // Update the profile picture for the first user document found (assuming username is unique)
            DocumentReference userRef = snapshot.Documents.ElementAt(0).Reference;
            await userRef.UpdateAsync(dataName, dataToSave);
        }
        else
        {
            Debug.LogError("User not found: " + userName);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Return a saved item from Firestore given a username. 
    ///
    public async Task<T> GetDataFromUserId<T>(string userId, string dataName)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                        .WhereEqualTo("user_id", userId)
                                        .GetSnapshotAsync();

        if (snapshot != null && snapshot.Count > 0)
        {
            DocumentSnapshot document = snapshot.Documents.ElementAt(0);

            // Check if the document contains the specified dataName field
            if (document.ContainsField(dataName))
            {
                T data = document.GetValue<T>(dataName);
                return data;
            }
            else
            {
                Debug.LogError($"Field '{dataName}' not found in document for user: {userId}");
                return default;
            }
        }
        else
        {
            Debug.LogError($"No document found for user: {userId}");
            return default;
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Return a saved item from Firestore given a username. 
    ///
    public async Task<T> GetDataFromUsername<T>(string userName, string dataName)
    {
        QuerySnapshot snapshot = await db.Collection("user_data")
                                        .WhereEqualTo("username", userName)
                                        .GetSnapshotAsync();

        if (snapshot != null && snapshot.Count > 0)
        {
            DocumentSnapshot document = snapshot.Documents.ElementAt(0);

            // Check if the document contains the specified dataName field
            if (document.ContainsField(dataName))
            {
                T data = document.GetValue<T>(dataName);
                return data;
            }
            else
            {
                Debug.LogError($"Field '{dataName}' not found in document for user: {userName}");
                return default;
            }
        }
        else
        {
            Debug.LogError($"No document found for user: {userName}");
            return default;
        }
    }
}