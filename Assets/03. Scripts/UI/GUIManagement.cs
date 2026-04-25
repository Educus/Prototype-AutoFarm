using UnityEngine;
using TMPro;

public class GUIManagement : MonoBehaviour
{
    // 시간
    [SerializeField] private TimeManager timeManager;

    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timeText;

    private string[] value;

    [SerializeField] private CurrencyManager currencyManager;
    // NPC
    [SerializeField] private TMP_Text npcText;

    // 창고
    [SerializeField] private TMP_Text storageText;

    // 골드
    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {
        timeManager.onTimeSetpEvent += SetTime;
    }
    private void Start()
    {
        SetTime();
    }
    private void Update()
    {
        SetNPC();
        SetStorage();
        SetGold();
    }

    // 시간 GUI 자동 갱신
    private void SetTime()
    {
        value = timeManager.GetTimeString();

        dayText.text = $"Day-{value[0]}";
        timeText.text = $"{value[1]}:{value[2]}";
    }

    // NPC GUI 자동 갱신
    private void SetNPC()
    {
        foreach (var npc in DataManager.Instance.NPCManager.npcs)
        {
            // 일 하는 중인 NPC
        }
        npcText.text = $"/{DataManager.Instance.NPCManager.npcs.Count}";
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
