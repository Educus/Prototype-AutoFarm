
using System.Collections.Generic;
using UnityEngine;

public interface Loader<Key, Value>
{
    public Dictionary<Key, Value> MakeDict();
}

public interface IInteractable
{
    void OnInteract();
}