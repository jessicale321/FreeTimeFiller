using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomTaskButton : MonoBehaviour
{
    private CustomTaskCreator _myCreator;

    [Header("Required Components")]
    private Button _buttonComponent;
    private TMP_Text _taskNameDisplayed;

    public TaskData TaskData { get; private set; }

    private void Awake()
    {
        _buttonComponent = GetComponent<Button>();
        _taskNameDisplayed = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        _buttonComponent.onClick.AddListener(SelectButton);
    }

    private void OnDisable()
    {
        _buttonComponent.onClick.RemoveListener(SelectButton);
    }

    private void SelectButton()
    {
        if(_myCreator != null)
        {
            _myCreator.EditExistingTask(this);
        }
        Debug.Log(_taskNameDisplayed.text + " was selected! Begin editing this custom task.");
    }

    public void UpdateCustomTaskButton(TaskData taskData, CustomTaskCreator customTaskCreator)
    {
        TaskData = taskData;

        _myCreator = customTaskCreator;

        _taskNameDisplayed.text = taskData.taskName;
    }
}
