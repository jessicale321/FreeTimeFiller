using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class ChooseTaskPool : MonoBehaviour
{
    // Where to place buttons under
    [SerializeField] private Transform categoryPanel;
    // The button to spawn and put text inside of
    [SerializeField] private GameObject taskCategoryButton;

    [SerializeField] private Button finishChoosingButton;

    private Dictionary<TaskCategory, CategoryButton> taskButtons = new Dictionary<TaskCategory, CategoryButton>();

    private List<TaskCategory> _chosenTaskCategories = new List<TaskCategory>();

    private async void Awake()
    {
        // Sign in to account anonymously (* should use actual account login *)
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        LoadCategoriesFromCloud();
    }

    private void Start()
    {    
        CreateButtons();
    }

    private void OnEnable()
    {
        finishChoosingButton.onClick.AddListener(FinishChoosing);
    }

    private void OnDisable()
    {
        finishChoosingButton.onClick.RemoveListener(FinishChoosing);
    }

    ///-///////////////////////////////////////////////////////////
    /// Spawn in a button for every task category that exists, then place it on the canvas.
    /// 
    private void CreateButtons()
    {
        // For each Task Category we have created, add a drop down element for it
        foreach (TaskCategory category in Enum.GetValues(typeof(TaskCategory)))
        {
            GameObject newButton =  Instantiate(taskCategoryButton, categoryPanel);

            CategoryButton categoryButtonComponent = newButton.GetComponent<CategoryButton>();

            categoryButtonComponent.SetCustomTaskCreator(this);
            categoryButtonComponent.UpdateDisplayedCategory(category);

            taskButtons.Add(category, categoryButtonComponent);
        }
    }

    private void FinishChoosing()
    {
        if (_chosenTaskCategories.Count > 0)
        {
            Debug.Log("User finished picking categories!");

            SaveCategoriesToCloud();
        }
        else
        {
            Debug.Log("Please choose atleast 1 category");
        }       
    }

    public async void SaveCategoriesToCloud()
    {
        JsonUtility.ToJson(_chosenTaskCategories);

        // Save list of custom tasks to the user's account
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "chosenTaskCategories", _chosenTaskCategories} });

        Debug.Log("Saved chosen task categories");
    }

    public async void LoadCategoriesFromCloud()
    {
        // Load the list of custom tasks created by the user from their cloud account
        var savedList = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>
        {
            "chosenTaskCategories"
        });

        // If there's data loaded, deserialize it back into a list of strings
        if (savedList.TryGetValue("chosenTaskCategories", out var data))
        {
            List<string> listLoaded = data.Value.GetAs<List<string>>();

            foreach (string categoryAsJson in listLoaded)
            {
                // Parse each saved string back to the enum representation
                if (Enum.TryParse(categoryAsJson, out TaskCategory category))
                {
                    _chosenTaskCategories.Add(category);

                    // Re-select all buttons that the user selected in the past
                    taskButtons[category].SelectButtonOnCommand();

                    Debug.Log($"User loaded task category: {category}");
                }
                else
                {
                    Debug.LogWarning($"Failed to parse task category: {categoryAsJson}");
                }
            }
        }
    }

    public void AddClickedButton(CategoryButton categoryButtonClicked)
    {
        TaskCategory clickedCategory = categoryButtonClicked.GetTaskCategory();

        // Don't add duplicates
        if (_chosenTaskCategories.Contains(clickedCategory)) return;

        _chosenTaskCategories.Add(clickedCategory);

        Debug.Log($"User has chosen: {clickedCategory}");
    }

    public void RemoveClickedButton(CategoryButton categoryButtonClicked)
    {
        TaskCategory clickedCategory = categoryButtonClicked.GetTaskCategory();

        // Don't remove a task category if it was never selected
        if (!_chosenTaskCategories.Contains(clickedCategory)) return;

        _chosenTaskCategories.Remove(clickedCategory);

        Debug.Log($"User has removed: {clickedCategory}");
    }
}
