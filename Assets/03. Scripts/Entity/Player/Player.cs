using System;
using UnityEngine;
using UnityEngine.AI;

public class Player : StatusBase
{
    public float moveSpeed = 3f;

    IInteractable interactTarget;
    bool isInteracting;

    public Inventory mainInventory;
    public Inventory subInventory;

    public int selectedSubSlotIndex { get; private set; } = -1;

    public static event Action OnInitialized;

    void Start()
    {
        GameManager.Instance.player = this;

        Initialize();
    }

    // Player 기본 세팅
    public void Initialize()
    {
        entityName = "Player";

        InitializeInventories();
    }

    private void InitializeInventories()
    {
        // ID는 반드시 고유해야 함 (Save/Load 기준)
        mainInventory.id = $"{entityName}_main";
        subInventory.id = $"{entityName}_sub";

        mainInventory.type = InventoryType.Main;
        subInventory.type = InventoryType.Sub;

        // 슬롯 초기화 (기본값)
        if (mainInventory.slots.Count == 0)
            mainInventory.Initialize(5);

        if (subInventory.slots.Count == 0)
            subInventory.Initialize(3);

        // InventoryManager 등록
        InventoryManager invManager =
            DataManager.Instance.InventoryManager;

        invManager.Register(mainInventory);
        invManager.Register(subInventory);

        OnInitialized?.Invoke();
    }

    public void SelectSubSlot(int index)
    {
        selectedSubSlotIndex = index;
    }

    public override void OnInteract()
    {
    }
}
