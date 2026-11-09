using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    None,
    Popup,
    Build,
    Work
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    // 시스템, 이벤트 호출 담당

    // 게임 모드 관리
    [Header("Mode")]
    public GameMode CurrentMode { get; private set; } = GameMode.None;
    private GameMode previousMode = GameMode.None;
    [SerializeField] public GameMode NowGameMode = GameMode.None;

    [Header("Player")]
    public Player player;

    [Header("Work")]
    public NPC selectedNPC;

    // 일시정지
    [Header("Game")]
    [SerializeField] private bool isPlayValue = false;

    public bool isTitle = true;

    #region Game(isPlay)
    public bool isPlay
    {
        get => isPlayValue;
        set
        {
            isPlayValue = value;
            ApplyTimeScale();
            onSpeedChanged?.Invoke(isPlayValue, GameSpeed);
        }
    }

    public float GameSpeed { get; private set; } = 1f;

    public event Action<bool, float> onSpeedChanged;

    private void ApplyTimeScale()
    {
        Time.timeScale = isPlayValue ? GameSpeed : 0f;
    }
    #endregion

    // 카메라 타겟
    [Header("Camera")]
    public GameObject targetLock;

    // 모드 변경 시 호출
    public event Action<GameMode> onModeChanged;

    // Save/Load
    public event Action save;
    public event Action load;

    #region Unity
    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
            Destroy(gameObject);

        ApplyTimeScale();
    }

    private void Update()
    {
        NowGameMode = CurrentMode;
    }

    #endregion

    #region Mode
    // None 상태일 때만 진입 가능
    public bool EnterMode(GameMode mode)
    {
        if (CurrentMode != GameMode.None)
        {
            Debug.Log($"이미 다른 모드 진행중 : {CurrentMode}");

            return false;
        }

        CurrentMode = mode;

        Debug.Log($"모드 진입 : {mode}");

        RefreshHighlights();

        onModeChanged?.Invoke(CurrentMode);

        return true;
    }
    public void ExitMode()
    {
        CurrentMode = GameMode.None;

        selectedNPC = null;

        Debug.Log("모드 종료");

        RefreshHighlights();

        onModeChanged?.Invoke(CurrentMode);
    }

    public bool IsMode(GameMode mode)
    {
        return CurrentMode == mode;
    }

    public bool IsBusy()
    {
        return CurrentMode != GameMode.None;
    }

    // work 모드 전환
    public bool EnterWorkMode()
    {
        // Popup 상태에서만 가능
        if (CurrentMode != GameMode.Popup)
            return false;

        previousMode = CurrentMode;

        CurrentMode = GameMode.Work;

        RefreshHighlights();

        onModeChanged?.Invoke(CurrentMode);

        return true;
    }

    public void ExitWorkMode()
    {
        if (CurrentMode != GameMode.Work)
            return;

        CurrentMode = previousMode;

        previousMode = GameMode.None;

        RefreshHighlights();

        onModeChanged?.Invoke(CurrentMode);
    }
    #endregion

    #region Speed
    // 타이틀에서 게임 시작 후 1배속
    public void StartPlay()
    {
        isTitle = false;
        GameSpeed = 1f;
        isPlay = true;
    }

    // 스페이스바
    public void CycleSpeed()
    {
        PlayerDataManager player = DataManager.Instance.PlayerDataManager;
        IReadOnlyList<float> speeds = player.UnlockedSpeeds;

        if (!isPlay)
        {
            GameSpeed = speeds[0];
            isPlay = true;
            return;
        }

        int index = -1;

        for (int i = 0; i < speeds.Count; i++)
        {
            if (Mathf.Approximately(speeds[i], GameSpeed))
            {
                index = i;
                break;
            }
        }

        if (index +1 < speeds.Count)
        {
            GameSpeed = speeds[index + 1];
            ApplyTimeScale();
            onSpeedChanged?.Invoke(isPlayValue, GameSpeed);
        }
        else
        {
            isPlay = false;
        }
    }
    #endregion

    #region Highlight
    public void RefreshHighlights()
    {
        if (OriginDataManager.Instance == null)
            return;

        if (DataManager.Instance.BuildingManager == null)
            return;

        foreach (var building in DataManager.Instance.BuildingManager.GetAll())
        {
            building.UpdateHighlight();
        }
    }
    #endregion
}
