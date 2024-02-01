using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Task : MonoBehaviour
{
    // Data that this task is currently using
    [SerializeField] private TaskData taskData;

    [Header("UI Components")] 
    [SerializeField] private TMP_Text taskName;
    // Panel that stars are parented by
    [SerializeField] private GameObject difficultyLevelPanel;
    // Star that will spawn on the screen
    [SerializeField] private GameObject difficultyStar;
    // All stars currently spawned in
    private List<GameObject> _allStars = new List<GameObject>();

    public void UpdateTask(TaskData data)
    {
        // Change task name
        taskName.text = data.taskName;

        int starCount = _allStars.Count;
        
        // Remove excess stars
        if (starCount > data.difficultyLevel)
        {
            Debug.Log("Remove stars!");
            for (int i = 0; i < starCount - data.difficultyLevel; i++)
            {
                Destroy(_allStars[0]);
                _allStars.Remove(_allStars[0]);

            }
        }
        // Add missing stars
        else if (starCount < data.difficultyLevel)
        {
            Debug.Log("Add stars!");
            for (int i = 0; i < data.difficultyLevel - starCount; i++)
            {
                GameObject newStar =  Instantiate(difficultyStar, difficultyLevelPanel.transform, false);
                _allStars.Add(newStar);

            }
        }

    }
}
