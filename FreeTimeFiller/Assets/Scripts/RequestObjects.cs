using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RequestObjects : MonoBehaviour
    {

        public TMP_Text userIDDisplay;
        public Button acceptButton;

        private string m_userID;
        
        public void SetData(string UserID, Action<string> onAccept)
        {
            // save information
            m_userID = UserID;

            // display information
            userIDDisplay.SetText(UserID);

            // add callback
            acceptButton.onClick.AddListener(() =>
            {
                onAccept.Invoke(m_userID);
            });
        }
    }
}

