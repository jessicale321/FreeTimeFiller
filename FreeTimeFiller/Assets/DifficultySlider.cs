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

    private void UpdateText(float newValue)
    {
        // Convert slider's value to an integer, then update text on screen
        difficultyText.text = "Difficulty: " + ((int) newValue).ToString();
    }
}
