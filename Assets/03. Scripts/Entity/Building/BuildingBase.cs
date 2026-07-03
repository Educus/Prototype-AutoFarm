using UnityEngine;

public abstract class BuildingBase : MonoBehaviour, ILeftInteractable
{
    [Header("Building Info")]
    public BuildingType type = BuildingType.None;

    public int itemID;

    [HideInInspector]
    public string id;

    public BuildingData data;

    public Sprite icon;

    [Header("Optional Components")]
    [HideInInspector]
    public Inventory inventory;

    [Header("Highlight")]
    [SerializeField] private Renderer cachedRenderer;

    private Color defaultColor;

    #region Unity

    protected virtual void Awake()
    {
        // Renderer 자동 캐싱
        if (cachedRenderer == null)
        {
            cachedRenderer = GetComponent<Renderer>();
        }

        // 기본 색 저장
        if (cachedRenderer != null)
        {
            defaultColor = cachedRenderer.material.color;
        }

        // Inventory는 optional
        inventory = GetComponent<Inventory>();
    }

    #endregion

    #region Initialize

    // Instantiate 직후 직접 호출
    public virtual void Initialize()
    {
        // 생성된 이름 사용
        id = gameObject.name;

        // 건물 등록
        DataManager.Instance.BuildingManager.Register(this);
    }

    #endregion

    #region Highlight
    public void UpdateHighlight()
    {
        GameManager gm = GameManager.Instance;

        // WorkMode 아니면 기본색
        if (!gm.IsMode(GameMode.Work))
        {
            SetColor(defaultColor);
            return;
        }

        // 할당 불가능 건물
        if (!CanAssignWork())
        {
            SetColor(Color.gray);
            return;
        }

        // 선택된 NPC
        var selected = gm.selectedNPC;

        // 선택 안 된 상태
        if (selected == null)
        {
            SetColor(defaultColor);
            return;
        }

        // 선택된 NPC가 사용 중인 건물
        if (selected.job.buildingIDs.Contains(id))
        {
            SetColor(Color.green);
            return;
        }

        // 다른 NPC가 사용 중
        if (DataManager.Instance.NPCManager.IsBuildingAssigned(id))
        {
            SetColor(Color.red);
            return;
        }

        // 기본색
        SetColor(defaultColor);
    }
    protected virtual bool CanAssignWork()
    {
        return type != BuildingType.None &&
               type != BuildingType.Rocket &&
               type != BuildingType.Storage;
    }

    public virtual int GetWorkSlotCost()
    {
        return data?.workSlotCost ?? 0;
    }

    private void SetColor(Color color)
    {
        if (cachedRenderer == null)
            return;

        cachedRenderer.material.color = color;
    }

    public virtual void OnWorkInteract()
    {
        NPC selectedNPC =
            GameManager.Instance.selectedNPC;

        if (selectedNPC == null)
            return;

        // 작업 불가능 건물
        if (!CanAssignWork())
            return;

        // 이미 등록된 건물
        if (selectedNPC.job.buildingIDs.Contains(id))
        {
            selectedNPC.job.buildingIDs.Remove(id);

            // 마지막 건물 제거 시 직업 초기화
            if (selectedNPC.job.buildingIDs.Count == 0)
            {
                selectedNPC.job.jobType = JobType.None;
            }

            GameManager.Instance.RefreshHighlights();

            return;
        }

        // 다른 NPC가 사용중
        if (DataManager.Instance
            .NPCManager
            .IsBuildingAssigned(id))
        {
            return;
        }

        // 직업 미설정
        if (selectedNPC.job.jobType == JobType.None)
        {
            Debug.Log($"Building : {name}, JobType : {data.jobType}");

            selectedNPC.job.jobType =
                data.jobType;
        }

        // 다른 종류 작업 불가
        if (selectedNPC.job.jobType != data.jobType)
        {
            return;
        }

        int usedSlots =
            selectedNPC.job.GetUsedSlots();

        int cost =
            data.workSlotCost;

        if (usedSlots + cost >
            selectedNPC.job.maxWorkSlots)
        {
            Debug.Log("작업 슬롯 부족");

            return;
        }

        selectedNPC.job.buildingIDs.Add(id);
        selectedNPC.JobChanged();

        GameManager.Instance.RefreshHighlights();
    }
    #endregion

    #region Abstract

    public abstract string GetJsonData();

    public abstract void LoadJsonData(string json);

    public abstract void OnInteract();

    #endregion
}