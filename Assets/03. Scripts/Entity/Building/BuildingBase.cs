using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BuildingInteraction))]
public abstract class BuildingBase : MonoBehaviour
{
    public BuildingType type = BuildingType.None;

    public int itemID;
    // [HideInInspector]
    public string id;
    public bool isClone = false;

    public BuildingData data;

    public Sprite icon;

    public abstract string GetJsonData();
    public abstract void LoadJsonData(string json);


    protected virtual void Awake()
    {
        StartCoroutine(IEAwake());
    }

    private IEnumerator IEAwake()
    {
        while (!isClone) { yield return null; }

        id = gameObject.name;
        DataManager.Instance.BuildingManager.Register(this);
    }
}
