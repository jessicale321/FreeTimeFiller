using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class ChooseTaskPool : MonoBehaviour
{
    // Where to place buttons under
    [SerializeField] private Transform categoryPanel;
    // The button to spawn and put text inside of
    [SerializeField] private GameObject taskCategoryButton;

    private Dictionary<CategoryButton, TaskCategory> taskButtons = new Dictionary<CategoryButton, TaskCategory>();

    private List<TaskCategory> chosenTaskCategories = new List<TaskCategory>();

    private void Start()
    {    
        CreateButtons();
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

            categoryButtonComponent.UpdateDisplayedCategory(category);
            categoryButtonComponent.SetCustomTaskCreator(this);

            taskButtons.Add(categoryButtonComponent, category);
        }
    }

    public void AddClickedButton(CategoryButton categoryButtonClicked)
    {
        chosenTaskCategories.Add(taskButtons[categoryButtonClicked]);

        Debug.Log($"User has chosen: {taskButtons[categoryButtonClicked]}");
    }

    public void RemoveClickedButton(CategoryButton categoryButtonClicked)
    {
        chosenTaskCategories.Remove(taskButtons[categoryButtonClicked]);

        Debug.Log($"User has removed: {taskButtons[categoryButtonClicked]}");
    }
}
