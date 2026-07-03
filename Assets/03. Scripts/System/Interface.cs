
using System.Collections.Generic;
using UnityEngine;

public interface Loader<Key, Value>
{
    public Dictionary<Key, Value> MakeDict();
}

public interface ILeftInteractable
{
    void OnInteract();
}

public interface IRightInteractable
{
    void OnInteract(int itemId);
}