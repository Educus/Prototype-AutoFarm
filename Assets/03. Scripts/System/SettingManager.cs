using System;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-120)]
public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; }

    public const float MinUIScale = 0.75f;
    public const float MaxUIScale = 1.5f;

    public SettingsData Data { get; private set; } = new SettingsData();

    public event Action<float> OnUIScaleChanged;

    private string FilePath => Path.Combine(Application.persistentDataPath, "settings.json");

    #region Unity
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;

        Load();
    }

    private void Start()
    {
        ApplyAll();
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
            Debug.LogError($"[SettingManager] 설정을 읽을 수 없어 기본값을 사용합니다: {e}");
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
        AudioListener.volume = Data.masterVolume;

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetVolumes(Data.bgmVolume, Data.sfxVolume);
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
        Data = new SettingsData
        {
            screenWidth = Screen.width,
            screenHeight = Screen.height,
            screenMode = (int)Screen.fullScreenMode
        };

        ApplyAll();
    }
    #endregion
}
