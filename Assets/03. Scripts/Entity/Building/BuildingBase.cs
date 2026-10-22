using UnityEngine;

public abstract class BuildingBase : MonoBehaviour, ILeftInteractable, IRightInteractable
{
    [Header("Building Info")]
    public BuildingType type = BuildingType.None;

    public int itemID;

    public int ClickPriority => ClickPriorities.Building;
    public ClickTarget Target => ClickTarget.Building;

    [HideInInspector]
    public string id;

    public BuildingData data;

    public Sprite icon;

    [Header("Optional Components")]
    [HideInInspector]
    public Inventory inventory;

    [Header("Highlight")]
    [Tooltip("비워두면 자신과 자식의 SpriteRenderer를 자동으로 수집")]
    [SerializeField] private SpriteRenderer[] highlightRenderers;

    // highlightRenderers와 같은 순서로 저장된 각 렌더러의 기본색
    private Color[] defaultColors;

    #region Unity

    protected virtual void Awake()
    {
        CacheHighlightRenderers();

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

    // 하이라이트 대상 렌더러와 기본색을 저장 (자식이 런타임에 추가되면 다시 호출)
    public void CacheHighlightRenderers()
    {
        if (highlightRenderers == null || highlightRenderers.Length == 0)
            highlightRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        defaultColors = new Color[highlightRenderers.Length];

        for (int i = 0; i < highlightRenderers.Length; i++)
            defaultColors[i] = highlightRenderers[i].color;
    }

    public void UpdateHighlight()
    {
        Color? color = GetHighlightColor();

        if (color.HasValue)
            ApplyColor(color.Value);
        else
            RestoreColors();
    }

    // 현재 상태에 맞는 하이라이트 색 (null이면 기본색)
    private Color? GetHighlightColor()
    {
        GameManager gm = GameManager.Instance;

        // WorkMode 아니면 기본색
        if (!gm.IsMode(GameMode.Work))
            return null;

        // 할당 불가능 건물
        if (!CanAssignWork())
            return Color.gray;

        NPC selected = gm.selectedNPC;

        // 선택된 NPC 없음
        if (selected == null)
            return null;

        // 선택된 NPC가 사용 중인 건물
        if (selected.job.buildingIDs.Contains(id))
            return Color.green;

        // 다른 NPC가 사용 중
        if (DataManager.Instance.NPCManager.IsBuildingAssigned(id))
            return Color.red;

        return null;
    }

    // 모든 렌더러(자식 포함)에 색 적용
    private void ApplyColor(Color tint)
    {
        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            if (highlightRenderers[i] == null)
                continue;

            // 각 렌더러의 기본색에 곱해서 알파 등 원래 값을 유지
            highlightRenderers[i].color = defaultColors[i] * tint;
        }
    }

    // 모든 렌더러를 각자의 기본색으로 복원
    private void RestoreColors()
    {
        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            if (highlightRenderers[i] == null)
                continue;

            highlightRenderers[i].color = defaultColors[i];
        }
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

    #endregion

    #region DestroyBuilding
    
    // 처음 기본으로 주어진 건물인가? <= 파괴 불가(로켓, 기본 창고, 아이템이 들어 있는 창고)
    // 기본값 : 파괴 가능
    [HideInInspector] public bool isDefaultBuilding = false;

    // GetDestroyBlockReason == null 일 경우 파괴가능
    public virtual string GetDestroyBlockReason() => null;
    public bool CanDestroy => GetDestroyBlockReason() == null;

    // 파괴 직전 정리 훅 (건물별로 override)
    public virtual void OnBeforeDestroy() { }

    #endregion

    #region Save

    // 하위 건물
    // 저장이 필요한 건물 : 로켓, 밭
    // 저장이 필요 없는 건물 : 창고 (인벤토리가 저장), 목장 (동물 쪽에서 저장)
    public virtual string GetJsonData() => "";

    public virtual void LoadJsonData(string json) { }

    #endregion

    #region Interface

    public virtual void OnLInteract()
    {
        var gm = GameManager.Instance;

        if (gm.IsMode(GameMode.Build))
            OnBuildInteract();
        else if (gm.IsMode(GameMode.Work))
            OnWorkInteract();
        else if (gm.IsMode(GameMode.None))
            OnNormalInteract();
        // 그 외 모드는 아무 동작 없음
    }

    public virtual void OnBuildInteract()
    {
        Debug.Log($"Building {name} Build Interact");

        // 건물을 설치하는 중(미리보기)에는 파괴 메뉴를 열지 않음
        if (DataManager.Instance.BuildingManager.IsPlacing)
            return;

        // 파괴할 수 없는 건물도 메뉴는 띄움 (버튼이 꺼지고 이유가 표시됨)
        UIBuildingMenu.Instance.Show(this);
    }

    public virtual void OnWorkInteract()
    {
        Debug.Log($"Building {name} Work Interact");

        if (data == null)
        {
            Debug.LogWarning($"{name}: BuildingData가 지정되지 않았습니다.");
            return;
        }

        NPC selectedNPC = GameManager.Instance.selectedNPC;

        if (selectedNPC == null)
            return;

        // 작업 불가능 건물
        if (!CanAssignWork())
            return;

        // 이미 등록된 건물이면 해제
        if (selectedNPC.job.buildingIDs.Contains(id))
        {
            selectedNPC.job.buildingIDs.Remove(id);

            // 마지막 건물 제거 시 직업 초기화
            if (selectedNPC.job.buildingIDs.Count == 0)
                selectedNPC.job.jobType = JobType.None;

            selectedNPC.JobChanged();
            GameManager.Instance.RefreshHighlights();
            return;
        }

        // 다른 NPC가 사용 중
        if (DataManager.Instance.NPCManager.IsBuildingAssigned(id))
            return;

        // ---- 검증: 상태를 바꾸기 전에 실패 조건을 모두 확인 ----

        // 직업이 없으면 이 건물의 종류를 따라가므로 통과, 있으면 같은 종류만 가능
        bool hasJob = selectedNPC.job.jobType != JobType.None;

        if (hasJob && selectedNPC.job.jobType != data.jobType)
            return;

        // 작업 슬롯 확인
        int usedSlots = selectedNPC.job.GetUsedSlots();
        int cost = GetWorkSlotCost();

        if (usedSlots + cost > selectedNPC.job.maxWorkSlots)
        {
            Debug.Log("작업 슬롯 부족");
            return;
        }

        // ---- 변경: 모든 검증을 통과한 뒤에만 실행 ----

        if (!hasJob)
            selectedNPC.job.jobType = data.jobType;

        selectedNPC.job.buildingIDs.Add(id);
        selectedNPC.JobChanged();

        GameManager.Instance.RefreshHighlights();
    }

    public virtual void OnNormalInteract()
    {
        Debug.Log($"Building {name} Normal Interact");
    }

    public abstract void OnRInteract();

    #endregion
}