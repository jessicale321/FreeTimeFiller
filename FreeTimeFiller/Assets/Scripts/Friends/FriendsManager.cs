using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using TMPro;
// To use Unity's official account authentication service, go to Window < Package Manager < Unity Registry < then download and import Authentication into your project.
using Unity.Services.Authentication;
using Unity.Services.Friends;
using Unity.Services.Friends.Models;
using Unity.Services.Friends.Exceptions;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using Firebase.Firestore;

public class FriendsManager : MonoBehaviour
{
    // allows each user to choose if they want friends
    public bool UseFriends = true;

    // Friends Mangaer Definition, this allows FriendsUIManager to subscribe to events
    public static FriendsManager Active
    {
        get
        {
            if (internalActive == null)
                internalActive = FindObjectOfType<FriendsManager>();

            return internalActive; 
        }
    }
    // create Friends Manager
    private static FriendsManager internalActive;
    // Friend search Username Input
    [SerializeField] private TMP_InputField usernameInput;

    // callbacks for external script authentication
    public Action OnUserSignIn;
    // Defining the action that will be called by UI manager for displaying friends and friend requests on refresh
    public Action<List<PlayerProfile>> OnRequestsRefresh; //{ get; internal set; }
    public Action<List<PlayerProfile>> OnFriendsRefresh;  //{ get; internal set; }
    public Action<List<PlayerProfile>> OnSearchRefresh;
    // create database object
    public UserDatabase userDatabase;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Attach the UserDatabase component to the new GameObject
        userDatabase = FindObjectOfType<UserDatabase>();

        if (userDatabase == null)
        {
            Debug.Log("UserDatabase not found in this scene");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // when the scene starts, the friends are initialized if the user wants them. PlayerSignIn() should be called when the scene
        // opens, but it doesn't right now. So PlayerSignIn() is attached to the home button
        if (UseFriends)
        {
            Debug.Log("Initializing Friends Service");
            InitializeFriends();
        }
        OnUserSignIn?.Invoke();
    }

    // Calls Unity's built in Friend system. Allowing users to have friends
    private async void InitializeFriends()
    {
        // initialize services
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);
        }

        if (!AuthenticationService.Instance.IsSignedIn)
            Debug.Log("Hey! User Not Signed In!");
        
        // initialize friends
        await FriendsService.Instance.InitializeAsync();

        // save the friends relation
        friends = FriendsService.Instance.Friends;

        SubscribeToFriendsEventCallbacks();
    }

    // Creates a relationship with the friend request sent to another user using memberID
    public async void SendFriendRequest_ID(string memberID)
    {
        // relationship will appear as "friendRequest" if other user has not sent a friend request
        // if other user already sent a friend request relationship will be "friend"
        try
        {
            Debug.Log("Sending Friend Request");
            await FriendsService.Instance.AddFriendAsync(memberID);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public async void SendFriendRequestButton(string username)
    {
        string userID = await userDatabase.GetUseridByUsername(username);
        Debug.LogFormat("Username: {0}, UserID: {1}", username, userID);
        SendFriendRequest_ID(userID);
    }

    public void SearchFriend(string username)
    {
        RefreshSearch();
    }

    // Accepting friend requests, same thing as SendFriendRequest but this will refresh lists to show 
    // friends instead of appearing in friend requests
    public async void AcceptRequest(string memberID)
    {
        Relationship relationship = await FriendsService.Instance.AddFriendAsync(memberID);

        Debug.Log($"Friend request accepted from {memberID}. New relationship status is {relationship.Type}");

        RefreshLists();
    }

    // delete friend relationship, refresh lists to delete
    public async void DeleteFriend(string memberID)
    {
        await FriendsService.Instance.DeleteFriendAsync(memberID);

        RefreshLists();
    }

    // Friend Events
    // These events will be called with each freind request/ accept and delete
    void SubscribeToFriendsEventCallbacks()
    {
        try
        {
            FriendsService.Instance.RelationshipAdded += e =>
            {
                Debug.Log($"create {e.Relationship} EventReceived");
                RefreshLists();
            };
            FriendsService.Instance.MessageReceived += enabled =>
            {
                Debug.Log("MessageReceived EventReceived");
            };
            FriendsService.Instance.PresenceUpdated += e =>
            {
                Debug.Log("PresenceUpdated EventReceived");
            };
            FriendsService.Instance.RelationshipDeleted += enabled =>
            {
                Debug.Log($"Delete {enabled.Relationship} EventReceived");
                RefreshLists();
            };
        }
        catch (FriendsServiceException e)
        {
            //Debug.Log("Hey we are here and something went wrong with friends");
            Debug.Log(
            "An error occurred while performing the action. Code: " + e.StatusCode + ", Message: " + e.Message);
        }
    }

    // refreshes the lists of user's friends and friend requests
    public void RefreshLists()
    {
        RefreshFriends();
        RefreshRequests();
    }

    // This refreshses the requests from other users
    public async void RefreshRequests()
    {
        // clears previous requests
        m_Requests.Clear();
        // get new requests
        IReadOnlyList<Relationship> requests = FriendsService.Instance.IncomingFriendRequests;

        // create PlayerProfiles for each friend request, easier for displaying in UI
        foreach (Relationship request in requests)
        {
            m_Requests.Add(new PlayerProfile(await userDatabase.GetUsernameByUserid(request.Member.Id), request.Member.Id));

        }
        // Show on UI
        OnRequestsRefresh?.Invoke(m_Requests);
    }

    // Refershes the friends list of user
    public async void RefreshFriends()
    {
        Debug.Log("Refreshing Friends");
        // clear previous list to not show old data
        m_Friends.Clear();
        // get friends list
        IReadOnlyList<Relationship> friends = FriendsService.Instance.Friends;

        // create PlayerProfiles for displaying
        foreach(Relationship friend in friends)
        {
            m_Friends.Add(new PlayerProfile(await userDatabase.GetUsernameByUserid(friend.Member.Id), friend.Member.Id));
        }

        // show on UI
        OnFriendsRefresh?.Invoke(m_Friends);
    }

    // This refreshses the search for other users
    public async void RefreshSearch()
    {
        // Clears previous searches
        m_Search.Clear();

        // originally written to take a list of users but now we are just doing one user that matches username

        m_Search.Add(await userDatabase.SearchUsers(usernameInput.text));


        Debug.Log("m_search contains: " + m_Search[0]);

        // Show on UI
        OnSearchRefresh?.Invoke(m_Search);
    }

    // friends list
    private IReadOnlyList<Relationship> friends;
    // playerprofiles for friends and requests for displaying UI
    List<PlayerProfile> m_Requests = new List<PlayerProfile>();
    List<PlayerProfile> m_Friends = new List<PlayerProfile>();
    List<PlayerProfile> m_Search = new List<PlayerProfile>();
}
