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
    [SerializeField] private MenuScreen categoryChoiceScreen;

    [SerializeField] private Button homeBtn;
    [SerializeField] private Button searchBtn;
    [SerializeField] private Button profileBtn;
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
        finishChoosingCategoriesBtn.onClick.AddListener(OnFinishChoosingCategoriesButtonClicked);
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

    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnFinishChoosingCategoriesButtonClicked()
    {
        categoryChoiceScreen.Hide();
        lastShownScreen = categoryChoiceScreen;
    }
}
