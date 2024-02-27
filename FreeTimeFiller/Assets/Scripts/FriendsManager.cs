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

public class FriendsManager : MonoBehaviour
{
    public bool UseFriends = true;

    public static FriendsManager Active
    {
        get
        {
            if (internalActive == null)
                internalActive = FindObjectOfType<FriendsManager>();

            return internalActive; 
        }
    }

    private static FriendsManager internalActive;

    public TMP_InputField usernameInput;

    // callbacks for external scrip authentication
    public Action OnUserSignIn;
    // these action definitions might not work
    public Action<List<PlayerProfile>> OnRequestsRefresh; //{ get; internal set; }
    public Action<List<PlayerProfile>> OnFriendsRefresh;  //{ get; internal set; }

    private void Awake()
    {
        AuthenticationService.Instance.SignedIn += PlayerSignIn;
        if (UseFriends)
        {
            InitializeFriends();
        }
        PlayerSignIn();
    }
    // Start is called before the first frame update
  /*  private void Start()
    {
        if (UseFriends)
        {
            InitializeFriends();
        }

        PlayerSignIn();
    }*/

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

    public async void PlayerSignIn()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            OnUserSignIn?.Invoke();
        else
            Debug.Log("User was not signed in!");
    }

    public async void SendFriendRequest_ID(string memberID)
    {
        var relationship = await FriendsService.Instance.AddFriendAsync(memberID);

        Debug.Log($"Friend request send to {memberID}. New relationship is {relationship.Type}");
    }

    public async void AcceptRequest(string memberID)
    {
        Relationship relationship = await FriendsService.Instance.AddFriendAsync(memberID);

        Debug.Log($"Friend request accepted from {memberID}. New relationship status is {relationship.Type}");

        RefreshLists();
    }

    public async void DeleteFriend(string memberID)
    {
        await FriendsService.Instance.DeleteFriendAsync(memberID);

        RefreshLists();
    }

    // Friend Events

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
            Debug.Log(
                "An error occurred while performing teh action. Code: " + e.StatusCode + ", Message: " + e.Message);
        }
    }


    public void RefreshLists()
    {
        RefreshFriends();
        RefreshRequests();
    }

    public void RefreshRequests()
    {
        m_Requests.Clear();
        IReadOnlyList<Relationship> requests = FriendsService.Instance.IncomingFriendRequests;

        foreach (Relationship request in requests)
        {
            m_Requests.Add(new PlayerProfile(request.Member.Profile.Name, request.Member.Id));

        }
        // used auto-fix. might not work
        OnRequestsRefresh?.Invoke(m_Requests);
    }

    public void RefreshFriends()
    {
        m_Friends.Clear();
        IReadOnlyList<Relationship> friends = FriendsService.Instance.Friends;

        foreach(Relationship friend in friends)
        {
            m_Friends.Add(new PlayerProfile(friend.Member.Profile.Name, friend.Member.Id));
        }

        OnFriendsRefresh?.Invoke(m_Friends);
    }


    private IReadOnlyList<Relationship> friends;
    List<PlayerProfile> m_Requests = new List<PlayerProfile>();
    List<PlayerProfile> m_Friends = new List<PlayerProfile>();
}
