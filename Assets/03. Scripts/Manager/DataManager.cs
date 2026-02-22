using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Crops
{
    [SerializeField] public int index;
    [SerializeField] public string name;
    [SerializeField] public int price;
    [SerializeField] public int cropsClass;
}
[Serializable]
public class CropsData : Loader<int, Crops>
{
    public List<Crops> crops = new List<Crops>();

    public Dictionary<int, Crops> MakeDict()
    {
        Dictionary<int, Crops> dict = new Dictionary<int, Crops> ();

        foreach (Crops s in crops)
        {
            if (!dict.ContainsValue(s))
            {
                dict.Add(s.index, s);
            }
        }
        return dict;
    }
}

public class DataManager : MonoBehaviour
{
    // Json파일 읽기 쓰기

    public Dictionary<int, Crops> cropsDict = new Dictionary<int, Crops>();
    
    private void Start()
    {
        // Json파일 불러오기
        cropsDict = LoadJson<CropsData, int, Crops>("Json/Crops").MakeDict();

        /*
        // Load 갯수 확인
        Debug.Log("Crops Count: " + cropsDict.Count);

        // Load 내용물 확인
        foreach (var pair in cropsDict)
        {
            Debug.Log(
                $"Key: {pair.Key} | " +
                $"Index: {pair.Value.index} | " +
                $"Name: {pair.Value.name} | " +
                $"Price: {pair.Value.price} | " +
                $"Class: {pair.Value.cropsClass}"
            );
        }
        */
    }

    Load LoadJson<Load, Key, Value>(string path) where Load : Loader<Key, Value>
    {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<Load>(textAsset.text);
    }
}
