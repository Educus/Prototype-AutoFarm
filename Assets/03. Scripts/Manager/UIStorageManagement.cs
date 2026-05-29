using System.Collections.Generic;
using UnityEngine;

public class UIStorageManagement : MonoBehaviour
{
    [SerializeField] private GameObject rocketStorage;
    [SerializeField] private UINPCInterface npcInterface;
    [SerializeField] private GameObject htdroStorage;
    [SerializeField] private GameObject buildingStorage;
    [SerializeField] private GameObject npcStorage;

    public static UIStorageManagement Instance;
    public string targetBuilding { get; private set; }
    public GameObject RocketStorage => rocketStorage;

    // 구매 예정 아이템 저장용
    public Dictionary<int, int> buyItems = new();

    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
            Destroy(gameObject);
    }

    #region Open

    public void RocketInv()
    {
        if (!OpenPopup())
            return;

        rocketStorage.SetActive(true);
        buildingStorage.SetActive(true);

        npcInterface.gameObject.SetActive(true);
        npcInterface.ShowStaff();
    }

    public void HtdroInv()
    {
        if (!OpenPopup())
            return;

        htdroStorage.SetActive(true);
    }

    public void BuildingInv()
    {
        if (!OpenPopup())
            return;

        rocketStorage.SetActive(false);
        buildingStorage.SetActive(true);
    }

    public void NPCInv()
    {
        if (!OpenPopup())
            return;

        npcStorage.SetActive(true);
    }

    #endregion

    #region Close
    public void CloseStorageManagement()
    {
        rocketStorage.SetActive(false);
        buildingStorage.SetActive(false);
        npcInterface.gameObject.SetActive(false);

        htdroStorage.SetActive(false);

        npcStorage.SetActive(false);

        CheckPopupState();

        GameManager.Instance.ExitMode();
    }

    public void CloseRocketInv()
    {
        rocketStorage.SetActive(false);
        buildingStorage.SetActive(false);
        npcInterface.gameObject.SetActive(false);

        CheckPopupState();

        GameManager.Instance.ExitMode();
    }

    public void CloseHtdroInv()
    {
        htdroStorage.SetActive(false);

        CheckPopupState();

        GameManager.Instance.ExitMode();
    }

    public void CloseNPCInv()
    {
        npcStorage.SetActive(false);

        CheckPopupState();

        GameManager.Instance.ExitMode();
    }

    #endregion

    #region Popup

    private bool OpenPopup()
    {
        // 이미 Popup 모드면 허용
        if (GameManager.Instance.IsMode(GameMode.Popup))
        {
            return true;
        }

        return GameManager.Instance.EnterMode(GameMode.Popup);
    }

    private void CheckPopupState()
    {
        bool hasPopup =
            rocketStorage.activeSelf ||
            htdroStorage.activeSelf ||
            buildingStorage.activeSelf ||
            npcStorage.activeSelf;
    }

    #endregion

    #region Item Buy Slots
    public bool CanAddBuyItem(int itemID, int addAmount)
    {
        Dictionary<int, int> temp = new Dictionary<int, int>(buyItems);

        // 추가 적용
        if (temp.ContainsKey(itemID))
        {
            temp[itemID] += addAmount;
        }
        else
        {
            temp.Add(itemID, addAmount);
        }

        int totalSlotCount = 0;

        foreach (var pair in temp)
        {
            int id = pair.Key;
            int amount = pair.Value;

            int stack = DataManager.Instance.itemsData[id].stack;

            if (stack <= 0)
            {
                Debug.LogError($"아이템 {id}의 stack 값이 0 이하입니다.");
                continue;
            }

            totalSlotCount += Mathf.CeilToInt((float)amount / stack);
        }

        return totalSlotCount <= 16;
    }
    #endregion


    public void TargetBuilding(string buildingName)
    {
        targetBuilding = buildingName;
    }
}