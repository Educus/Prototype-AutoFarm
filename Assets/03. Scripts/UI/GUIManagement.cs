using UnityEngine;
using TMPro;

public class GUIManagement : MonoBehaviour
{
    // 시간
    [SerializeField] private TimeManager timeManager;

    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timeText;

    private string[] value;

    [SerializeField] private ResourceManager resourceManager;
    // NPC
    [SerializeField] private TMP_Text npcText;

    // 창고
    [SerializeField] private TMP_Text storageText;

    // 골드
    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {
        timeManager.onTimeSetpEvent += SetTime;
        resourceManager.onSetNPC += SetNPC;
        resourceManager.onSetStorage += SetStorage;
        resourceManager.onSetGold += SetGold;
    }
    private void Start()
    {
        SetTime();
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
        npcText.text = $"{resourceManager.npcCount}/99";
    }

    // 창고 GUI 자동 갱신
    private void SetStorage()
    {
        storageText.text = $"{resourceManager.storageCapacity}/999";
    }

    // 골드 GUI 자동 갱신
    private void SetGold()
    {
        goldText.text = $"{resourceManager.gold}";
    }
}
