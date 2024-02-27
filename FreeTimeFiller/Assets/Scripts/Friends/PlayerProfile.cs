using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    [field:SerializeField] public string Name { get; private set; }

    [field:SerializeField] public string Id { get; private set; }

    public PlayerProfile(string name, string id)
    {
        Name = name;
        Id = id;
    }

    public override string ToString()
    {
        return $"{Name}, Id: {Id}";
    }
}
