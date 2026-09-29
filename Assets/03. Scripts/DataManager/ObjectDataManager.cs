using Newtonsoft.Json;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ObjectType
{
    PC,
    NPC,
    Cattle
}
public class ObjectData
{
    [SerializeField] public int ID;
    [SerializeField] public string name;
    [SerializeField] public ObjectType type;
    [SerializeField] public int Speed;

    // 플레이어, NPC 전용
    [SerializeField] public int MainInv;
    [SerializeField] public int SubInv;
    [SerializeField] public int WorkDuration;       // 작업에 걸리는 시간

    // 동물 전용
    [SerializeField] public int DailyYield;         // 하루 생산 가능량
    [SerializeField] public int YieldCoolDown;      // 생산 쿨타임
    [SerializeField] public int YieldMaterial;      // 생산에 필요한 재료
    [SerializeField] public int YieldItem;          // 생산 아이템
}

public class ObjectDataManager : MonoBehaviour
{
    public Dictionary<int, ObjectData> objectData = new Dictionary<int, ObjectData>();

    private TextAsset jsonFile;

    private void Awake()
    {
        // 게임 실행 시 ProductDataTable 불러오기
        LoadObjectDataTable();

        // PrintAll();
    }

    // 기본 베이스 오브젝트 데이터 테이블 불러오기
    private void LoadObjectDataTable()
    {
        jsonFile = Resources.Load<TextAsset>("Json/ObjectDataTable");

        if (jsonFile == null)
        {
            Debug.Log("파일 없음");
            return;
        }

        List<ObjectData> objectList = JsonConvert.DeserializeObject<List<ObjectData>>(jsonFile.text);

        objectData.Clear();

        foreach (var obj in objectList)
        {
            objectData[obj.ID] = obj;
        }
    }
}
