using System;
using System.Collections.Generic;
using UnityEngine;

public enum RebindResult
{
    Success,
    UnknownAction,   // 표에 없는 동작
    NotRebindable,   // 표에서 바꿀 수 없다고 한 키 (IsRebindable != 1)
    InvalidKey,      // ESC, 마우스, 조이스틱, 시스템 키 등 사용할 수 없는 키
    Conflict         // 다른 동작이 이미 쓰는 키
}

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

    public event Action OnKeyBindingsChanged;

    // 키 입력을 받는 중이면 게임 입력 막기
    private int captureCount;
    public bool IsCapturingKey => captureCount > 0;

    public void BeginKeyCapture()
    {
        captureCount++;
    }

    public void EndKeyCapture()
    {
        captureCount = Math.Max(0, captureCount - 1);
    }

    private void EnsureKeyBindings()
    {
        if (keyBindings.Count == 0)
            FillDefaultKeys();
    }
    private void FillDefaultKeys()
    {
        keyBindings.Clear();

        KeyCodeDataManager table = KeyTable;

        if (table == null)
            return;

        foreach (var pair in table.DefaultKeys)
            keyBindings[pair.Key] = pair.Value;
    }

    private static KeyCodeDataManager KeyTable =>
        OriginDataManager.Instance != null ? 
        OriginDataManager.Instance.KeyCodes : null;

    // 키를 눌렀을 때 true
    public bool GetKeyDown(string actionName)
    {
        if (IsCapturingKey)
            return false;

        EnsureKeyBindings();

        if (!keyBindings.TryGetValue(actionName, out KeyCode key))
            return false;

        return Input.GetKeyDown(key);
    }

    // 키를 누르고 있을 때 true
    public bool GetKeyHeld(string actionName)
    {
        if (IsCapturingKey)
            return false;

        EnsureKeyBindings();

        if (!keyBindings.TryGetValue(actionName, out KeyCode key))
            return false;

        return Input.GetKey(key);
    }

    // 현재 설정된 키 (키 설정 화면 표시용. 입력 받는 중에도 읽을 수 있음)
    public KeyCode GetKey(string actionName)
    {
        EnsureKeyBindings();

        return keyBindings.TryGetValue(actionName, out KeyCode key) ? key : KeyCode.None;
    }

    // 키로 지정할 수 있는 키인가 (ESC, 마우스, 조이스틱, 시스템 키는 불가)
    public static bool IsAllowedBindingKey(KeyCode key)
    {
        if (key == KeyCode.None || key == KeyCode.Escape)
            return false;

        if (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6)
            return false;

        if (key >= KeyCode.JoystickButton0 && key <= KeyCode.Joystick8Button19)
            return false;

        // 같은 값의 이름이 여럿일 수 있어서 switch 대신 ||로 비교
        if (key == KeyCode.LeftWindows || key == KeyCode.RightWindows
            || key == KeyCode.LeftCommand || key == KeyCode.RightCommand
            || key == KeyCode.Menu || key == KeyCode.Print || key == KeyCode.SysReq
            || key == KeyCode.Break || key == KeyCode.Numlock
            || key == KeyCode.CapsLock || key == KeyCode.ScrollLock)
            return false;

        return true;
    }

    // 키 변경
    public RebindResult TryRebind(string actionName, KeyCode newKey, out string conflictAction)
    {
        conflictAction = null;

        EnsureKeyBindings();

        KeyCodeDataManager table = KeyTable;

        if (table == null || !table.Table.ContainsKey(actionName))
            return RebindResult.UnknownAction;

        if (!table.IsRebindable(actionName))
            return RebindResult.NotRebindable;

        if (!IsAllowedBindingKey(newKey))
            return RebindResult.InvalidKey;

        foreach (var pair in keyBindings)
        {
            if (pair.Key != actionName && pair.Value == newKey)
            {
                conflictAction = pair.Key;
                return RebindResult.Conflict;
            }

        }
        keyBindings[actionName] = newKey;
        OnKeyBindingsChanged?.Invoke();

        return RebindResult.Success;
    }

    public void ResetKeyBindings()
    {
        FillDefaultKeys();
        OnKeyBindingsChanged?.Invoke();
    }

    // settings.json 용 (SettingManager 가 사용)
    public List<KeyCodeSaveData> GetKeyBindingSaveData()
    {
        EnsureKeyBindings();

        var list = new List<KeyCodeSaveData>();

        foreach (var pair in keyBindings)
        {
            list.Add(new KeyCodeSaveData
            {
                actionName = pair.Key,
                keyCode = pair.Value.ToString()
            });
        }

        return list;
    }

    // 기본 키부터 채우고 저장된 키로 덮어씀 (새로 추가된 동작도 기본 키를 갖도록)
    // 저장된 값이 사용 불가 키이거나 다른 동작과 겹치면 그 항목은 무시하고 기본 키를 유지
    public void LoadKeyBindings(List<KeyCodeSaveData> saved)
    {
        FillDefaultKeys();

        if (saved != null)
        {
            KeyCodeDataManager table = KeyTable;

            foreach (var item in saved)
            {
                if (!Enum.TryParse(item.keyCode, out KeyCode key)
                    || !Enum.IsDefined(typeof(KeyCode), key))
                {
                    Debug.LogWarning($"키 불러오기 실패 : {item.actionName}");
                    continue;
                }

                // 표에 없거나 바꿀 수 없는 동작, 사용 불가 키는 무시
                if (table == null || !table.IsRebindable(item.actionName) || !IsAllowedBindingKey(key))
                    continue;

                bool conflict = false;

                foreach (var pair in keyBindings)
                {
                    if (pair.Key != item.actionName && pair.Value == key)
                    {
                        conflict = true;
                        break;
                    }
                }

                if (!conflict)
                    keyBindings[item.actionName] = key;
            }
        }

        OnKeyBindingsChanged?.Invoke();
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

    #region 배속
    // 일시정지, 1배율 기본 해금. 1.5배, 2배 등은 UnlockSpeed 로 해금 (오름차순 유지)
    private readonly List<float> unlockedSpeeds = new List<float> { 1f };

    public event Action OnSpeedUnlocked;

    public IReadOnlyList<float> UnlockedSpeeds => unlockedSpeeds;

    public bool IsSpeedUnlocked(float speed)
    {
        foreach (float s in unlockedSpeeds)
        {
            if (Mathf.Approximately(s, speed))
                return true;
        }

        return false;
    }

    // 해금했으면 true
    public bool UnlockSpeed(float speed)
    {
        if (speed <= 0f || IsSpeedUnlocked(speed))
            return false;

        unlockedSpeeds.Add(speed);
        unlockedSpeeds.Sort();

        OnSpeedUnlocked?.Invoke();

        return true;
    }
    #endregion

    #region Save / Load
    public PlayerDataSaveData GetSaveData()
    {
        var data = new PlayerDataSaveData { money = money };

        data.bookmarks.AddRange(bookmarks);
        data.unlockedSpeeds.AddRange(unlockedSpeeds);

        return data;
    }

    public void Load(PlayerDataSaveData data)
    {
        if (data == null)
            return;

        money = data.money;
        OnMoneyChanged?.Invoke(money);

        bookmarks.Clear();

        if (data.bookmarks != null)
        {
            foreach (var itemId in data.bookmarks)
                bookmarks.Add(itemId);
        }

        unlockedSpeeds.Clear();
        unlockedSpeeds.Add(1f);

        if (data.unlockedSpeeds != null)
        {
            foreach (float s in data.unlockedSpeeds)
            {
                if (s > 0f && !IsSpeedUnlocked(s))
                    unlockedSpeeds.Add(s);
            }
        }

        unlockedSpeeds.Sort();

        OnBookmarksChanged?.Invoke();
        OnSpeedUnlocked?.Invoke();
    }
    #endregion
}
