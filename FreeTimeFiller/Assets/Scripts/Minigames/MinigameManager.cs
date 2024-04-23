using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    [SerializeField, Range(1f, 300f)] private float gameTimer;

    [SerializeField, Range(0.1f, 1f)] private float gameTimeScale;

    [SerializeField] private Button quitButton;

    private float _currentGameTimer;

    private string _currentSceneName;

    private bool _gameIsPlaying;

    [Header("UI")]
    [SerializeField] private TMP_Text timerUI;

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        quitButton.onClick.AddListener(OnMinigameConcluded);
    }
    private void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        quitButton.onClick.RemoveListener(OnMinigameConcluded);
    }

    private void Start()
    {
        Time.timeScale = gameTimeScale;
        _currentGameTimer = gameTimer;
        StartGame();
    }

    ///-///////////////////////////////////////////////////////////
    /// Get the name of the current scene that just loaded.
    /// 
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(mode == LoadSceneMode.Additive)
            _currentSceneName = scene.name;
    }

    ///-///////////////////////////////////////////////////////////
    /// Do something when the minigame has loaded.
    /// 
    private void StartGame()
    {
        _gameIsPlaying = true;
    }

    private void Update()
    {
        // Countdown game timer only while it's playing
        if (_gameIsPlaying)
        {
            float deltaTime = Time.deltaTime/gameTimeScale;

            // Update remaining time
            _currentGameTimer -= deltaTime;

            // Display timer
            timerUI.text = ((int)_currentGameTimer).ToString();
        }

        // Conclude the game when the timer reaches 0
        if (_currentGameTimer <= 0f && _gameIsPlaying)
        {
            _gameIsPlaying = false;

            OnMinigameConcluded();
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// // Unload the current scene (go back to home page).
    /// 
    public void OnMinigameConcluded()
    {
        // Reset timescale before unloading scene
        Time.timeScale = 1f;

        SceneManager.UnloadSceneAsync(_currentSceneName);
    }
}
