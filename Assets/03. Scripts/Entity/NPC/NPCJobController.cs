using System.Linq;
using UnityEngine;

public class NPCJobController : MonoBehaviour
{
    [Header("Reference")]
    public NPC npc;

    [Header("Action")]
    [SerializeField]
    private float actionDuration = 1f;

    private float actionTimer;

    // 현재 목표 창고
    private StorageBuilding targetStorage;

    // =========================
    // Farm 작업 상태
    // =========================
    private enum FarmAction
    {
        Harvest,
        Plant,
        Water
    }

    private FarmAction farmAction;

    private int currentFarmIndex;
    private int currentTileIndex;

    private FarmBuilding currentFarm;
    private FarmTile currentTile;
    private FarmTileView currentTileView;

    // 오늘 씨앗이 더 이상 없는 상태
    private bool noMoreSeedsToday;

    // =========================
    // Ranch 작업 상태
    // =========================

    private int currentRanchIndex;

    // =========================
    // Unity
    // =========================

    private void Awake()
    {
        npc = GetComponent<NPC>();
    }

    private void Start()
    {
        // 날짜가 바뀌면 다시 일을 시작할 수 있도록 초기화
        TimeManager.Instance.onDayEvent += ResetDailyWork;
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.onDayEvent -= ResetDailyWork;
        }
    }

    private void Update()
    {
        // 쉬는 중이면 작업 안 함
        if (npc.job.step == JobStep.Rest)
            return;

        ProcessJob();
    }

    // =========================
    // 행동 시간 처리
    // =========================

    /// <summary>
    /// 행동 애니메이션 및 작업 시간 대기
    /// true : 행동 가능
    /// false : 아직 대기 중
    /// </summary>
    private bool CanDoAction()
    {
        actionTimer += Time.deltaTime;

        if (actionTimer < actionDuration)
            return false;

        actionTimer = 0f;
        return true;
    }

    // =========================
    // 하루 초기화
    // =========================

    /// <summary>
    /// 자정 이후 호출
    /// 작물 성장 및 동물 생산 준비 이후 실행됨
    /// Rest → Idle 전환
    /// </summary>
    private void ResetDailyWork()
    {
        currentFarmIndex = 0;
        currentTileIndex = 0;

        currentFarm = null;
        currentTile = null;
        currentTileView = null;

        currentRanchIndex = 0;

        npc.targetAnimal = null;

        noMoreSeedsToday = false;

        actionTimer = 0f;

        // 유효한 직업이 있다면 다시 작업 시작
        if (npc.job.IsValid())
        {
            npc.job.step = JobStep.Idle;
        }
    }

    // =========================
    // Job 처리
    // =========================

    private void ProcessJob()
    {
        if (!npc.job.IsValid())
            return;

        switch (npc.job.jobType)
        {
            case JobType.Farm:
                ProcessFarm();
                break;

            case JobType.Ranch:
                ProcessRanch();
                break;
        }
    }

    #region Farm
    private void ProcessFarm()
    {
        switch (npc.job.step)
        {
            case JobStep.Idle:

                currentFarmIndex = 0;
                currentTileIndex = 0;

                currentFarm = null;
                currentTile = null;
                currentTileView = null;

                noMoreSeedsToday = false;

                npc.job.step = JobStep.MoveToStorage;

                break;

            case JobStep.MoveToStorage:

                if (npc.isMoving)
                    return;

                StorageBuilding storage =
                    FindNearestStorageWithSeed();

                // 씨앗이 없어도 작업은 진행
                if (storage == null)
                {
                    noMoreSeedsToday = true;
                    npc.job.step = JobStep.FindFarmTile;
                    return;
                }

                Vector2Int pos =
                    GridManager.Instance.WorldToGrid(
                        storage.transform.position);

                npc.MoveTo(pos);

                npc.job.step = JobStep.TakeResource;

                break;

            case JobStep.TakeResource:

                if (npc.isMoving)
                    return;

                if (!CanDoAction())
                    return;

                TakeSeed();

                npc.job.step = JobStep.FindFarmTile;

                break;

            case JobStep.FindFarmTile:

                if (!FindNextFarmTile())
                {
                    npc.job.step = JobStep.ReturnToStorage;
                }

                break;

            case JobStep.MoveToFarmTile:

                if (npc.isMoving)
                    return;

                farmAction = FarmAction.Harvest;

                npc.job.step = JobStep.WorkFarmTile;

                break;

            case JobStep.WorkFarmTile:

                WorkCurrentFarmTile();

                break;

            case JobStep.ReturnToStorage:

                HandleReturnToStorage();

                break;

            case JobStep.DepositItems:

                HandleDepositItems();

                break;
        }
    }
    private void TakeSeed()
    {
        StorageBuilding storage = FindNearestStorageWithSeed();

        if (storage == null)
        {
            noMoreSeedsToday = true;
            return;
        }

        int canCarry = npc.subInventory
            .GetAddableAmount(npc.job.productItemID);

        if (canCarry <= 0)
        {
            noMoreSeedsToday = false;
            return;
        }

        int taken = storage.inventory.TakeUpTo(
            npc.job.productItemID,
            canCarry);

        if (taken <= 0)
        {
            noMoreSeedsToday = true;
            return;
        }

        npc.subInventory.AddItem(
            npc.job.productItemID,
            taken,
            -1);

        noMoreSeedsToday = false;
    }

    private bool FindNextFarmTile()
    {
        while (currentFarmIndex < npc.job.buildingIDs.Count)
        {
            currentFarm =
                DataManager.Instance.BuildingManager
                .Get<FarmBuilding>(
                    npc.job.buildingIDs[currentFarmIndex]);

            if (currentFarm == null)
            {
                currentFarmIndex++;
                currentTileIndex = 0;
                continue;
            }

            while (currentTileIndex < currentFarm.tiles.Count)
            {
                currentTile =
                    currentFarm.tiles[currentTileIndex];

                currentTileView =
                    currentFarm.tileViews[currentTileIndex];

                Vector2Int target =
                    GridManager.Instance.WorldToGrid(
                        currentTileView.transform.position);

                npc.MoveTo(target);

                npc.job.step =
                    JobStep.MoveToFarmTile;

                return true;
            }

            currentFarmIndex++;
            currentTileIndex = 0;
        }

        return false;
    }

    private void WorkCurrentFarmTile()
    {
        if (!CanDoAction())
            return;

        switch (farmAction)
        {
            case FarmAction.Harvest:

                if (currentTile.IsReady())
                {
                    int item =
                        currentFarm.TryHarvest(currentTile);

                    if (item > 0)
                    {
                        npc.AddItemToInventory(item, 1);
                    }
                }

                farmAction = FarmAction.Plant;

                break;

            case FarmAction.Plant:

                if (currentTile.CanPlant())
                {
                    if (!noMoreSeedsToday)
                    {
                        int taken =
                            npc.subInventory.TakeUpTo(
                                npc.job.productItemID,
                                1);

                        if (taken > 0)
                        {
                            currentFarm.TryPlant(
                                currentTile,
                                npc.job.productItemID);
                        }
                        else
                        {
                            npc.job.step =
                                JobStep.MoveToStorage;

                            return;
                        }
                    }
                }

                farmAction = FarmAction.Water;

                break;

            case FarmAction.Water:

                if (currentTile.hasCrop &&
                    !currentTile.watered)
                {
                    currentFarm.Water(currentTile);
                }

                currentTileIndex++;

                npc.job.step =
                    JobStep.FindFarmTile;

                break;
        }
    }

    private StorageBuilding FindNearestStorageWithSeed()
    {
        StorageBuilding result = null;

        float bestDistance = float.MaxValue;

        foreach (BuildingBase building in
                 DataManager.Instance.BuildingManager.GetAll())
        {
            StorageBuilding storage =
                building as StorageBuilding;

            if (storage == null)
                continue;

            int seedCount = storage.inventory.slots
                .Where(s => s.itemID == npc.job.productItemID)
                .Sum(s => s.count);

            if (seedCount <= 0)
                continue;

            float distance = Vector2Int.Distance(
                npc.GetGridPos(),
                GridManager.Instance.WorldToGrid(
                    storage.transform.position));

            if (distance < bestDistance)
            {
                bestDistance = distance;
                result = storage;
            }
        }

        return result;
    }
    #endregion

    #region Ranch
    private void ProcessRanch()
    {
        switch (npc.job.step)
        {
            case JobStep.Idle:

                currentRanchIndex = 0;
                npc.targetAnimal = null;

                npc.job.step = JobStep.FindAnimal;

                break;

            case JobStep.FindAnimal:

                if (!FindNextAnimal())
                {
                    npc.job.step = JobStep.ReturnToStorage;
                }

                break;

            case JobStep.MoveToAnimal:

                if (npc.isMoving)
                    return;

                npc.job.step = JobStep.InteractAnimal;

                break;

            case JobStep.InteractAnimal:

                InteractCurrentAnimal();

                break;

            case JobStep.ReturnToStorage:

                HandleReturnToStorage();

                break;

            case JobStep.DepositItems:

                HandleDepositItems();

                break;
        }
    }

    private bool FindNextAnimal()
    {
        while (currentRanchIndex < npc.job.buildingIDs.Count)
        {
            RanchBuilding ranch =
                DataManager.Instance.BuildingManager
                .Get<RanchBuilding>(
                    npc.job.buildingIDs[currentRanchIndex]);

            if (ranch == null)
            {
                currentRanchIndex++;
                continue;
            }

            foreach (AnimalBase animal in ranch.animals)
            {
                if (animal == null)
                    continue;

                if (!animal.isReady)
                    continue;

                npc.targetAnimal = animal;

                Vector2Int target =
                    GridManager.Instance.WorldToGrid(
                        animal.transform.position);

                npc.MoveTo(target);

                npc.job.step =
                    JobStep.MoveToAnimal;

                return true;
            }

            currentRanchIndex++;
        }

        return false;
    }

    private void InteractCurrentAnimal()
    {
        if (npc.targetAnimal == null)
        {
            npc.job.step = JobStep.FindAnimal;
            return;
        }

        // 행동 애니메이션 시간
        if (!CanDoAction())
            return;

        int itemID =
            npc.targetAnimal.Harvest();

        if (itemID > 0)
        {
            npc.AddItemToInventory(itemID, 1);
        }

        npc.targetAnimal = null;

        npc.job.step = JobStep.FindAnimal;
    }


    #endregion

    #region 공통

    private void HandleReturnToStorage()
    {
        // 이동 중
        if (npc.isMoving)
            return;

        // 목표 창고가 아직 없다면 탐색
        if (targetStorage == null)
        {
            targetStorage =
                FindNearestAvailableStorage();

            // 모든 창고가 가득 참
            if (targetStorage == null)
            {
                npc.job.step = JobStep.Rest;
                return;
            }

            Vector2Int pos =
                GridManager.Instance.WorldToGrid(
                    targetStorage.transform.position);

            npc.MoveTo(pos);

            return;
        }

        // 이동 완료
        npc.job.step = JobStep.DepositItems;
    }

    private void HandleDepositItems()
    {
        if (targetStorage == null)
        {
            npc.job.step = JobStep.Rest;
            return;
        }

        // 적재 애니메이션
        if (!CanDoAction())
            return;

        DepositToStorage(targetStorage.inventory);

        // 아직 남아있다면 다른 창고 탐색
        if (HasItemToDeposit())
        {
            targetStorage =
                FindNearestAvailableStorage();

            if (targetStorage != null)
            {
                Vector2Int pos =
                    GridManager.Instance.WorldToGrid(
                        targetStorage.transform.position);

                npc.MoveTo(pos);

                npc.job.step =
                    JobStep.ReturnToStorage;

                return;
            }
        }

        targetStorage = null;

        npc.job.step = JobStep.Rest;
    }

    private void DepositToStorage(Inventory storage)
    {
        foreach (var slot in npc.mainInventory.slots)
        {
            if (slot.itemID <= 0)
                continue;

            int remain =
                storage.AddItem(
                    slot.itemID,
                    slot.count,
                    slot.remainingStoragePeriod);

            int deposited =
                slot.count - remain;

            slot.count -= deposited;

            if (slot.count <= 0)
            {
                slot.Clear();
            }
        }

        foreach (var slot in npc.subInventory.slots)
        {
            if (slot.itemID <= 0)
                continue;

            int remain =
                storage.AddItem(
                    slot.itemID,
                    slot.count,
                    slot.remainingStoragePeriod);

            int deposited =
                slot.count - remain;

            slot.count -= deposited;

            if (slot.count <= 0)
            {
                slot.Clear();
            }
        }
    }

    private bool HasItemToDeposit()
    {
        foreach (var slot in npc.mainInventory.slots)
        {
            if (slot.itemID > 0)
                return true;
        }

        foreach (var slot in npc.subInventory.slots)
        {
            if (slot.itemID > 0)
                return true;
        }

        return false;
    }

    private StorageBuilding FindNearestAvailableStorage()
    {
        StorageBuilding result = null;

        float bestDistance = float.MaxValue;

        foreach (BuildingBase building in
                 DataManager.Instance.BuildingManager.GetAll())
        {
            StorageBuilding storage =
                building as StorageBuilding;

            if (storage == null)
                continue;

            if (storage.inventory.IsFull())
                continue;

            float distance = Vector2Int.Distance(
                npc.GetGridPos(),
                GridManager.Instance.WorldToGrid(
                    storage.transform.position));

            if (distance < bestDistance)
            {
                bestDistance = distance;
                result = storage;
            }
        }

        return result;
    }
    #endregion
}