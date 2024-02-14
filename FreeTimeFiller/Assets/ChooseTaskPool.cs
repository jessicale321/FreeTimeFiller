using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChooseTaskPool : MonoBehaviour
{
    // Where to place buttons under
    [SerializeField] private Transform categoryPanel;
    // The button to spawn and put text inside of
    [SerializeField] private GameObject taskCategoryButton;

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
            GameObject categoryButton =  Instantiate(taskCategoryButton, categoryPanel);
            categoryButton.GetComponentInChildren<TMP_Text>().text = category.ToString();
        }
    }
}
