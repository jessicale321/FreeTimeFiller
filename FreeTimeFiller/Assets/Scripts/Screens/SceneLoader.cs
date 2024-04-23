using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneLoader : MonoBehaviour
{
    // Disable these two components when loading a new scene
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private EventSystem eventSystem;
}
