using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//-/////////////////////////////////////////////////////////////////////
///
public class ScreenController : MonoBehaviour
{
    [SerializeField] private MenuScreen homeScreen;
    [SerializeField] private MenuScreen searchScreen;
    [SerializeField] private MenuScreen profileScreen;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button profileButton;

    private MenuScreen lastShownScreen;

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void Start()
    {
        homeScreen.Show();
        lastShownScreen = homeScreen;

        homeButton.onClick.AddListener(OnHomeButtonClicked);
        searchButton.onClick.AddListener(OnSearchButtonClicked);
        profileButton.onClick.AddListener(OnProfileButtonClicked);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnHomeButtonClicked()
    {
        lastShownScreen.Hide();
        homeScreen.Show();
        lastShownScreen = homeScreen;
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnSearchButtonClicked()
    {
        lastShownScreen.Hide();
        searchScreen.Show();
        lastShownScreen = searchScreen;
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnProfileButtonClicked()
    {
        lastShownScreen.Hide();
        profileScreen.Show();
        lastShownScreen = profileScreen;
    }
}
