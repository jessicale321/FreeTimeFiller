using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Minigame Task", menuName = "ScriptableObjects/Task Data/Minigame")]
public class SpecialTaskData : TaskData
{
    // What is the index of the scene to load?
    public string sceneName;
}
