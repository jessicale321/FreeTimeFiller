using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Minigame : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(OnMinigameConcluded), 3f);
    }

    protected virtual void OnMinigameLoaded()
    {
        
    }

    protected virtual void OnMinigameConcluded()
    {
        // Unload the current scene (go back to home page)
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
