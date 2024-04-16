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
        
        public void SetData(PlayerProfile user, Action<string> onAccept)
        {
            // save information
            m_userID = user.Id;

            // display information
            userIDDisplay.SetText(user.Name);

            // add callback
            acceptButton.onClick.AddListener(() =>
            {
                onAccept.Invoke(m_userID);
            });
        }
    }
}

