using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
// To use Unity's official account authentication service, go to Window < Package Manager < Unity Registry < then download and import Authentication into your project.
using Unity.Services.Authentication;
using UnityEngine.UI;
using JetBrains.Annotations;
using System;
using UnityEngine.SocialPlatforms;

namespace UI
{
    public class FriendsUIManager : MonoBehaviour
    {
        // Varibales to show user's name, id, and playerinfo
        // We shouldn't actually need playerinfo, i just kept it in case because he had it in the tutorial
        [SerializeField] private TMP_Text playerUserName;
        [SerializeField] private TMP_Text playerUserID;
        [SerializeField] private TMP_Text playerInfo;
        // Player username of profile being viewed
        [SerializeField] private TMP_Text profileViewUsername;
        [Header("Send Friend Request")]
        [SerializeField] private Button sendFriendRequestButton;
        // This is the search bar for who the friend request is being sent to
        [Header("From Control Area")]
        [SerializeField] private TMP_InputField friendRecipientID;
        [SerializeField] private TMP_InputField friendUsername;
        // NOT REAL PREFABS, these are GameObjects that are being created for each friend request or new friend 
        [Header("From Info Area")]
        [SerializeField] private RequestObjects requestPrefab;
        [SerializeField] private FriendObject friendPrefab;
        [SerializeField] private SearchObject searchPrefab;
        [SerializeField] private SearchObject userNotFoundPrefab;

        [Header("Required Scripts")]
        [SerializeField] private FriendProfileScreenController friendProfileScreenController;

        // Start is called before the first frame update
        /* private void Awake()
         {
             FriendsManager.Active.OnUserSignIn += OnUserSignIn;
         }*/

        private void OnEnable()
        {
            // subscribing to event callbacks for each method in FriendsManager
            FriendsManager.Active.OnUserSignIn += OnUserSignIn;
            FriendsManager.Active.OnRequestsRefresh += OnRequestsRefresh;
            FriendsManager.Active.OnFriendsRefresh += OnFriendsRefresh;
            FriendsManager.Active.OnSearchRefresh += OnSearchRefresh;
        }

        // 
        private void OnUserSignIn()
        {
            // display user data after sign in 
            playerUserID.text = AuthenticationService.Instance.PlayerId;
            playerUserName.text = AuthenticationService.Instance.PlayerName;
            playerInfo.text = AuthenticationService.Instance.Profile;
            Debug.Log("We accessed the stuff");
        }

        // Calls SendFriendRequest from FriendManager
        public void SendFriendRequest_ID()
        {
            FriendsManager.Active.SendFriendRequest_ID(friendRecipientID.text);
        }

        public void SendFriendRequestButton()
        {
            FriendsManager.Active.SendFriendRequestButton(profileViewUsername.text);
        }

        public void SearchFriendName()
        {
            FriendsManager.Active.SearchFriend(friendUsername.text);
        }


        // Displays all new friend requests
        public void OnRequestsRefresh(List<PlayerProfile> requests)
        {
            // remove all former requests
            /*for (int i = 0; i < requestUIs.Count; i++)
            {
                Destroy(requestUIs[i].gameObject);
                requestUIs.RemoveAt(i);
            }*/
            int i = 0;
            Transform panelRequest = GameObject.Find("DisplayPanelRequest").transform;
            foreach (Transform child in panelRequest)
            {
                if (i == 0)
                {
                    i++;
                }
                else
                {
                    Destroy(child.gameObject);
                }
            }

            // create a new request object for each request
            foreach (PlayerProfile req in requests)
            {
                // TODO: Make Request Objects appear underneath other objects intead of on top of each other
                RequestObjects newRequestItem = Instantiate(requestPrefab, requestPrefab.transform.parent);
                newRequestItem.gameObject.SetActive(true);

                // set the data for the new request CHANGED TO NAME
                newRequestItem.SetData(req, OnRequestAccept);

                requestUIs.Add(newRequestItem);
            }

            Debug.Log("Updating List");
        }

        // Displays all Friends for friends list
        public void OnFriendsRefresh(List<PlayerProfile> friends)
        {
            // remove all former requests
            /*for (int i = 0; i < friendsUIs.Count; i++)
            {
                Destroy(friendsUIs[i].gameObject);
                friendsUIs.RemoveAt(i);
            }*/
            int i = 0; 
            Transform panelFriend = GameObject.Find("DisplayPanelFriend").transform;
            foreach (Transform child in panelFriend)
            {
                if (i == 0)
                {
                    i++;
                }
                else
                {
                    Destroy(child.gameObject);
                }
            }
            // create a new request object for each request
            foreach (PlayerProfile fr in friends)
            {
                FriendObject newFriendItem = Instantiate(friendPrefab, friendPrefab.transform.parent) as FriendObject;
                newFriendItem.gameObject.SetActive(true);

                // set the data for the request CHANGED TO NAME
                newFriendItem.SetData(fr.Id, fr.Name, OnDeleteFriend, OnViewProfile);

                friendsUIs.Add(newFriendItem);
            }
        }

        public void OnSearchRefresh(List<PlayerProfile> friends)
        {
            userNotFoundPrefab.gameObject.SetActive(false);
            // remove all former requests
            for (int i = 0; i < searchUIs.Count; i++)
            {
                Destroy(searchUIs[i].gameObject);
                searchUIs.RemoveAt(i);
            }
            // create a new request object for each request
            if (friends[0] != null)
            {
                foreach (PlayerProfile fr in friends)
                {
                    SearchObject newFriendItem = Instantiate(searchPrefab, searchPrefab.transform.parent) as SearchObject;
                    newFriendItem.gameObject.SetActive(true);
                    Debug.Log("Created game object for viewing friend profile.");

                    newFriendItem.SetData(fr.Name, OnViewProfile);

                    searchUIs.Add(newFriendItem);
                }
            }
            else
            {
                userNotFoundPrefab.gameObject.SetActive(true);
            }
            
        }

        // Connected to button for requestPrefab
        public void OnRequestAccept(string id)
        {
            FriendsManager.Active.AcceptRequest(id);
            Debug.LogFormat("Accepting friend request from {0}", id);
        }

        // Connected to button for friendPrefab
        public void OnDeleteFriend(string id)
        {
            FriendsManager.Active.DeleteFriend(id);
        }

        public void OnViewProfile(string username)
        {
            Debug.Log("OnViewProfile called");
            profileViewUsername.text = username;
            friendProfileScreenController.LoadFriendData(username);
        }

        List<RequestObjects> requestUIs = new List<RequestObjects>();
        List<FriendObject> friendsUIs = new List<FriendObject>();
        List<SearchObject> searchUIs = new List<SearchObject>();
    }
}
