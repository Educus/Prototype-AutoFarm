using System;
using System.Collections.Generic;
using UnityEngine;

// 저장/불러오기에서 새로 쓰는 데이터 클래스 모음

#region Player
// 플레이어는 행동을 저장하지 않고 위치만 저장
[Serializable]
public class PlayerSaveData
{
    public Vector3 position;
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
    public float masterVolume = 1f;
    public float bgmVolume = 0.8f;
    public float sfxVolume = 0.8f;

    // 화면, 0일 경우 첫 실행
    public int screenWidth = 0;
    public int screenHeight = 0;
    public int screenMode = (int)FullScreenMode.Windowed;

    // UI 배율 (기본 1)
    public float uiScale = 1f;
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