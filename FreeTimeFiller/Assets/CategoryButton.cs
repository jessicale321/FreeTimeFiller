using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryButton : MonoBehaviour
{
    private ChooseTaskPool myCreator;
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

    public void UpdateDisplayedCategory(TaskCategory category)
    {
        displayedCategory = category;

        _textBox.text = category.ToString();
    }

    public void SetCustomTaskCreator(ChooseTaskPool taskPoolCreator)
    {
        myCreator = taskPoolCreator;
    }

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

    public void SelectButtonOnCommand()
    {
        _selected = true;
        _selectedBox.SetActive(true);
        myCreator.AddClickedButton(this);
    }

    public TaskCategory GetTaskCategory()
    {
        return displayedCategory;
    }
}
