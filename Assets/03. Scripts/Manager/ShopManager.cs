using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    // 영수증에 담을 수 있는 칸 수 (아이템 stack 기준)
    public const int CartSlotLimit = 16;

    private readonly Dictionary<int, int> cart = new Dictionary<int, int>();

    // 영수증 UI는 이 이벤트를 구독해서 다시 그림
    public event Action onCartChanged;

    public IReadOnlyDictionary<int, int> Cart => cart;
    public bool HasCartItems => cart.Count > 0;

    #region 영수증(장바구니)

    // 영수증이 차지하는 칸 수
    public int GetCartSlotCount()
    {
        return CountSlots(cart);
    }

    // 칸 수 제한 확인
    public bool CanAddToCart(int itemID, int addAmount)
    {
        var temp = new Dictionary<int, int>(cart);

        temp.TryGetValue(itemID, out int current);
        temp[itemID] = current + addAmount;

        return CountSlots(temp) <= CartSlotLimit;
    }

    // 영수증에 추가 (칸이 모자라면 false)
    public bool TryAddToCart(int itemID, int amount)
    {
        if (amount <= 0 || !CanAddToCart(itemID, amount))
            return false;

        cart.TryGetValue(itemID, out int current);
        SetCartItem(itemID, current + amount);

        return true;
    }

    public void SetCartItem(int itemID, int count)
    {
        if (count <= 0) cart.Remove(itemID);
        else cart[itemID] = count;

        onCartChanged?.Invoke();
    }

    // 영수증의 X 버튼: 해당 항목 구매 취소
    public void RemoveCartItem(int itemID)
    {
        SetCartItem(itemID, 0);
    }

    // 로켓 출발: 영수증을 확정 목록으로 넘기고 비움 (복사가 아니라 이동)
    public Dictionary<int, int> TakeCart()
    {
        var snapshot = new Dictionary<int, int>(cart);

        cart.Clear();
        onCartChanged?.Invoke();

        return snapshot;
    }

    private static int CountSlots(Dictionary<int, int> items)
    {
        int totalSlotCount = 0;

        foreach (var pair in items)
        {
            if (!DataManager.Instance.itemsData.TryGetValue(pair.Key, out ItemData item))
            {
                Debug.LogError($"아이템 {pair.Key}의 데이터가 없습니다.");
                continue;
            }

            if (item.stack <= 0)
            {
                Debug.LogError($"아이템 {pair.Key}의 stack 값이 0 이하입니다.");
                continue;
            }

            totalSlotCount += Mathf.CeilToInt((float)pair.Value / item.stack);
        }

        return totalSlotCount;
    }

    #endregion

    #region 구매 금액

    // 구매 단가: ItemData.basicPrice (변하지 않음)
    public int GetBuyPrice(int itemID)
    {
        return DataManager.Instance.itemsData[itemID].basicPrice;
    }

    // 영수증 합계 (Rocket.TryLaunch가 출발할 때 차감하는 금액)
    public int GetCartTotal()
    {
        int total = 0;

        foreach (var pair in cart)
            total += GetBuyPrice(pair.Key) * pair.Value;

        return total;
    }

    #endregion

    #region 판매 가격

    // 시세가 있는 아이템만 판매 가능
    public bool CanSell(int itemID)
    {
        return DataManager.Instance.productClosingData.TryGetValue(itemID, out ProductClosing closing)
            && closing.HasData;
    }

    // 현재 가격 (오늘 캔들의 종가). 기존 Rocket의 prices[0]은 "가장 오래된" 종가였음
    // 시세가 1시간마다 변하므로 로켓이 출발하는 순간의 가격으로 판매 대금이 정해짐
    public int GetSellPrice(int itemID)
    {
        return DataManager.Instance.productClosingData[itemID].CurrentPrice;
    }

    #endregion

    #region Save / Load

    public List<ItemCountEntry> GetSaveData()
    {
        var list = new List<ItemCountEntry>();

        foreach (var pair in cart)
            list.Add(new ItemCountEntry { itemID = pair.Key, count = pair.Value });

        return list;
    }

    public void Load(List<ItemCountEntry> entries)
    {
        cart.Clear();

        if (entries != null)
        {
            foreach (var e in entries)
                cart[e.itemID] = e.count;
        }

        onCartChanged?.Invoke();
    }

    #endregion
}