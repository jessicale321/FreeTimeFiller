using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//-/////////////////////////////////////////////////////////////////////
///
public class ScreenController : MonoBehaviour
{
    [Header("Main Scenes")]
    [SerializeField] private MenuScreen homeScreen;
    [SerializeField] private MenuScreen searchScreen;
    [SerializeField] private MenuScreen profileScreen;

    [Header("Smaller Scenes")]
    [SerializeField] private MenuScreen settingsScreen;
    [SerializeField] private MenuScreen profileSelectScreen;
    [SerializeField] private MenuScreen categoryChoiceScreen;
    [SerializeField] private MenuScreen friendProfileScreen;

    [Header("NavBar Buttons")]
    [SerializeField] private Button homeBtn;
    [SerializeField] private Button searchBtn;
    [SerializeField] private Button profileBtn;

    [Header("Assorted Buttons")]
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button editProfilePicButton;
    [SerializeField] private Button finishChoosingCategoriesBtn;

    private MenuScreen lastShownScreen;

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void Start()
    {
        homeScreen.Show();
        lastShownScreen = homeScreen;

        homeBtn.onClick.AddListener(OnHomeButtonClicked);
        searchBtn.onClick.AddListener(OnSearchButtonClicked);
        profileBtn.onClick.AddListener(OnProfileButtonClicked);
        settingsBtn.onClick.AddListener(OnSettingsButtonClicked);
        finishChoosingCategoriesBtn.onClick.AddListener(OnFinishChoosingCategoriesButtonClicked);
        editProfilePicButton.onClick.AddListener(OnEditProfilePicButtonClicked);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void SwitchScreen(MenuScreen newScreen)
    {
        lastShownScreen.Hide();
        newScreen.Show();
        lastShownScreen = newScreen;
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnHomeButtonClicked()
    {
        SwitchScreen(homeScreen);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnSearchButtonClicked()
    {
        SwitchScreen(searchScreen);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnProfileButtonClicked()
    {
        SwitchScreen(profileScreen);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnSettingsButtonClicked()
    {
        SwitchScreen(settingsScreen);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnEditProfilePicButtonClicked()
    {
        SwitchScreen(profileSelectScreen);
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnFinishChoosingCategoriesButtonClicked()
    {
        categoryChoiceScreen.Hide();
        lastShownScreen = categoryChoiceScreen;
    }

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnViewProfileClicked()
    {
        lastShownScreen.Hide();
        friendProfileScreen.Show();
        lastShownScreen = friendProfileScreen;
    }
}
