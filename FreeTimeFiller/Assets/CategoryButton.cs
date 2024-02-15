using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CategoryButton : MonoBehaviour
{
    private TaskCategory displayedCategory;

    private TMP_Text textBox;

    private void Awake()
    {
        // Get reference to text component
        textBox = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void UpdateDisplayedCategory(TaskCategory category)
    {
        displayedCategory = category;

        textBox.text = category.ToString();
    }
}
