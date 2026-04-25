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
    [SerializeField] public int ObjectID;
    [SerializeField] public string ObjectName;
    [SerializeField] public ObjectType ObjectType;
    [SerializeField] public int Price;
    [SerializeField] public int MainInv;
    [SerializeField] public int SubInv;
    [SerializeField] public int Speed;
    [SerializeField] public int WorkDuration;
    [SerializeField] public int ProductionAmount;
    [SerializeField] public int UseToDemo;
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
            objectData[obj.ObjectID] = obj;
        }
    }
}
