using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
}
