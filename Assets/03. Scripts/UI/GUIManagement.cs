using UnityEngine;
using TMPro;

public class GUIManagement : MonoBehaviour
{
    // 시간
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timeText;

    [SerializeField] private CurrencyManager currencyManager;
    // NPC
    [SerializeField] private TMP_Text npcText;

    // 창고
    [SerializeField] private TMP_Text storageText;

    // 골드
    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {
        TimeManager.Instance.onMinuteEvent += SetTime;
    }
    private void Start()
    {
        SetTime(TimeManager.Instance.minute);
    }
    private void Update()
    {
        SetNPC();
        SetStorage();
        SetGold();
    }

    private void OnDisable()
    {
        TimeManager.Instance.onMinuteEvent -= SetTime;
    }

    // 시간 GUI 자동 갱신
    private void SetTime(int minute)
    {
        int day = TimeManager.Instance.day;
        int hour = TimeManager.Instance.hour;

        dayText.text = $"Day-{day}";
        timeText.text = $"{hour}:{minute}";
    }

    // NPC GUI 자동 갱신
    private void SetNPC()
    {
        int value = 0;

        foreach (var npc in DataManager.Instance.NPCManager.npcs)
        {
            // 일 하는 중인 NPC
            if (npc.Value.job.productItemID != 0) value++;
        }

        npcText.text = $"{value}/{DataManager.Instance.NPCManager.npcs.Count}";
    }

    // 창고 GUI 자동 갱신
    private void SetStorage()
    {
        int totalStorage = 0;
        int storage = 0;

        foreach (var invens in DataManager.Instance.InventoryManager.inventories)
        {
            if (invens.Value.type == InventoryType.Unified)
            {
                totalStorage += invens.Value.slots.Count;

                foreach (var slot in invens.Value.slots)
                {
                    storage += slot.IsEmpty() ? 0 : 1;
                }
            }
        }
        storageText.text = $"{storage}/{totalStorage}";
    }

    // 골드 GUI 자동 갱신
    private void SetGold()
    {
        goldText.text = $"{DataManager.Instance.CurrencyManager.money}";
    }

}
