using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class PlayerDataManager : MonoBehaviour
{
    #region 재화
    public int money = 1000;

    public Action<int> OnMoneyChanged;

    public void AddMoney(int amount)
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
    #endregion

    #region 키 설정
    // 실제 사용 키
    private readonly Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();

    public event Action OnKeyBindingChanged;

    private void EnsureKeyBindings()
    {
        if (keyBindings.Count == 0)
            FillDefaultKeys();
    }
    private void FillDefaultKeys()
    {
        keyBindings.Clear();

        KeyCodeDataManager table = OriginDataManager.Instance != null
            ? OriginDataManager.Instance.KeyCodes : null;

        if (table == null)
            return;

        foreach (var pair in table.DefaultKeys)
            keyBindings[pair.Key] = pair.Value;
    }

    // 키를 눌렀을 때 true
    public bool GetKeyDown(string actionName)
    {
        EnsureKeyBindings();

        if (!keyBindings.TryGetValue(actionName, out KeyCode key))
            return false;

        return Input.GetKeyDown(key);
    }

    // 키를 누르고 있을 때 true
    public bool GetKeyHeld(string actionName)
    {
        EnsureKeyBindings();

        if (!keyBindings.TryGetValue(actionName, out KeyCode key))
            return false;

        return Input.GetKey(key);
    }

    public KeyCode GetKey(string actionName)
    {
        EnsureKeyBindings();

        return keyBindings.TryGetValue(actionName, out KeyCode key) ? key : KeyCode.None;
    }

    // 키 변경
    public bool RebindKey(string actionName, KeyCode newKey)
    {
        EnsureKeyBindings();

        KeyCodeDataManager table = OriginDataManager.Instance != null
            ? OriginDataManager.Instance.KeyCodes : null;

        if (table != null && !table.IsRebindable(actionName))
            return false;

        keyBindings[actionName] = newKey;
        OnKeyBindingChanged?.Invoke();

        return true;
    }

    public void ResetBindings()
    {
        FillDefaultKeys();
        OnKeyBindingChanged?.Invoke();
    }
    #endregion

    #region 북마크
    private readonly HashSet<int> bookmarks = new HashSet<int>();

    // 북마크 변경 시 호출되는 이벤트(UI 갱신)
    public event Action OnBookmarksChanged;

    public bool IsBookmarked(int itemId)
    {
        return bookmarks.Contains(itemId);
    }

    public void SetBookmark(int itemId, bool value)
    {
        bool changed = value ? bookmarks.Add(itemId) : bookmarks.Remove(itemId);

        if (changed)
            OnBookmarksChanged?.Invoke();
    }

    public bool ToggleBookmark(int itemId)
    {
        bool now = !IsBookmarked(itemId);
        SetBookmark(itemId, now);

        return now;
    }
    #endregion

    #region Save / Load
    public PlayerDataSaveData GetSaveData()
    {
        EnsureKeyBindings();

        var data = new PlayerDataSaveData { money = money };

        foreach (var pair in keyBindings)
        {
            data.keyCodes.Add(new KeyCodeSaveData
            {
                actionName = pair.Key,
                keyCode = pair.Value.ToString()
            });
        }

        data.bookmarks.AddRange(bookmarks);

        return data;
    }

    public void Load(PlayerDataSaveData data)
    {
        if (data == null)
            return;

        money = data.money;
        OnMoneyChanged?.Invoke(money);

        FillDefaultKeys();

        if (data.keyCodes != null)
        {
            foreach (var keyData in data.keyCodes)
            {
                if (Enum.TryParse(keyData.keyCode, out KeyCode key))
                    keyBindings[keyData.actionName] = key;
                else
                    Debug.LogWarning($"키 불러오기 실패 : {keyData.actionName}");
            }
        }

        bookmarks.Clear();

        if (data.bookmarks != null)
        {
            foreach (var itemId in data.bookmarks)
                bookmarks.Add(itemId);
        }

        OnKeyBindingChanged?.Invoke();
        OnBookmarksChanged?.Invoke();
    }
    #endregion
}
