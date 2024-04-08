using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//-/////////////////////////////////////////////////////////////////////
///
public class ScreenController : MonoBehaviour
{
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject searchScreen;
    [SerializeField] private GameObject profileScreen;
    [SerializeField] private GameObject friendProfileScreen;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button profileButton;

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void Start()
    {
        homeScreen.SetActive(true);
        searchScreen.SetActive(false);
        profileScreen.SetActive(false);

        homeButton.onClick.AddListener(OnHomeButtonClicked);
        searchButton.onClick.AddListener(OnSearchButtonClicked);
        profileButton.onClick.AddListener(OnProfileButtonClicked);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnHomeButtonClicked()
    {
        homeScreen.SetActive(true);
        searchScreen.SetActive(false);
        profileScreen.SetActive(false);
        friendProfileScreen.SetActive(false);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnSearchButtonClicked()
    {
        homeScreen.SetActive(false);
        searchScreen.SetActive(true);
        profileScreen.SetActive(false);
        friendProfileScreen.SetActive(false);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnProfileButtonClicked()
    {
        homeScreen.SetActive(false);
        searchScreen.SetActive(false);
        profileScreen.SetActive(true);
        friendProfileScreen.SetActive(false);
    }

    public void OnViewProfileClicked()
    {
        homeScreen.SetActive(false);
        searchScreen.SetActive(false);
        profileScreen.SetActive(false);
        friendProfileScreen.SetActive(true);
    }
}
