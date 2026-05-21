using UnityEngine;

public abstract class BuildingBase : MonoBehaviour, IInteractable
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
        var gm = GameManager.Instance;

        // WorkMode 아니면 기본색
        if (!gm.isWorkMode)
        {
            SetColor(defaultColor);
            return;
        }

        // 할당 불가능 건물
        if (type == BuildingType.None ||
            type == BuildingType.Rocket ||
            type == BuildingType.Storage)
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

    private void SetColor(Color color)
    {
        if (cachedRenderer == null)
            return;

        cachedRenderer.material.color = color;
    }

    #endregion

    #region Abstract

    public abstract string GetJsonData();

    public abstract void LoadJsonData(string json);

    public abstract void OnInteract();

    #endregion
}