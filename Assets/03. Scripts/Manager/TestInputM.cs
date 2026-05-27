using UnityEngine;
using TMPro;

public class TestInputM : MonoBehaviour
{
    // 테스트용 키 텍스트
    [SerializeField] private TMP_Text testKeyText;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private MarketManager marketManager;

    [Header("NPC Spawn")]
    [SerializeField] private GameObject npcPrefab;

    void Update()
    {
        // 임시 테스트용 키
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            testKeyText.text = "1번 키 입력\n시간 30분 추가";
            timeManager.TestAddTime(30);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            testKeyText.text = "2번 키 입력\n매일 이벤트 갱신(날짜변경X)";
            timeManager.CheckDayEvent();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            testKeyText.text = "3번 키 입력";
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            testKeyText.text = "4번 키 입력";
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SpawnNPCAtMouse();
            testKeyText.text = "5번 키 입력\nNPC 생성";
        }
    }
    private void SpawnNPCAtMouse()
    {
        if (npcPrefab == null)
        {
            Debug.LogWarning("NPC Prefab is null.");
            return;
        }

        Vector3 mousePos =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mousePos.z = 0f;

        GameObject obj =
            Instantiate(npcPrefab, mousePos, Quaternion.identity);

        int npcIndex = DataManager.Instance.NPCManager.npcs.Count;

        // 이름 생성
        string npcID = $"6021_{npcIndex}";

        obj.name = npcID;

        NPC npc = obj.GetComponent<NPC>();

        npc.Initialize(npcID);
    }
}