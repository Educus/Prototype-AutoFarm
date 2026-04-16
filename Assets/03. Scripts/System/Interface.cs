
using System.Collections.Generic;
using UnityEngine;

public interface Loader<Key, Value>
{
    public Dictionary<Key, Value> MakeDict();
}

public interface IInteractable
{
    string GetName();
    string GetStatus();
    void OnInteract();
}

public interface ISaveable
{
    string Save();
    void Load(string json);
}