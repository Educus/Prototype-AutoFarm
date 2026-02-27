
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
    void OnInteract(Player player);
    public Transform GetTransform();
}