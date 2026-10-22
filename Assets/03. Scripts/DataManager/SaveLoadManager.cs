using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SaveReason
{
    AutoDaily,  // 매일 00시
    Manual,     // 게임 종료 시 수동저장
    StoryStart, // 스토리 시작 시
    StoryEnd    // 스토리 종료 시
}

public class SaveLoadManager : MonoBehaviour
{
    private const int SaveVersion = 1;

    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private MarketManager marketManager;

    private int autoSaveScheduleId = -1;
    private bool autoSavePending;

    #region Unity
    private void Start()
    {
        GameManager.Instance.save += Save;
        GameManager.Instance.load += Load;

        autoSaveScheduleId = TimeManager.Instance.RegisterDaily(0, 0, RequestAutoSave);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.save -= Save;
            GameManager.Instance.load -= Load;
        }

        if (TimeManager.Instance != null && autoSaveScheduleId >= 0)
            TimeManager.Instance.RemoveSchedule(autoSaveScheduleId);
    }
    #endregion

    #region Save
    // 수면 스킵 등으로 자정을 여러 번 지나도 같은 프레임에는 한 번만 저장
    private void RequestAutoSave()
    {
        if (autoSavePending)
            return;

        autoSavePending = true;
        StartCoroutine(AutoSaveAtEndOfFrame());
    }

    private IEnumerator AutoSaveAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        autoSavePending = false;
        Save(SaveReason.AutoDaily);
    }

    // 기존 호출부(GameManager.save 이벤트 등) 호환용
    public void Save()
    {
        Save(SaveReason.Manual);
    }

    public void Save(SaveReason reason, string tag = null)
    {
        try
        {
            string json = JsonUtility.ToJson(BuildSaveData(), true);

            string path = GetPath(reason, tag);
            string tmp = path + ".tmp";

            // 임시 파일에 먼저 쓰고 덮어써서, 도중에 실패해도 기존 세이브가 깨지지 않게 함
            File.WriteAllText(tmp, json);
            File.Copy(tmp, path, true);
            File.Delete(tmp);

            Debug.Log($"[SaveLoadManager] 저장 완료 ({reason}) : {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 저장 실패: {e}");
        }
    }

    // 종료 버튼에서 호출: 저장하고 종료
    public void QuitGame()
    {
        Save(SaveReason.Manual);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private GameSaveData BuildSaveData()
    {
        DataManager dm = DataManager.Instance;

        GameSaveData data = new GameSaveData();
        data.version = SaveVersion;

        // Player (행동은 저장하지 않고 위치만)
        data.player = new PlayerSaveData
        {
            position = GameManager.Instance.player.transform.position
        };

        // Time, Chunk
        data.time = TimeManager.Instance.GetSaveData();
        data.unlockedChunks = chunkManager.GetSaveData();

        // Currency, Market, Shop cart
        data.currency = dm.CurrencyManager.GetSaveData();
        data.market = marketManager.GetSaveData();
        data.shopCart = dm.ShopManager.GetSaveData();

        // Building
        data.buildings = dm.BuildingManager.GetSaveData();

        // Inventory (NPC 인벤토리는 NPCSaveData에 들어 있으므로 제외)
        HashSet<string> npcInvIds = new HashSet<string>();

        foreach (var npc in dm.NPCManager.npcs.Values)
        {
            npcInvIds.Add(npc.mainInventory.id);
            npcInvIds.Add(npc.subInventory.id);
            npcInvIds.Add(npc.upgradeInventory.id);
        }

        data.inventories = new List<InventorySaveData>();

        foreach (var inv in dm.InventoryManager.inventories.Values)
        {
            if (npcInvIds.Contains(inv.id))
                continue;

            data.inventories.Add(inv.GetSaveData());
        }

        // Animal
        data.animals = new List<AnimalSaveData>();

        foreach (var animal in dm.AnimalManager.animals.Values)
        {
            data.animals.Add(animal.GetSaveData());
        }

        // NPC
        data.npcs = new List<NPCSaveData>();

        foreach (var npc in dm.NPCManager.npcs.Values)
        {
            data.npcs.Add(npc.GetSaveData());
        }

        // Key Codes
        data.keyCodes = KeyCodeDataManager.Instance.GetSaveData();

        return data;
    }

    private string GetPath(SaveReason reason, string tag)
    {
        string dir = Application.persistentDataPath;

        switch (reason)
        {
            // 스토리 지점 저장은 일일 저장과 다른 파일 (그 지점부터 다시 시작할 때 사용)
            // 단순 자동 저장 용도라면 아래 두 줄을 지우고 save.json 하나로 합치세요
            case SaveReason.StoryStart: return $"{dir}/story_{tag}_start.json";
            case SaveReason.StoryEnd: return $"{dir}/story_{tag}_end.json";

            // 일일 저장과 종료 저장은 이어하기용
            default: return $"{dir}/save.json";
        }
    }

    #endregion

    #region Load

    public void Load()
    {
        Load(SaveReason.Manual);
    }

    public void Load(SaveReason reason, string tag = null)
    {
        string path = GetPath(reason, tag);

        // 1. 파일 읽기와 검증: 파싱이 성공한 다음에만 기존 상태를 지우기 시작
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveLoadManager] 세이브 파일이 없습니다: {path}");
            return;
        }

        GameSaveData data;

        try
        {
            data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 세이브 파일을 읽을 수 없습니다: {e}");
            return;
        }

        if (data == null)
        {
            Debug.LogError("[SaveLoadManager] 세이브 데이터가 비어 있습니다.");
            return;
        }

        try
        {
            ApplySaveData(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 불러오기 중 오류: {e}");
        }
    }

    private void ApplySaveData(GameSaveData data)
    {
        DataManager dm = DataManager.Instance;

        // 2. 모드/UI 정리, NPC와 동물 정리 (건물을 다시 만드는 동안 옛 객체가 남지 않게)
        if (inputManager != null)
            inputManager.CloseAllModes();

        dm.NPCManager.Clear();
        dm.AnimalManager.Clear();

        // 3. 게임 시간, 청크 (시장 이벤트의 날짜와 이동 판정이 의존)
        TimeManager.Instance.Load(data.time);
        chunkManager.Load(data.unlockedChunks);

        // 4. 플레이어 (행동은 저장하지 않으므로 진행 중인 행동과 이동을 모두 끊고 위치만 복원)
        if (data.player != null)
        {
            Player player = GameManager.Instance.player;

            player.GetComponent<PlayerAction>().ResetAction();
            player.GetComponent<PlayerController>().TeleportTo(data.player.position);
        }

        // 5. 돈, 시장, 상점 장바구니
        dm.CurrencyManager.Load(data.currency);
        marketManager.Load(data.market);
        dm.ShopManager.Load(data.shopCart);

        // 6. 건물 (기존 건물 삭제 -> 생성 -> 건물별 데이터 복원)
        dm.BuildingManager.Load(OrEmpty(data.buildings));

        // 7. 인벤토리 (건물 인벤토리가 등록된 뒤. NPC 인벤토리는 저장 때 제외했으므로 여기 없음)
        foreach (var invData in OrEmpty(data.inventories))
        {
            Inventory inv = dm.InventoryManager.Get(invData.id);

            if (inv == null)
                continue;

            inv.Load(invData);
        }

        // 8. 동물: 자동 할당을 끄고 저장된 소속대로 배정한 뒤 마지막에 한 번만 자동 할당
        AnimalManager animalManager = dm.AnimalManager;
        animalManager.BeginLoad();

        try
        {
            foreach (var animalData in OrEmpty(data.animals))
            {
                AnimalBase animal = animalManager.Spawn(
                    animalData.itemId, animalData.id, animalData.position);

                if (animal == null)
                    continue;

                animal.Load(animalData);

                if (!string.IsNullOrEmpty(animalData.ranchBuildingID))
                {
                    RanchBuilding ranch = dm.BuildingManager
                        .Get<RanchBuilding>(animalData.ranchBuildingID);

                    if (ranch != null)
                        animalManager.AssignTo(animal, ranch);
                }
            }
        }
        finally
        {
            animalManager.EndLoad();
        }

        // 9. NPC: 저장된 id로 다시 생성 (건물과 동물이 먼저 있어야 작업 대상을 찾을 수 있음)
        foreach (var npcData in OrEmpty(data.npcs))
        {
            NPC npc = dm.NPCManager.Spawn(npcData.id, npcData.position);

            if (npc != null)
                npc.Load(npcData);
        }

        // 10. 키코드
        KeyCodeDataManager.Instance.Load(data.keyCodes);

        // 불러온 직후에 시간이 바로 흐르지 않게 일시정지
        TimeManager.Instance.Pause();
        GameManager.Instance.isPlay = false;
    }

    // 옛 세이브에는 새 필드가 없어서 null일 수 있음
    private static List<T> OrEmpty<T>(List<T> list)
    {
        return list ?? new List<T>();
    }

    #endregion
}