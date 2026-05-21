using System.Collections;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour, IInteractable
{
    [Header("Building Info")]
    public BuildingType type = BuildingType.None;

    public int itemID;

    [HideInInspector]
    public string id;

    public bool isClone = false;

    public BuildingData data;

    public Sprite icon;

    [Header("Highlight")]
    public Renderer renderer;

    private Color defaultColor;

    public abstract string GetJsonData();
    public abstract void LoadJsonData(string json);


    protected virtual void Awake()
    {
        StartCoroutine(IEAwake());

        renderer = GetComponent<Renderer>();
        defaultColor = renderer.material.color;
    }

    private IEnumerator IEAwake()
    {
        while (!isClone) { yield return null; }

        id = gameObject.name;
        Debug.Log(gameObject.name);
        DataManager.Instance.BuildingManager.Register(this);
    }

    public void UpdateHighlight()
    {
        var gm = GameManager.Instance;

        // WorkMode 아니면 기본
        if (!gm.isWorkMode)
        {
            SetColor(defaultColor);
            return;
        }

        // 할당 불가능 건물 → 회색
        if (type == BuildingType.None ||
            type == BuildingType.Rocket ||
            type == BuildingType.Storage)
        {
            SetColor(Color.gray);
            return;
        }

        // 선택된 NPC
        var selected = gm.selectedNPC;

        // 선택 없음
        if (selected == null)
        {
            SetColor(defaultColor);
            return;
        }

        // 이 건물이 선택된 NPC에 포함?
        if (selected.job.buildingIDs.Contains(id))
        {
            SetColor(Color.green);
            return;
        }

        // 다른 NPC가 사용 중인지 체크
        if (DataManager.Instance.NPCManager.IsBuildingAssigned(id))
        {
            SetColor(Color.red);
            return;
        }

        // 기본
        SetColor(defaultColor);
    }

    void SetColor(Color color)
    {
        renderer.material.color = color;
    }

    public abstract void OnInteract();
}
