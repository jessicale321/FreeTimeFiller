using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySlider : MonoBehaviour
{
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private TMP_Text difficultyText;

    private void OnEnable()
    {
        difficultySlider.onValueChanged.AddListener(UpdateText);
    }

    private void OnDisable()
    {
        difficultySlider.onValueChanged.RemoveListener(UpdateText);
    }

    ///-///////////////////////////////////////////////////////////
    /// When the slider's value has changed, update the text on screen to show the 
    /// new difficulty level.
    /// 
    private void UpdateText(float newValue)
    {
        // Convert slider's value to an integer, then update text on screen
        difficultyText.text = "Difficulty: " + ((int) newValue).ToString();
    }

    ///-///////////////////////////////////////////////////////////
    /// Set the difficulty slider's displayed value to the float value passed in.
    /// 
    public void SetDifficulty(float newValue)
    {
        difficultySlider.value = newValue;
        UpdateText(newValue);
    }

    ///-///////////////////////////////////////////////////////////
    /// Return the currently selected difficulty level.
    /// 
    public int GetDifficultyValue()
    {
        return (int) difficultySlider.value;
    }
}
