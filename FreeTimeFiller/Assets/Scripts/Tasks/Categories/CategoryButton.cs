using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CategoryButton : MonoBehaviour
{
    private CategoryManager _myCreator;
    private TaskCategory _displayedCategory;

    private Button _buttonComponent;
    [SerializeField] private TMP_Text textBox;

    [SerializeField] private GameObject selectedBox;

    // Did the user already click on this button (they want this category to show in the task pool)
    private bool _selected;

    private void Awake()
    {
        _buttonComponent = GetComponent<Button>();
        selectedBox.SetActive(false);
    }

    private void OnEnable()
    {
        ReselectButtonOnEnable();
        
        _buttonComponent.onClick.AddListener(ChooseTaskCategory);
    }

    private void OnDisable()
    {
        _buttonComponent.onClick.RemoveListener(ChooseTaskCategory);
    }

    ///-///////////////////////////////////////////////////////////
    /// Set the TaskCategory that this button will display on screen.
    /// 
    public void UpdateDisplayedCategory(TaskCategory category)
    {
        _displayedCategory = category;

        textBox.text = category.ToString();
    }

    ///-///////////////////////////////////////////////////////////
    /// Set a reference to the TaskPool script
    /// 
    public void SetCustomTaskCreator(CategoryManager taskPoolCreator)
    {
        _myCreator = taskPoolCreator;
    }

    ///-///////////////////////////////////////////////////////////
    /// When this button is clicked on, select it and tell the TaskPool script
    /// that the user might want this task category. If this button was already selected,
    /// tell the TaskPool script that the user no longer wants this task category.
    /// 
    private void ChooseTaskCategory()
    {
        if (!_selected)
        {
            SelectButtonOnCommand();
        }

        else
        {
            UnselectButtonOnCommand();
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// When this button gameObject is enabled, check to see if the category this button
    /// displays, is saved to the user's cloud. If so, then this button should become selected
    /// to notify the user that they have it saved. This also prevents buttons from remaining unselected if the user
    /// closes the category screen without saving.
    /// 
    private void ReselectButtonOnEnable()
    {
        // If this button doesn't have a reference to the CategoryManager, then return
        if (_myCreator == null) return;

        if (_myCreator.IsCategorySaved(_displayedCategory))
        {
            SelectButtonOnCommand();
        }
        else
        {
            UnselectButtonOnCommand();
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Call this method to select the button without clicking on it.
    /// 
    public void SelectButtonOnCommand()
    {
        _selected = true;
        selectedBox.SetActive(true);
        _myCreator.AddClickedButton(this);
    }

    ///-///////////////////////////////////////////////////////////
    /// A button was clicked on after it was already selected, therefore the user
    /// doesn't want this category anymore.
    /// 
    private void UnselectButtonOnCommand()
    {
        _selected = false;
        selectedBox.SetActive(false);
        _myCreator.RemoveClickedButton(this);
    }
    

    ///-///////////////////////////////////////////////////////////
    /// Return the task category that is displayed by this button
    /// 
    public TaskCategory GetTaskCategory()
    {
        return _displayedCategory;
    }
}
