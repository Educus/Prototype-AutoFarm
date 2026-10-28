using System;
using UnityEngine;

[Serializable]
public class AnimalSaveData
{
    public int itemId;
    public string id;
    public string animalName;
    public Vector3 position;

    public int isStack;

    public string ranchBuildingID;

    public int remainingTime;

    public int todayHarvestCount;
}
