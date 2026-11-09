using System;
using System.Collections.Generic;
using UnityEngine;

// 저장/불러오기에서 새로 쓰는 데이터 클래스 모음
#region Game
[Serializable]
public class GameSaveData
{
    public int version = 1;

    public PlayerSaveData player;           // 플레이어 오브젝트(위치)
    public PlayerDataSaveData playerData;   // 돈, 북마크, 해금한 배속
    public TimeSaveData time;
    public List<Vector2Int> unlockedChunks;

    public MarketSaveData market;
    public List<ItemCountEntry> shopCart;

    public List<BuildingSaveData> buildings;
    public List<InventorySaveData> inventories;
    public List<AnimalSaveData> animals;
    public List<NPCSaveData> npcs;
}
#endregion

#region Player
// 플레이어는 행동을 저장하지 않고 위치만 저장
[Serializable]
public class PlayerSaveData
{
    public Vector3 position;
}

// PlayerDataManager의 저장 데이터
// 조작키 관련 데이터는 SettingsData에 저장
[Serializable]
public class PlayerDataSaveData
{
    public int money;
    public List<int> bookmarks = new List<int>();         // 북마크한 아이템 ID
    public List<float> unlockedSpeeds = new List<float>();// 해금한 배속 목록
}
#endregion

#region System
[Serializable]
public class TimeSaveData
{
    public int day;
    public int hour;
    public int minute;
}

[Serializable]
public class SettingsData
{
    // 소리 (0~1)
    public float masterVolume = 0.8f;
    public float bgmVolume = 0.8f;
    public float sfxVolume = 0.8f;

    // 화면, 0일 경우 첫 실행
    public int screenWidth = 0;
    public int screenHeight = 0;
    public int screenMode = (int)FullScreenMode.Windowed;

    // UI 배율 (기본 1)
    public float uiScale = 1f;

    // 바꾼 조작키 저장
    public List<KeyCodeSaveData> keyCodes = new List<KeyCodeSaveData>();
}
#endregion

// 아이템 ID와 개수 쌍 (JsonUtility는 Dictionary를 저장하지 못해서 목록으로 바꿔 저장)
// 상점 장바구니, 로켓 구매 확정 목록에서 공용으로 사용
[Serializable]
public class ItemCountEntry
{
    public int itemID;
    public int count;
}

#region Building
[Serializable]
public class RocketSaveData
{
    public bool isAway;
    public int remainingMinutes;
    public int money;
    public bool hasPendingBuyDelivery;
    public List<ItemCountEntry> reservedBuyItems = new List<ItemCountEntry>();
}

[Serializable]
public class BuildingSaveData
{
    public int itemId;  // 아이템 아이디
    public string id;   // 건물 이름(저장 키)
    public Vector2 position;

    public Vector2Int gridPosition;   // 왼쪽 아래 칸 (ApplyToGrid 기준)

    // 처음 기본으로 주어진 건물인가 (창고 파괴 규칙에 사용)
    public bool isDefault;

    public BuildingType type;   // 건물 타입
    public string jsonData;     // 건물 상세 데이터
}
#endregion

#region Animal
[Serializable]
public class AnimalSaveData
{
    public int itemId;
    public string id;
    public string animalName;
    public Vector3 position;
 
    public int isStack;
 
    // 배정된 목장 건물 id (없으면 빈 값 = 미배정)
    public string ranchBuildingID;
 
    // 다음 생산까지 남은 시간
    public int remainingTime;
 
    // 오늘 수확한 횟수
    public int todayHarvestCount;
}
#endregion

#region Market
[Serializable]
public class MarketItemSaveData
{
    public int itemID;
    public MarketState state;

    // JsonUtility는 null을 표현하지 못해서 이벤트가 없는 항목도 빈 객체로 복원됨
    // 그래서 이벤트 존재 여부를 따로 저장
    public bool hasEvent;
    public MarketEvent currentEvent;   // hasEvent가 false면 무시
}

[Serializable]
public class ClosingPriceSaveData
{
    public int itemID;
    public float exactPrice;

    public List<PriceCandle> candles = new List<PriceCandle>();
}

[Serializable]
public class MarketSaveData
{
    public List<MarketItemSaveData> states = new List<MarketItemSaveData>();
    public List<ClosingPriceSaveData> closingPrices = new List<ClosingPriceSaveData>();
}
#endregion