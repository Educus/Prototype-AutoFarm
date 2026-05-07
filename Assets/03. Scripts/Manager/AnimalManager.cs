using System.Collections.Generic;
using UnityEngine;

public enum AnimalType
{
    NONE,
    COW,
    CHICKEN
}
public class AnimalManager : MonoBehaviour
{
    public Dictionary<string, AnimalBase> animals = new Dictionary<string, AnimalBase>();

    public void Register(AnimalBase animal)
    {
        if (!animals.ContainsKey(animal.id))
            animals.Add(animal.id, animal);
    }

    public AnimalBase Get(string id)
    {
        return animals[id];
    }
}
