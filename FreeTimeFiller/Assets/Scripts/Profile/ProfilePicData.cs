using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewProfilePicture", menuName = "ScriptableObjects/ProfilePicData/GenericPic")]
public class ProfilePicData : ScriptableObject
{
    //public string name;
    public Sprite sprite;
    public bool isUnlocked;
}
