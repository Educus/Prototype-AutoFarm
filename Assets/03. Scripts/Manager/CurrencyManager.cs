using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public int money = 1000;

    public Action<int> OnMoneyChanged;

    public void Add(int amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    public bool Spend(int amount)
    {
        if (money < amount)
            return false;

        money -= amount;
        OnMoneyChanged?.Invoke(money);
        return true;
    }

    public int Get()
    {
        return money;
    }

    #region Save / Load
    public CurrencySaveData GetSaveData()
    {
        return new CurrencySaveData
        {
            money = money
        };
    }

    public void Load(CurrencySaveData data)
    {
        money = data.money;
        OnMoneyChanged?.Invoke(money);
    }
    #endregion
}
