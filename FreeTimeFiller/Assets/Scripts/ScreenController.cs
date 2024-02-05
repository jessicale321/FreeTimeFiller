using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenController : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject searchScreen;
    public GameObject profileScreen;

    public Button homeButton;
    public Button searchButton;
    public Button profileButton;


    void Start()
    {
        homeScreen.SetActive(true);
        searchScreen.SetActive(false);
        profileScreen.SetActive(false);

        homeButton.onClick.AddListener(OnHomeButtonClicked);
        searchButton.onClick.AddListener(OnSearchButtonClicked);
        profileButton.onClick.AddListener(OnProfileButtonClicked);
    }
    
    public void OnHomeButtonClicked()
    {
        homeScreen.SetActive(true);
        searchScreen.SetActive(false);
        profileScreen.SetActive(false);
    }

    public void OnSearchButtonClicked()
    {
        homeScreen.SetActive(false);
        searchScreen.SetActive(true);
        profileScreen.SetActive(false);
    }

    public void OnProfileButtonClicked()
    {
        homeScreen.SetActive(false);
        searchScreen.SetActive(false);
        profileScreen.SetActive(true);
    }
}
