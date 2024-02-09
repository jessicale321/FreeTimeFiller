using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CategoryDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown taskCategoryDropdown;

    private Dictionary<TaskCategory, int> categoryToIndex = new Dictionary<TaskCategory, int>();

    private TaskCategory selectedCategory;

    private void OnEnable()
    {
        taskCategoryDropdown.onValueChanged.AddListener(UpdateSelectedCategory);
    }

    private void OnDisable()
    {
        taskCategoryDropdown.onValueChanged.RemoveListener(UpdateSelectedCategory);
    }

    private void Start()
    {
        PopulateDropdown();
    }

    // Set the currently selected task category to dropdown element the user has clicked on
    private void UpdateSelectedCategory(int index)
    {
        // The selected category is located at the index of the selected dropdown element
        selectedCategory = categoryToIndex.ElementAt(index).Key;
    }

    // Add all TaskCategories to the dropdown lists
    private void PopulateDropdown()
    {
        taskCategoryDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        int index = 0;

        // For each Task Category we have created, add a drop down element for it
        foreach (TaskCategory category in Enum.GetValues(typeof(TaskCategory)))
        {
            // Convert the task category name to a string
            options.Add(new TMP_Dropdown.OptionData(category.ToString(), null));

            categoryToIndex.Add(category,index++);
        }

        taskCategoryDropdown.AddOptions(options);
    }

    // Return the currently selected task category
    public TaskCategory GetSelectedTaskCategory()
    {
        return selectedCategory;
    }
}
