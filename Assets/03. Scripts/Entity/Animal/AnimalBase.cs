using System;
using UnityEngine;

public abstract class AnimalBase : MonoBehaviour, IRightInteractable
{
    // 이걸 부모로 쓰는 동물들
    // 동물 타입만 연결해주면 모두 연결, 행동 가능하게 만들기
    // 추가한다면 동물 설명?
    [HideInInspector] public RanchBuilding ranchBuilding = null; // 농장 건물 참조

    [HideInInspector] public int itemId;         // 아이디
    [HideInInspector] public string id;          // 아이디
    [HideInInspector] public string animalName;  // 동물 이름

    public AnimalType type;     // 동물 타입
    public int productItemID;   // 생산 아이템

    [Tooltip("생산시간(10분 단위)")]
    public int productionTime = 360;  // 생산 시간

    // 다음 생산까지 남은 시간
    private int remainingTime;

    // 하루 최대 수확 횟수
    [SerializeField]
    protected int dailyHarvestCount = 2;

    // 오늘 수확한 횟수
    protected int todayHarvestCount;

    // 현재 생산품 개수
    [HideInInspector]
    public int isStack;

    // 수확에 필요한 아이템
    [Header("Harvest")]
    public int harvestRequiredItemID = -1;

    #region Initialize

    public void Initialize(string animalID)
    {
        id = animalID;

        remainingTime = productionTime;

        gameObject.name = animalID;

        DataManager.Instance.AnimalManager.Register(this);

        TimeManager.Instance.onMinuteEvent += ReproducingItem;
        TimeManager.Instance.onDayEvent += ResetDailyProduction;
    }

    #endregion


    #region Unity

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.onMinuteEvent -= ReproducingItem;
            TimeManager.Instance.onDayEvent -= ResetDailyProduction;
        }
    }

    #endregion


    #region Interaction

    public void OnInteract(Player player)
    {
        Debug.Log("동물 클릭 됨");

        GameManager.Instance.player
            .GetComponent<PlayerAction>()
            .StartAnimalAction(this);
    }

    #endregion

    #region Production

    // 수확 가능한가?
    public bool IsReady()
    {
        return isStack > 0;
    }


    // 아이템 생산
    private void ReproducingItem(int minute)
    {
        // 아직 수확하지 않은 생산품이 있다면
        // 추가 생산하지 않음
        if (isStack > 0)
            return;

        remainingTime -= minute;
        Debug.Log($"동물 {minute}분 생산 시간 감소: {remainingTime}분 남음");
        
        if (remainingTime > 0)
            return;

        // 오늘 수확 가능 횟수를 모두 사용했다면 생산하지 않음
        if (todayHarvestCount >= dailyHarvestCount)
            return;

        // 생산품 생성
        isStack = 1;

        // 목장 건물이 있다면 작업 요청
        ranchBuilding?.RequestWork();

        // 다음 생산까지 쿨타임 초기화
        remainingTime = productionTime;
    }

    // 하루가 바뀌면 오늘 수확 횟수 초기화
    private void ResetDailyProduction(int day)
    {
        todayHarvestCount = 0;
    }

    // 수확
    public int Harvest(Player player)
    {
        // 생산품이 없으면 수확 불가
        if (isStack < 1)
            return -1;

        // 오늘 수확 횟수를 모두 사용했다면 수확 불가
        if (todayHarvestCount >= dailyHarvestCount)
            return -1;

        if (type == AnimalType.NONE)
            return -1;

        // 필요한 아이템 검사
        if (!HasRequiredItem(player))
            return -1;

        // 생산품 하나 수확
        isStack--;

        // 오늘 수확 횟수 증가
        todayHarvestCount++;

        return productItemID;
    }
    
    private bool HasRequiredItem(Player player)
    {
        // 필요한 아이템이 설정되어 있지 않다면
        if (harvestRequiredItemID < 0)
            return true;

        // 플레이어가 없다면
        if (player == null)
            return false;

        // 선택한 슬롯의 아이템 확인
        if (player.subInventory.slots[player.selectedSubSlotIndex].itemID == harvestRequiredItemID)
        {
            return true;
        }
        else
        {
            Debug.Log($"수확 아이템 없음 : {harvestRequiredItemID} 필요");
            return false;
        }

    }

    public int Harvest(NPC npc)
    {
        // 생산품이 없으면 수확 불가
        if (isStack < 1)
            return -1;

        // 오늘 수확 횟수를 모두 사용했다면 수확 불가
        if (todayHarvestCount >= dailyHarvestCount)
            return -1;

        if (type == AnimalType.NONE)
            return -1;

        // 필요한 아이템 검사
        if (!HasRequiredItem(npc))
            return -1;

        // 생산품 하나 수확
        isStack--;

        // 오늘 수확 횟수 증가
        todayHarvestCount++;

        return productItemID;
    }

    private bool HasRequiredItem(NPC npc)
    {
        // 필요한 아이템이 없으면 누구나 수확 가능
        if (harvestRequiredItemID < 0)
            return true;

        if (npc == null)
            return false;

        // NPC 인벤토리에 필요한 아이템이 있는지 검사
        if (npc.subInventory.ContainsItem(harvestRequiredItemID))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    // 할당된 건물이 없다면 마음대로 움직임 (추가 예정)


    #region Save / Load

    public AnimalSaveData GetSaveData()
    {
        return new AnimalSaveData
        {
            itemId = this.itemId,
            id = this.id,
            animalName = this.animalName,
            isStack = this.isStack,

            position = transform.position
        };
    }

    public void Load(AnimalSaveData data)
    {
        this.itemId = data.itemId;
        this.id = data.id;
        this.animalName = data.animalName;
        this.isStack = data.isStack;

        transform.position = data.position;

        remainingTime = productionTime;
    }

    #endregion
}
