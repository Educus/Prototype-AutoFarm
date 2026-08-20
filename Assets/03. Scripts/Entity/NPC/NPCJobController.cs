using System.Collections.Generic;
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

    #region Farm 작업 상태
    private enum FarmAction
    {
        Harvest,
        Plant,
        Water
    }

    private FarmAction farmAction;

    private int currentTileIndex;

    private FarmBuilding currentFarm;
    private FarmTile currentTile;
    private FarmTileView currentTileView;

    // 작업 요청된 밭
    private Queue<FarmBuilding> farmWorkQueue = new Queue<FarmBuilding>();

    // 작업Queue에 들어간 밭
    private HashSet<FarmBuilding> queuedFarms = new HashSet<FarmBuilding>();

    // 이미 이벤트를 구독한 밭
    private HashSet<FarmBuilding> subscribedFarms = new HashSet<FarmBuilding>();

    // 현재 작업 중인 밭이 있는지
    private bool isWorkingFarm;

    // 오늘 씨앗이 더 이상 없는 상태
    private bool noMoreSeedsToday;
    #endregion

    // =========================
    // Ranch 작업 상태
    // =========================

    private int currentRanchIndex;

    #region Unity

    private void Awake()
    {
        npc = GetComponent<NPC>();

        npc.OnJobChanged += OnJobChanged;
    }

    private void Start()
    {
        OnJobChanged();
    }

    private void Update()
    {
        // 쉬는 중이면 작업 안 함
        if (npc.job.step == JobStep.Rest)
            return;

        ProcessJob();
    }

    private void OnDestroy()
    {
        npc.OnJobChanged -= OnJobChanged;

        UnsubscribeAllFarmEvents();
    }

    private void OnJobChanged()
    {
        npc.StopMove();

        // 기존 작업 Queue 제거
        ClearFarmWorkQueue();

        // 기존 Farm 이벤트 해제
        UnsubscribeAllFarmEvents();

        // 현재 작업 상태 초기화
        ResetWorkState();

        // 현재 Job이 Farm이라면 새로 등록
        if (npc.job.jobType == JobType.Farm)
        {
            SubscribeFarmEvents();
        }

        // 유효한 직업이면 다시 작업 시작
        if (npc.job.IsValid())
        {
            npc.job.step = JobStep.Idle;
        }
        else
        {
            npc.job.step = JobStep.Rest;
        }
    }
    #endregion

    #region 행동 시간 처리
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
    #endregion


    #region 직업 및 날짜 변경
    // Job 변경 시 작업 상태 초기화
    private void ResetWorkState()
    {
        currentTileIndex = 0;

        currentFarm = null;
        currentTile = null;
        currentTileView = null;

        noMoreSeedsToday = false;

        actionTimer = 0f;

        currentRanchIndex = 0;

        npc.targetAnimal = null;

        targetStorage = null;

        isWorkingFarm = false;
    }

    // 하루가 바뀌면 작업 상태 초기화
    private void ResetDailyWork()
    {
        ClearFarmWorkQueue();
        ResetWorkState();

        if (npc.job.IsValid())
        {
            npc.job.step = JobStep.Idle;
        }
    }
    #endregion

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

    #region Farm 구독 및 Queue 관리
    // Job이 Farm일 때 이벤트 구독 해제
    private void ClearFarmWorkQueue()
    {
        farmWorkQueue.Clear();
        queuedFarms.Clear();
    }

    // Job이 Farm일 때 할당 해제된 건물의 이벤트 구독 해제
    private void UnsubscribeAllFarmEvents()
    {
        foreach (FarmBuilding farm in subscribedFarms)
        {
            if (farm == null)
                continue;

            farm.onWorkRequested -= OnFarmWorkRequested;
        }

        subscribedFarms.Clear();
    }

    // Job이 Farm일 때 이벤트 구독
    private void SubscribeFarmEvents()
    {
        if (npc.job.jobType != JobType.Farm)
            return;

        foreach (string buildingID in npc.job.buildingIDs)
        {
            FarmBuilding farm =
                DataManager.Instance.BuildingManager
                .Get<FarmBuilding>(buildingID);

            if (farm == null)
                continue;

            // 중복 구독 방지
            if (subscribedFarms.Contains(farm))
                continue;

            farm.onWorkRequested += OnFarmWorkRequested;

            subscribedFarms.Add(farm);

            // 이미 작업이 존재하는 Farm이면
            // 이벤트를 기다리지 말고 바로 Queue 등록
            if (farm.HasPendingWork())
            {
                EnqueueFarm(farm);
            }
        }
    }

    private void OnFarmWorkRequested(FarmBuilding farm)
    {
        EnqueueFarm(farm);

        // 쉬고 있었다면 다시 작업 시작
        if (npc.job.step == JobStep.Rest)
        {
            npc.job.step = JobStep.Idle;
        }
    }

    // Queue에 Farm을 넣는 함수
    private void EnqueueFarm(FarmBuilding farm)
    {
        if (farm == null)
            return;

        // 현재 작업 중인 Farm
        // → Queue에 넣지 않는다.
        // 현재 작업을 끝내면서 다시 확인한다.
        if (farm == currentFarm)
            return;

        // 담당 Farm인지 확인
        if (!npc.job.buildingIDs.Contains(farm.id))
            return;

        // 이미 Queue에 있으면 중복 방지
        if (queuedFarms.Contains(farm))
            return;

        // 실제 작업이 없다면 넣지 않음
        if (!farm.HasPendingWork())
            return;

        farmWorkQueue.Enqueue(farm);
        queuedFarms.Add(farm);
    }
    #endregion

    #region Farm
    private void ProcessFarm()
    {
        switch (npc.job.step)
        {
            case JobStep.Idle:

                if (!StartNextFarm())
                {
                    npc.job.step = JobStep.Rest;
                    return;
                }

                break;

            case JobStep.MoveToStorage:

                if (npc.isMoving)
                    return;

                targetStorage = FindNearestStorageWithSeed();

                // 창고에 씨앗이 있으면 가지러 감
                if (targetStorage != null)
                {
                    Vector2Int pos =
                        GridManager.Instance.WorldToGrid(
                            targetStorage.transform.position);

                    npc.MoveTo(pos);

                    npc.job.step = JobStep.TakeResource;
                    return;
                }

                // 창고에는 없음
                // 인벤토리에 있으면 그대로 작업
                if (npc.subInventory.ContainsItem(npc.job.productItemID))
                {
                    npc.job.step = JobStep.FindFarmTile;
                    return;
                }

                // 둘 다 없음
                noMoreSeedsToday = true;
                npc.job.step = JobStep.FindFarmTile;

                break;

            case JobStep.TakeResource:

                if (npc.isMoving)
                    return;

                if (!CanDoAction())
                    return;

                TakeSeed(targetStorage);

                targetStorage = null;

                npc.job.step = JobStep.FindFarmTile;

                break;

            case JobStep.FindFarmTile:

                if (!FindNextFarmTile())
                {
                    FinishCurrentFarm();
                }

                break;

            case JobStep.MoveToFarmTile:

                if (npc.isMoving)
                    return;

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

    private bool StartNextFarm()
    {
        while (farmWorkQueue.Count > 0)
        {
            FarmBuilding farm = farmWorkQueue.Dequeue();

            queuedFarms.Remove(farm);

            if (farm == null)
                continue;

            // 담당 Farm인지 확인
            if (!npc.job.buildingIDs.Contains(farm.id))
                continue;

            // 작업이 이미 끝났다면 다음 Farm
            if (!farm.HasPendingWork())
                continue;

            currentFarm = farm;
            currentTileIndex = 0;
            currentTile = null;
            currentTileView = null;

            isWorkingFarm = true;

            npc.job.step = JobStep.MoveToStorage;

            return true;
        }

        currentFarm = null;
        isWorkingFarm = false;

        return false;
    }

    private void TakeSeed(StorageBuilding storage)
    {
        if (storage == null)
        {
            noMoreSeedsToday = true;
            return;
        }

        // 작업에 사용하지 않는 씨앗 먼저 적재
        TryDepositToStorage(storage);

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
        if (currentFarm == null)
            return false;

        // =================================
        // 1. 수확
        // =================================

        var harvestTiles =
            currentFarm.GetHarvestableTiles();

        if (harvestTiles.Count > 0)
        {
            currentTile = harvestTiles[0];
            farmAction = FarmAction.Harvest;

            MoveToCurrentFarmTile();

            return true;
        }

        // =================================
        // 2. 심기
        // =================================

        if (!noMoreSeedsToday)
        {
            var plantTiles =
                currentFarm.GetPlantableTiles();

            if (plantTiles.Count > 0)
            {
                currentTile = plantTiles[0];
                farmAction = FarmAction.Plant;

                MoveToCurrentFarmTile();

                return true;
            }
        }

        // =================================
        // 3. 물주기
        // =================================

        var waterTiles =
            currentFarm.GetWaterableTiles();

        if (waterTiles.Count > 0)
        {
            currentTile = waterTiles[0];
            farmAction = FarmAction.Water;

            MoveToCurrentFarmTile();

            return true;
        }

        return false;
    }

    private void MoveToCurrentFarmTile()
    {
        if (currentTile == null)
            return;

        int index =
            currentFarm.tiles.IndexOf(currentTile);

        if (index < 0 ||
            index >= currentFarm.tileViews.Count)
            return;

        currentTileView =
            currentFarm.tileViews[index];

        Vector2Int target =
            GridManager.Instance.WorldToGrid(
                currentTileView.transform.position);

        npc.MoveTo(target);

        npc.job.step =
            JobStep.MoveToFarmTile;
    }

    private void WorkCurrentFarmTile()
    {
        if (!CanDoAction())
            return;

        switch (farmAction)
        {
            // =========================
            // Harvest
            // =========================

            case FarmAction.Harvest:

                if (currentTile != null &&
                    currentTile.IsReady())
                {
                    int item =
                        currentFarm.TryHarvest(currentTile);

                    if (item > 0)
                    {
                        npc.AddItemToInventory(item, 1);
                    }
                }

                // 수확 후 다시 탐색
                npc.job.step = JobStep.FindFarmTile;

                break;


            // =========================
            // Plant
            // =========================

            case FarmAction.Plant:

                if (currentTile != null &&
                    currentTile.CanPlant())
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

                            // =========================
                            // 심었으면 바로 물주기
                            // =========================

                            currentFarm.TryWater(currentTile);

                            // 씨앗이 모두 떨어졌는지 확인
                            if (!npc.subInventory.ContainsItem(
                                npc.job.productItemID))
                            {
                                noMoreSeedsToday = true;
                            }
                        }
                        else
                        {
                            noMoreSeedsToday = true;
                        }
                    }
                }

                npc.job.step = JobStep.FindFarmTile;

                break;


            // =========================
            // Water
            // =========================

            case FarmAction.Water:

                if (currentTile != null &&
                    currentTile.hasCrop &&
                    !currentTile.IsReady() &&
                    !currentTile.watered)
                {
                    currentFarm.TryWater(currentTile);
                }

                npc.job.step = JobStep.FindFarmTile;

                break;
        }
    }

    private void FinishCurrentFarm()
    {
        // 현재 Farm에 새로운 작업이 생겼다면
        // Queue 다음 Farm으로 가지 않고 현재 Farm을 다시 처리
        if (currentFarm != null &&
            currentFarm.HasPendingWork())
        {
            currentTile = null;
            currentTileView = null;
            currentTileIndex = 0;

            npc.job.step = JobStep.FindFarmTile;
            return;
        }

        currentFarm = null;
        currentTile = null;
        currentTileView = null;

        if (!StartNextFarm())
        {
            npc.job.step = JobStep.ReturnToStorage;
        }
    }

    private StorageBuilding FindNearestStorageWithSeed()
    {
        Debug.Log($"찾는 아이템 : {npc.job.productItemID}");

        foreach (BuildingBase building in
         DataManager.Instance.BuildingManager.GetAll())
        {
            StorageBuilding storage =
                building as StorageBuilding;

            if (storage == null)
                continue;

            Debug.Log($"창고 발견 : {storage.id}");

            foreach (var slot in storage.inventory.slots)
            {
                Debug.Log(
                    $"item={slot.itemID} count={slot.count}");
            }
        }

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

        // 적재할 아이템이 없다면 쉬기
        if (!HasItemToDeposit())
        {
            npc.job.step = JobStep.Rest;
            return;
        }

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

        if (!TryDepositToStorage(targetStorage))
        {
            targetStorage = null;
            npc.job.step = JobStep.Rest;
            return;
        }

        targetStorage = null;

        npc.job.step = JobStep.Rest;
    }

    private bool DepositToStorage(Inventory storage)
    {
        bool deposited = false;

        foreach (var slot in npc.mainInventory.slots)
        {
            if (slot.itemID <= 0)
                continue;

            int added = storage.AddItem(
                    slot.itemID,
                    slot.count,
                    slot.remainingStoragePeriod);

            if (added > 0)
                deposited = true;

            slot.count -= added;

            if (slot.count <= 0)
            {
                slot.Clear();
            }
        }

        // 씨앗을 적제(사용X)
        foreach (var slot in npc.subInventory.slots)
        {
            if (slot.itemID <= 0)
                continue;

            if (slot.itemID == npc.job.productItemID)
                continue;
        
            int remain = storage.AddItem(
                    slot.itemID,
                    slot.count,
                    slot.remainingStoragePeriod);

            if (remain > 0)
                deposited = true;

            slot.count -= remain;
        
            if (slot.count <= 0)
            {
                slot.Clear();
            }
        }

        if (deposited)
        {
            storage.InvokeChange();
            npc.mainInventory.InvokeChange();
            npc.subInventory.InvokeChange();
        }

        return deposited;
    }

    private bool TryDepositToStorage(StorageBuilding storage)
    {
        while (HasItemToDeposit())
        {
            if (storage == null)
                return false;

            bool deposited = DepositToStorage(storage.inventory);

            // 하나도 못 넣었으면 다른 창고 탐색
            if (!deposited)
            {
                storage = FindNearestAvailableStorage(storage);
                continue;
            }

            // 모두 적재 완료
            if (!HasItemToDeposit())
                return true;

            // 아직 남았다면 다른 창고 탐색
            storage = FindNearestAvailableStorage(storage);
        }

        return true;
    }

    private bool HasItemToDeposit()
    {
        // 보유한 농작물이 있는가?
        foreach (var slot in npc.mainInventory.slots)
        {
            if (slot.itemID > 0)
                return true;
        }

        // 보유한 씨앗이 있는가?
        foreach (var slot in npc.subInventory.slots)
        {
            // 작업중인 씨앗 제외
            float slotItemID = slot.itemID;

            if (slotItemID > 0 && slotItemID != npc.job.productItemID)
                return true;
        }

        return false;
    }

    private StorageBuilding FindNearestAvailableStorage(StorageBuilding ignoreStorage = null)
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

            // 사용한 창고는 제외
            if (storage == ignoreStorage)
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