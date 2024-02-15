using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryButton : MonoBehaviour
{
    private TaskPool myCreator;
    private TaskCategory displayedCategory;

    private Button _buttonComponent;
    private TMP_Text _textBox;

    [SerializeField] private GameObject _selectedBox;

    // Did the user already click on this button (they want this category to show in the task pool)
    private bool _selected;

    private void Awake()
    {
        // Get reference to text component
        _textBox = GetComponentInChildren<TMP_Text>();
        _buttonComponent = GetComponent<Button>();
    }

    private void Start()
    {
        _selectedBox.SetActive(false);
    }

    private void OnEnable()
    {
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
        displayedCategory = category;

        _textBox.text = category.ToString();
    }

    ///-///////////////////////////////////////////////////////////
    /// Set a reference to the TaskPool script
    /// 
    public void SetCustomTaskCreator(TaskPool taskPoolCreator)
    {
        myCreator = taskPoolCreator;
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
            _selected = true;
            _selectedBox.SetActive(true);
            myCreator.AddClickedButton(this);
        }

        else
        {
            _selected = false;
            _selectedBox.SetActive(false);
            myCreator.RemoveClickedButton(this);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Call this method to select the button without clicking on it.
    /// 
    public void SelectButtonOnCommand()
    {
        _selected = true;
        _selectedBox.SetActive(true);
        myCreator.AddClickedButton(this);
    }

    ///-///////////////////////////////////////////////////////////
    /// Return the task category that is displayed by this button
    /// 
    public TaskCategory GetTaskCategory()
    {
        return displayedCategory;
    }
}
