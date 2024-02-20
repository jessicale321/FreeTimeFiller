using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class TaskPool : MonoBehaviour
{
    [Header("UI Components")]
    // Where to place buttons under
    [SerializeField] private Transform categoryPanel;
    // The button to spawn and put text inside of
    [SerializeField] private GameObject taskCategoryButton;
    // The button that save the list of chosen task categories to the user's cloud account
    [SerializeField] private Button finalizeChoicesButton;
    [SerializeField] private Button resetChoicesButton;

    /* A dictionary that stores a category and a button along with it
     * Key: The task category (ex. Chores)
     * Value: The button that was created for the category
     */
    private Dictionary<TaskCategory, CategoryButton> _taskButtons = new Dictionary<TaskCategory, CategoryButton>();

    // The list of task categories that the user has picked out and saved
    public List<TaskCategory> ChosenTaskCategories { get; private set; }

    public event Action<List<TaskCategory>> TaskCategoriesChanged;

    private async void Awake()
    {
        // Sign in to account anonymously (* should use actual account login *)
        await UnityServices.InitializeAsync();

        ChosenTaskCategories = new List<TaskCategory>();
    }

    private void Start()
    {    
        // Spawn in buttons for each task category at the start
        CreateButtons();
    }

    private void OnEnable()
    {
        finalizeChoicesButton.onClick.AddListener(FinishChoosing);
        resetChoicesButton.onClick.AddListener(ClearAllCategoryChoicesData);
    }

    private void OnDisable()
    {
        finalizeChoicesButton.onClick.RemoveListener(FinishChoosing);
        resetChoicesButton.onClick.RemoveListener(ClearAllCategoryChoicesData);
    }

    ///-///////////////////////////////////////////////////////////
    /// Spawn in a button for every task category that exists, then place it on the canvas.
    /// 
    private void CreateButtons()
    {
        // For each Task Category we have created, add a drop down element for it
        foreach (TaskCategory category in Enum.GetValues(typeof(TaskCategory)))
        {
            // Spawn in a new button
            GameObject newButton =  Instantiate(taskCategoryButton, categoryPanel);

            CategoryButton categoryButtonComponent = newButton.GetComponent<CategoryButton>();

            // In the new button, give it a reference for this TaskPool and give it a TaskCategory
            categoryButtonComponent.SetCustomTaskCreator(this);
            categoryButtonComponent.UpdateDisplayedCategory(category);

            _taskButtons.Add(category, categoryButtonComponent);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// When the user has clicked the "Finish" button, call the method to save the categories to the
    /// user's cloud account.
    /// 
    private void FinishChoosing()
    {
        // Only attempt to save the task categories if the list isn't empty
        if (ChosenTaskCategories.Count > 0)
        {
            Debug.Log("User finished picking categories!");

            SaveCategoriesToCloud();
        }
        else
        {
            Debug.Log("Please choose at least 1 category");
        }       
    }

    ///-///////////////////////////////////////////////////////////
    /// Take the list of categories that the user chose and convert to a json string.
    /// Then save that string to a user's cloud account.
    /// 
    private async void SaveCategoriesToCloud()
    {
        JsonUtility.ToJson(ChosenTaskCategories);

        // Save list of custom tasks to the user's account
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { "chosenTaskCategories", ChosenTaskCategories} });

        // Tell listeners that the user has changed their task category preferences
        TaskCategoriesChanged?.Invoke(ChosenTaskCategories);

        Debug.Log("Saved chosen task categories");
    }

    ///-///////////////////////////////////////////////////////////
    /// Find the list of task categories that the user saved in the past.
    /// Re-select any buttons on the screen if the user selected them in the past.
    /// 
    public async Task LoadCategoriesFromCloud()
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

            if (listLoaded != null)
            {
                // Loop through each serialized string and try to convert them back to TaskCategories
                foreach (string categoryAsJson in listLoaded)
                {
                    // Parse each saved string back to the enum representation
                    if (Enum.TryParse(categoryAsJson, out TaskCategory category))
                    {
                        ChosenTaskCategories.Add(category);

                        // Re-select all buttons that the user selected in the past
                        if (_taskButtons.TryGetValue(category, out CategoryButton button))
                        {
                            _taskButtons[category].SelectButtonOnCommand();
                        }
                        Debug.Log($"User loaded task category: {category}");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to parse task category: {categoryAsJson}");
                    }
                }
            }
            else
            {
                Debug.Log("Could not find any saved Task Categories!");
            }
            
            // Tell listeners that the user has changed their task category preferences
            TaskCategoriesChanged?.Invoke(ChosenTaskCategories);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Delete all task category preferences made by the user.
    /// 
    private async void ClearAllCategoryChoicesData()
    {
        ChosenTaskCategories.Clear();

        // Overwrite the data with an empty string to "delete" it
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
        { "chosenTaskCategories", "" }
    });

        Debug.Log("Task category preferences were reset!");
    }

    ///-///////////////////////////////////////////////////////////
    /// When a task category button is clicked, it will call this method to tell this script
    /// that the user has a task category they might want in their preferences.
    /// 
    public void AddClickedButton(CategoryButton categoryButtonClicked)
    {
        TaskCategory clickedCategory = categoryButtonClicked.GetTaskCategory();

        // Don't add duplicates
        if (ChosenTaskCategories.Contains(clickedCategory)) return;

        ChosenTaskCategories.Add(clickedCategory);

        Debug.Log($"User has chosen: {clickedCategory}");
    }

    ///-///////////////////////////////////////////////////////////
    /// When a task category is clicked (after already being clicked), it will call this method
    /// to tell this script that the user no longer wants that task category in their preferences.
    /// 
    public void RemoveClickedButton(CategoryButton categoryButtonClicked)
    {
        TaskCategory clickedCategory = categoryButtonClicked.GetTaskCategory();

        // Don't remove a task category if it was never selected
        if (!ChosenTaskCategories.Contains(clickedCategory)) return;

        ChosenTaskCategories.Remove(clickedCategory);

        Debug.Log($"User has removed: {clickedCategory}");
    }
}
