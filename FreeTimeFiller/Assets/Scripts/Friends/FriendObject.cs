using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace UI
{
    public class FriendObject : MonoBehaviour
    {
        public TMP_Text userIDDisplay;
        public Button deleteButton;

        private string m_userID;

        public void SetData(string userID, string name, Action<string> OnDelete)
        {
            // save information
            m_userID = userID;

            // display information
            userIDDisplay.SetText(name);

            // add callback
            deleteButton.onClick.AddListener(() =>
            {
                OnDelete?.Invoke(m_userID);
            });
        }
    }
}

