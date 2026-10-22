using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCJobController))]
public class NPC : MonoBehaviour, ILeftInteractable
{
    public int ClickPriority => ClickPriorities.Npc;
    public ClickTarget Target => ClickTarget.Npc;

    // NPC
    public string id;
    public string entityName;

    [Header("Water")]
    public int water;
    public int maxWater;

    [Header("Inventory")]
    public Inventory mainInventory;
    public Inventory subInventory;
    public Inventory upgradeInventory;

    [Header("Job")]
    public NPCJobConfig job = new NPCJobConfig();
    public AnimalBase targetAnimal;

    private GridManager gridManager;
    private Pathfinder pathfinder;

    private Coroutine moveCoroutine;

    //Sound 관련
    private SoundManager soundManager;
    private AudioSource SFX;

    //애니메이션 관련
    private NPCAnimation npcAnimation;

    public bool isMoving { get; private set; }


    public float moveSpeed = 3f;

    // 작업 및 할당 건물이 바뀌었을 때 작업 초기화
    public event Action OnJobChanged;

    // 상호작용
    public void OnLInteract()
    {
        if (GameManager.Instance.selectedNPC == this)
        {
            UIStorageManagement.Instance.NPCInv();
        }
        else
        {
            GameManager.Instance.selectedNPC = this;
            GameManager.Instance.targetLock = this.gameObject;
        }
    }

    // NPC 기본 세팅
    public void Initialize(string npcID)
    {
        id = npcID;

        gameObject.name = npcID;

        entityName = $"Prototype {DataManager.Instance.NPCManager.npcs.Count}";

        gridManager = GridManager.Instance;
        pathfinder = gridManager.Pathfinder;

        InitializeInventories();

        // NPC 등록
        DataManager.Instance.NPCManager.Register(this);

        //애니메이션 스크립트 초기화
        npcAnimation = GetComponent<NPCAnimation>();

        //사운드 관련 초기화
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        SFX = GetComponent<AudioSource>();
    }

    private void InitializeInventories()
    {
        // ID는 반드시 고유해야 함 (Save/Load 기준)
        mainInventory.id = $"{id}_main";
        subInventory.id = $"{id}_sub";
        upgradeInventory.id = $"{id}_upgrade";

        mainInventory.type = InventoryType.Main;
        subInventory.type = InventoryType.Sub;
        upgradeInventory.type = InventoryType.Upgrade;

        // 슬롯 초기화 (기본값)
        if (mainInventory.slots.Count == 0)
            mainInventory.Initialize(12);

        if (subInventory.slots.Count == 0)
            subInventory.Initialize(3);

        if (upgradeInventory.slots.Count == 0)
            upgradeInventory.Initialize(5);

        // InventoryManager 등록
        InventoryManager invManager =
            DataManager.Instance.InventoryManager;

        invManager.Register(mainInventory);
        invManager.Register(subInventory);
        invManager.Register(upgradeInventory);
    }

    public void SetName(string name)
    {
        entityName = name;
    }
    public string GetName()
    {
        return entityName;
    }

    public void JobChanged()
    {
        OnJobChanged?.Invoke();
    }

    #region 업그레이드 관리
    public bool HasUpgrade(int itemID)
    {
        return upgradeInventory.ContainsItem(itemID);
    }
    public bool AddUpgrade(int itemID)
    {
        return upgradeInventory.AddItem(itemID, 1, -1) > 0;
    }
    public void RemoveUpgrade(int itemID)
    {
        upgradeInventory.TakeUpTo(itemID, 1);
    }
    #endregion

    #region 물 관리
    public void AddWater(int amount)
    {
        water = Mathf.Min(water + amount, maxWater);
    }

    public bool UseWater(int amount)
    {
        if (water < amount)
            return false;

        water -= amount;
        return true;
    }
    #endregion

    #region 이동
    public void MoveTo(Vector2Int target)
    {
        // 이동 중이면 무시
        if (isMoving) return;

        List<Node> path = pathfinder.FindPath(GetGridPos(), target);

        if (path == null || path.Count == 0)
            return;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveAlongPath(path));
    }

    public void StopMove()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isMoving = false;

        // 코루틴 중간에 멈췄을 때 애니메이션 상태를 초기화
        if (npcAnimation != null)
            npcAnimation.IsMoving = false;
    }

    IEnumerator MoveAlongPath(List<Node> path)
    {
        isMoving = true;
        npcAnimation.IsMoving = true;

        foreach (Node node in path)
        {
            Vector3 target = gridManager.GetStandPosition(node.x, node.y);

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                Vector2 direction = (target - transform.position).normalized;

                npcAnimation.MoveDirection = direction;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            // 정확한 위치로 보정
            transform.position = target;
        }

        npcAnimation.IsMoving = false;

        //코루틴 종료 시점에 SFX 종료
        soundManager.StopSFX(SFX);
        isMoving = false;
        moveCoroutine = null;
    }

    // 위치 매번 계산
    public Vector2Int GetGridPos()
    {
        // 초기화 시 오류 방지로 GridManager.Instance를 직접 호출
        return GridManager.Instance.CharacterToGrid(transform.position);
    }
    #endregion

    #region 인벤토리 접근 방식
    public int TakeItemFromInventory(int itemID, int amount)
    {
        // 메인 → 서브 순으로 가져감
        int taken = mainInventory.TakeUpTo(itemID, amount);

        if (taken < amount)
        {
            taken += subInventory.TakeUpTo(itemID, amount - taken);
        }

        return taken;
    }

    public int AddItemToInventory(int itemID, int amount)
    {
        int remainingStoragePeriod = -1;

        if (DataManager.Instance.itemsData.ContainsKey(itemID))
        {
            remainingStoragePeriod =
                DataManager.Instance.itemsData[itemID].storagePeriod;
        }

        int added = mainInventory.AddItem(
            itemID,
            amount,
            remainingStoragePeriod);

        if (added < amount)
        {
            added += subInventory.AddItem(
                itemID,
                amount - added,
                remainingStoragePeriod);
        }

        return added;
    }
    #endregion

    public void DepositInventoryToStorage()
    {
        Inventory storage =
            DataManager.Instance.InventoryManager
            .Get("storage");

        foreach (var slot in mainInventory.slots)
        {
            if (slot.itemID <= 0)
                continue;

            storage.AddItem(slot.itemID, slot.count, slot.remainingStoragePeriod);

            slot.Clear();
        }
    }

    #region Save / Load / Unregister
    public void UnregisterInventories()
    {
        InventoryManager invManager = DataManager.Instance.InventoryManager;

        invManager.Unregister(mainInventory);
        invManager.Unregister(subInventory);
        invManager.Unregister(upgradeInventory);
    }

    public NPCSaveData GetSaveData()
    {
        return new NPCSaveData
        {
            id = id,
            entityName = entityName,
            position = transform.position,
            water = water,
            maxWater = maxWater,

            mainInventory = mainInventory.GetSaveData(),
            subInventory = subInventory.GetSaveData(),
            upgradeInventory = upgradeInventory.GetSaveData(),

            job = CloneJob(job)
        };
    }

    // 호출 순서 : Spawn->Initialize->Load
    public void Load(NPCSaveData data)
    {
        if (data == null)
        {
            Debug.LogError("NPC Load Error: data is null");
            return;
        }

        // 로드 시 이동 중이면 멈춤
        StopMove();

        if (id != data.id)
        {
            Debug.LogWarning($"NPC Load Warning: id 불일치 : Current: {id}, Data: {data.id}");
        }

        entityName = data.entityName;

        // 칸 기준 복원
        GridManager grid = GridManager.Instance;
        Vector2Int cell = grid.CharacterToGrid(data.position);
        transform.position = grid.GetStandPosition(cell.x, cell.y);

        water = data.water;
        maxWater = data.maxWater;

        mainInventory.Load(data.mainInventory);
        subInventory.Load(data.subInventory);
        upgradeInventory.Load(data.upgradeInventory);

        job = CloneJob(data.job);

        JobChanged();
    }

    private static NPCJobConfig CloneJob(NPCJobConfig source)
    {
        if (source == null)
            return new NPCJobConfig();

        return JsonUtility.FromJson<NPCJobConfig>(JsonUtility.ToJson(source));
    }
    #endregion
}

// 확인할 것: NPCJobController가 JobChanged() 때 HasPendingWork()/HasAnimalWork()를 다시 확인하는지
//            (농장/목장의 onWorkRequested는 NPC가 불러와지기 전에 발생하고 지나가 버림)