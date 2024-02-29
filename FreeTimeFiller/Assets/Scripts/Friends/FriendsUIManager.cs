using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using JetBrains.Annotations;
using System;

namespace UI
{
    public class FriendsUIManager : MonoBehaviour
    {
        // Varibales to show user's name, id, and playerinfo
        // We shouldn't actually need playerinfo, i just kept it in case because he had it in the tutorial
        public TMP_Text playerUserName;
        public TMP_Text playerUserID;
        public TMP_Text playerInfo;
        // Friends loby is connnected to the canvas that shows the screen manager Jessica created
        public GameObject friendsLobby;
        // This is the search bar for who the friend request is being sent to
        [Header("From Control Area")] public TMP_InputField friendRecipientID;
        // NOT REAL PREFABS, these are GameObjects that are being created for each friend request or new friend 
        [Header("From Info Area")]
        public RequestObjects requestPrefab;
        public FriendObject friendPrefab;

        // Start is called before the first frame update
       /* private void Awake()
        {
            FriendsManager.Active.OnUserSignIn += OnUserSignIn;
        }*/

        private void Start()
        {
            // Game objects are hidden on start, this actually might be my issue with canvas not showing lol
            foreach (object o in transform)
            {
                (o as Transform).gameObject.SetActive(false);
            }
            // subscribing to event callbacks for each method in FriendsManager
            FriendsManager.Active.OnUserSignIn += OnUserSignIn;
            FriendsManager.Active.OnRequestsRefresh += OnRequestsRefresh;
            FriendsManager.Active.OnFriendsRefresh += OnFriendsRefresh;
        }

        // 
        private void OnUserSignIn()
        {
            // display user data after sign in 
            playerUserID.text = AuthenticationService.Instance.PlayerId;
            playerUserName.text = AuthenticationService.Instance.PlayerName;
            playerInfo.text = AuthenticationService.Instance.Profile;
            Debug.Log("We accessed the stuff");
            // we could probably attach this to just the start(), it all depends how we want the UI to interact
            friendsLobby.SetActive(true);
        }

        // Calls SendFriendRequest from FriendManager
        public void SendFriendRequest_ID()
        {
            FriendsManager.Active.SendFriendRequest_ID(friendRecipientID.text);
        }

        // Displays all new friend requests
        private void OnRequestsRefresh(List<PlayerProfile> requests)
        {
            // remove all former requests
            for (int i = 0; i < requestUIs.Count; i++)
            {
                Destroy(requestUIs[i].gameObject);
                requestUIs.RemoveAt(i);
            }

            // create a new request object for each request
            foreach (PlayerProfile req in requests)
            {
                // TODO: Make Request Objects appear underneath other objects intead of on top of each other
                RequestObjects newRequestItem = Instantiate(requestPrefab, requestPrefab.transform.parent);
                newRequestItem.gameObject.SetActive(true);

                // set the data for the new request CHANGED TO NAME
                newRequestItem.SetData(req.Name, OnRequestAccept);

                requestUIs.Add(newRequestItem);
            }

            Debug.Log("Updating List");
        }

        // Displays all Friends for friends list
        private void OnFriendsRefresh(List<PlayerProfile> friends)
        {
            // remove all former requests
            for (int i = 0; i < friendsUIs.Count; i++)
            {
                Destroy(friendsUIs[i].gameObject);
                friendsUIs.RemoveAt(i);
            }

            // create a new request object for each request
            foreach (PlayerProfile fr in friends)
            {
                FriendObject newFriendItem = Instantiate(friendPrefab, friendPrefab.transform.parent) as FriendObject;
                newFriendItem.gameObject.SetActive(true);

                // set the data for the request CHANGED TO NAME
                newFriendItem.SetData(fr.Id, fr.Name, OnDeleteFriend);

                friendsUIs.Add(newFriendItem);
            }
        }

        // Connected to button for requestPrefab
        private void OnRequestAccept(string id)
        {
            FriendsManager.Active.AcceptRequest(id);
        }

        // Connected to button for friendPrefab
        private void OnDeleteFriend(string id)
        {
            FriendsManager.Active.DeleteFriend(id);
        }

        List<RequestObjects> requestUIs = new List<RequestObjects>();
        List<FriendObject> friendsUIs = new List<FriendObject>();
    }
}
