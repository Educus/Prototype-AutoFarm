using System.Collections.Generic;
using UnityEngine;

public class RanchBuilding : BuildingBase
{
    public List<AnimalBase> animals = new List<AnimalBase>();

    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Ranch;
    }

    #region NPC 참조 영역


    #endregion

    public override string GetJsonData()
    {
        throw new System.NotImplementedException();
    }

    public override void LoadJsonData(string json)
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteract()
    {
        
    }

    
}
