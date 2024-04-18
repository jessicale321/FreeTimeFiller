using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Minigame : MonoBehaviour
{
    [SerializeField, Range(1f, 300f)] private float gameTimer;

    private string _currentSceneName;

    private void Start()
    {
        Invoke(nameof(OnMinigameConcluded), 3f);
    }

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    ///-///////////////////////////////////////////////////////////
    /// Get the name of the current scene that just loaded
    /// 
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(mode == LoadSceneMode.Additive)
            _currentSceneName = scene.name;
    }

    protected virtual void OnMinigameLoaded()
    {
        
    }

    protected virtual void OnMinigameConcluded()
    {
        // Unload the current scene (go back to home page)
        SceneManager.UnloadSceneAsync(_currentSceneName);
    }
}
