using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomTaskCreator : MonoBehaviour
{
    [SerializeField] private TMP_InputField taskNameInputField;

    [SerializeField] private Slider difficultySlider;

    [SerializeField] private Button createButton;

    private void OnEnable()
    {
        createButton.onClick.AddListener(AttemptCreation);
    }

    private void OnDisable()
    {
        createButton.onClick.RemoveListener(AttemptCreation);
    }

    private void AttemptCreation()
    {
        string taskName = taskNameInputField.text;

        // Don't allow task names that have no actual text
        if (string.IsNullOrEmpty(taskName) || taskName == "")
        {
            Debug.Log("Task name is empty!");
            return;
        }

        CreateCustomTask();
    }

    private void CreateCustomTask()
    {
        string taskName = taskNameInputField.text;

        TaskData newCustomTask = ScriptableObject.CreateInstance<TaskData>();

        newCustomTask.taskName = taskName;
        newCustomTask.difficultyLevel = ((int)difficultySlider.value);



        // TESTING CUSTOM TASK CREATION
        Debug.Log($"User entered -> Name: {newCustomTask.taskName}, Difficulty: {newCustomTask.difficultyLevel}");

        string path = $"Assets/Resources/Task Data/Custom/{taskName} Custom.asset";
        AssetDatabase.CreateAsset(newCustomTask, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newCustomTask;
    }
}
