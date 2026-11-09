using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WarningMessageData
{
    public int ID;
    public string Name;
    public string Type;
    public string DefaultKeyCode;
    public string Modifier1;
    public string Modifier2;
    public int IsRebindable;
    public string Description_KR;
}

[DefaultExecutionOrder(-100)]
public class WarningMessageManager : MonoBehaviour
{
    public Dictionary<int, WarningMessageData> warningMessageData = new Dictionary<int, WarningMessageData>();

    private void Awake()
    {
        LoadDefaultData();
    }

    void LoadDefaultData()
    {
        TextAsset jsonFile =
             Resources.Load<TextAsset>("Json/WarningMessage");

        if (jsonFile == null)
        {
            Debug.LogError("WarningMessage ¾øÀ½");
            return;
        }

        List<WarningMessageData> warningMessageList =
            JsonConvert.DeserializeObject<List<WarningMessageData>>(jsonFile.text);
    }
}
