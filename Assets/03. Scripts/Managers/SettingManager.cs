using System;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-120)]
public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; }

    public const float MinUIScale = 0.75f;
    public const float MaxUIScale = 1.25f;

    public SettingsData Data { get; private set; } = new SettingsData();

    public event Action<float> OnUIScaleChanged;

    private PlayerDataManager keySource;

    private string FilePath => Path.Combine(Application.persistentDataPath, "settings.json");

    #region Unity
    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Load();
    }

    private void Start()
    {
        ApplyAll();

        keySource = DataManager.Instance != null ? DataManager.Instance.PlayerDataManager : null;

        if (keySource != null)
        {
            keySource.LoadKeyBindings(Data.keyCodes);
            keySource.OnKeyBindingsChanged += OnKeyBindingsChanged;
        }
        else
        {
            Debug.LogError("[SettingManager] PlayerDataManager 를 찾을 수 없습니다.");
        }
    }

    private void OnDestroy()
    {
        if (keySource != null)
            keySource.OnKeyBindingsChanged -= OnKeyBindingsChanged;
    }

    // 키가 바뀔 때마다 바로 저장 (설정 창을 닫지 않고 종료해도 유지)
    private void OnKeyBindingsChanged()
    {
        Data.keyCodes = keySource.GetKeyBindingSaveData();
        Save();
    }
    #endregion

    #region Save / Load
    public void Save()
    {
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SettingManager] 설정 저장 실패: {e}");
        }
    }
    private void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                Data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(FilePath)) ?? new SettingsData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SettingManager] 설정을 읽을 수 없어 기본값을 사용: {e}");
            Data = new SettingsData();
        }

        // 처음 실행 : 현재 값을 기본값으로 설정
        if (Data.screenWidth <= 0 || Data.screenHeight <= 0)
        {
            Data.screenWidth = Screen.width;
            Data.screenHeight = Screen.height;
            Data.screenMode = (int)Screen.fullScreenMode;
        }

        Data.masterVolume = Mathf.Clamp01(Data.masterVolume);
        Data.bgmVolume = Mathf.Clamp01(Data.bgmVolume);
        Data.sfxVolume = Mathf.Clamp01(Data.sfxVolume);
        Data.uiScale = Mathf.Clamp(Data.uiScale, MinUIScale, MaxUIScale);

        if (Data.keyCodes == null)
            Data.keyCodes = new System.Collections.Generic.List<KeyCodeSaveData>();
    }
    #endregion

    #region Apply
    public void ApplyAll()
    {
        ApplyVolume();
        ApplyScreen();
        OnUIScaleChanged?.Invoke(Data.uiScale);
    }

    private void ApplyVolume()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetVolumes(Data.masterVolume, Data.bgmVolume, Data.sfxVolume);
    }

    private void ApplyScreen()
    {
        Screen.SetResolution(Data.screenWidth, Data.screenHeight, (FullScreenMode)Data.screenMode);
    }
    #endregion

    #region 값 변경
    public void SetVolumes(float master, float bgm, float sfx)
    {
        Data.masterVolume = Mathf.Clamp01(master);
        Data.bgmVolume = Mathf.Clamp01(bgm);
        Data.sfxVolume = Mathf.Clamp01(sfx);

        ApplyVolume();
    }

    public void SetScreen(int width, int height, FullScreenMode mode)
    {
        Data.screenWidth = width;
        Data.screenHeight = height;
        Data.screenMode = (int)mode;

        ApplyScreen();
    }

    public void SetUIScale(float scale)
    {
        Data.uiScale = Mathf.Clamp(scale, MinUIScale, MaxUIScale);

        OnUIScaleChanged?.Invoke(Data.uiScale);
    }

    public void ResetToDefault()
    {
        var keep = Data.keyCodes;

        Data = new SettingsData
        {
            screenWidth = Screen.width,
            screenHeight = Screen.height,
            screenMode = (int)Screen.fullScreenMode,
            keyCodes = keep
        };

        ApplyAll();
    }
    #endregion
}
