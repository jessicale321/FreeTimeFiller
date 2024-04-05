using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SearchObject : MonoBehaviour
    {
        public TMP_Text userIDDisplay;
        public Button viewProfileButton;

        private string m_userID;

        public void SetData(string UserID, Action<string> onView)
        {
            // save information
            m_userID = UserID;

            // display information
            userIDDisplay.SetText(UserID);

            // add callback
            viewProfileButton.onClick.AddListener(() =>
            {
                onView.Invoke(m_userID);
            });
        }
    }
}

