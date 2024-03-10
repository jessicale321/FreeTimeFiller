using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Friends;
using Unity.Services.Friends.Models;
using Unity.Services.Friends.Exceptions;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

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
    // This is unused at the moment. It was used in the tutorial for the person to log in
    public TMP_InputField usernameInput;

    // callbacks for external script authentication
    public Action OnUserSignIn;
    // Defining the action that will be called by UI manager for displaying friends and friend requests on refresh
    public Action<List<PlayerProfile>> OnRequestsRefresh; //{ get; internal set; }
    public Action<List<PlayerProfile>> OnFriendsRefresh;  //{ get; internal set; }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // when the scene starts, the friends are initialized if the user wants them. PlayerSignIn() should be called when the scene
        // opens, but it doesn't right now. So PlayerSignIn() is attached to the home button
        if (UseFriends)
        {
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

    // If the player signs in, call the OnUserSign in from UI manager, displaying what's needed. No longer needed
  /*  public async void PlayerSignIn()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            OnUserSignIn?.Invoke();
        else
            Debug.Log("User was not signed in!");
    }*/

    // Creates a relationship with the friend request sent to another user using memberID
    public async void SendFriendRequest_ID(string memberID)
    {
        // relationship will appear as "friendRequest" if other user has not sent a friend request
        // if other user already sent a friend request relationship will be "friend"
        try
        {
            await FriendsService.Instance.AddFriendByNameAsync(memberID);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    // Accepting friend requests, same thing as SendFriendRequest but this will refresh lists to show 
    // friends instead of appearing in friend requests
    public async void AcceptRequest(string memberID)
    {
        Relationship relationship = await FriendsService.Instance.AddFriendByNameAsync(memberID);

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
            Debug.Log("Hey we are here and something went wrong with friends");
            //Debug.Log(
            //"An error occurred while performing the action. Code: " + e.StatusCode + ", Message: " + e.Message);
        }
    }

    // refreshes the lists of user's friends and friend requests
    public void RefreshLists()
    {
        RefreshFriends();
        RefreshRequests();
    }

    // This refreshses the requests from other users
    public void RefreshRequests()
    {
        // clears previous requests
        m_Requests.Clear();
        // get new requests
        IReadOnlyList<Relationship> requests = FriendsService.Instance.IncomingFriendRequests;

        // create PlayerProfiles for each friend request, easier for displaying in UI
        foreach (Relationship request in requests)
        {
            m_Requests.Add(new PlayerProfile(request.Member.Profile.Name, request.Member.Id));

        }
        // Show on UI
        OnRequestsRefresh?.Invoke(m_Requests);
    }

    // Refershes the friends list of user
    public void RefreshFriends()
    {
        // clear previous list to not show old data
        m_Friends.Clear();
        // get friends list
        IReadOnlyList<Relationship> friends = FriendsService.Instance.Friends;

        // create PlayerProfiles for displaying
        foreach(Relationship friend in friends)
        {
            m_Friends.Add(new PlayerProfile(friend.Member.Profile.Name, friend.Member.Id));
        }

        // show on UI
        OnFriendsRefresh?.Invoke(m_Friends);
    }

    // friends list
    private IReadOnlyList<Relationship> friends;
    // playerprofiles for friends and requests for displaying UI
    List<PlayerProfile> m_Requests = new List<PlayerProfile>();
    List<PlayerProfile> m_Friends = new List<PlayerProfile>();
}
