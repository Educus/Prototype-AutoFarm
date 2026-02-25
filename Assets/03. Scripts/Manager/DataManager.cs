using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class Crops
{
    [SerializeField] public int itemId;
    [SerializeField] public string itemName;
    [SerializeField] public float priceStdDev;
    [SerializeField] public int costPrice;
    [SerializeField] public string growthTime;
    [SerializeField] public string maximumYield;
    [SerializeField] public string waterConsumption;
}

public class DataManager : MonoBehaviour
{
    // 아이템 시세 불러오기 저장하기
    // Json파일 읽기 쓰기

    public Dictionary<int, Crops> cropsDict = new Dictionary<int, Crops>();

    private string path;

    private void Awake()
    {
        // 게임 실행 시 origin파일 불러오기
        path = Path.Combine(Application.dataPath, "Resources/Json/CropsOrigin.Json");

        LoadCrops();
        
        PrintAll();
    }

    // 이어하기
    public void ContinueGame()
    {
        // 이어하기 시 저장된 파일 불러오기
        path = Path.Combine(Application.dataPath, "Resources/Json/Crops1.Json");

        LoadCrops();
    }

    // 저장
    public void SaveCrops()
    {
        try
        {
            string json = JsonConvert.SerializeObject(cropsDict, Formatting.Indented);
            path = Path.Combine(Application.dataPath, "Resources/Json/Crops1.Json");
            File.WriteAllText(path, json);

            Debug.Log("저장 성공" + path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving data : {ex.Message}");
        }
    }

    // 로드
    public void LoadCrops()
    {
        if (!File.Exists(path))
        {
            // 저장된 파일이 없을 경우
            Debug.Log("파일 없음");
            cropsDict = new Dictionary<int, Crops>();
            return;
        }

        string json = File.ReadAllText(path);

        cropsDict = JsonConvert.DeserializeObject<Dictionary<int, Crops>>(json);

        Debug.Log("로드 성공");
    }

    // 저장 내용 출력
    public void PrintAll()
    {
        foreach (var pair in cropsDict)
        {
            Debug.Log($"Key:{pair.Key} Name:{pair.Value.itemName} Price:{pair.Value.costPrice} Class:{pair.Value.priceStdDev}");
        }
    }
}
