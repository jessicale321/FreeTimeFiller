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
        public TMP_Text playerUserName;
        public TMP_Text playerUserID;
        public TMP_Text playerInfo;

        public GameObject friendsLobby;

        [Header("From Control Area")] public TMP_InputField friendRecipientID;

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
            foreach (object o in transform)
            {
                (o as Transform).gameObject.SetActive(false);
            }

            FriendsManager.Active.OnUserSignIn += OnUserSignIn;
            FriendsManager.Active.OnRequestsRefresh += OnRequestsRefresh;
            FriendsManager.Active.OnFriendsRefresh += OnFriendsRefresh;
        }

        // Update is called once per frame
        private void OnUserSignIn()
        {
            playerUserID.text = AuthenticationService.Instance.PlayerId;
            playerUserName.text = AuthenticationService.Instance.PlayerName;
            playerInfo.text = AuthenticationService.Instance.Profile;
            Debug.Log("We accessed the stuff");

            friendsLobby.SetActive(true);
        }

        public void SendFriendRequest_ID()
        {
            FriendsManager.Active.SendFriendRequest_ID(friendRecipientID.text);
        }

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
                RequestObjects newRequestItem = Instantiate(requestPrefab, requestPrefab.transform.parent);
                newRequestItem.gameObject.SetActive(true);

                // set the data for the new request
                newRequestItem.SetData(req.Id, OnRequestAccept);

                requestUIs.Add(newRequestItem);
            }

            Debug.Log("Updating List");
        }

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

                // set the data for the request
                newFriendItem.SetData(fr.Id, OnDeleteFriend);

                friendsUIs.Add(newFriendItem);
            }
        }

        private void OnRequestAccept(string id)
        {
            FriendsManager.Active.AcceptRequest(id);
        }

        private void OnDeleteFriend(string id)
        {
            FriendsManager.Active.DeleteFriend(id);
        }

        List<RequestObjects> requestUIs = new List<RequestObjects>();
        List<FriendObject> friendsUIs = new List<FriendObject>();
    }
}
