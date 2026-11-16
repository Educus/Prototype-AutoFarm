using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    [Header("Sound")]
    [RequiredRef, SerializeField] private Slider masterSlider;
    [RequiredRef, SerializeField] private Slider bgmSlider;
    [RequiredRef, SerializeField] private Slider sfxSlider;
    [RequiredRef, SerializeField] private TMP_Text masterLabel;
    [RequiredRef, SerializeField] private TMP_Text bgmLabel;
    [RequiredRef, SerializeField] private TMP_Text sfxLabel;

    [Header("Screen")]
    [RequiredRef, SerializeField] private TMP_Dropdown resolutionDropdown;
    [RequiredRef, SerializeField] private TMP_Dropdown screenModeDropdown;
    [RequiredRef, SerializeField] private Slider uiScaleSlider;
    [RequiredRef, SerializeField] private TMP_Text uiScaleLabel;

    [Header("Buttons")]
    [RequiredRef, SerializeField] private Button closeButton;
    [RequiredRef, SerializeField] private TMP_Text closeText;
    [RequiredRef, SerializeField] private Button resetButton;

    [Header("Scroll")]
    [RequiredRef, SerializeField] private ScrollRect scrollRect;

    [Header("Return To Title (게임 중에만 표시)")]
    [RequiredRef, SerializeField] private GameObject returnConfirmPanel;
    [RequiredRef, SerializeField] private TMP_Text returnConfirmText;
    [RequiredRef, SerializeField] private Button returnYButton;
    [RequiredRef, SerializeField] private Button returnNButton;

    // 드롭다운 항목 순서와 FullScreenMode 대응
    private static readonly FullScreenMode[] Modes =
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed
    };

    private static readonly string[] ModeNames = { "FullScreenWindow", "ExclusiveFullScreen", "Windowed" };

    private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
    private bool refreshing;

    private void Awake()
    {
        masterSlider.onValueChanged.AddListener(_ => OnVolumeChanged());
        bgmSlider.onValueChanged.AddListener(_ => OnVolumeChanged());
        sfxSlider.onValueChanged.AddListener(_ => OnVolumeChanged());

        resolutionDropdown.onValueChanged.AddListener(_ => OnScreenChanged());
        screenModeDropdown.onValueChanged.AddListener(_ => OnScreenChanged());

        uiScaleSlider.minValue = SettingManager.MinUIScale;
        uiScaleSlider.maxValue = SettingManager.MaxUIScale;
        uiScaleSlider.onValueChanged.AddListener(OnUIScaleChanged);

        closeButton.onClick.AddListener(Close);
        resetButton.onClick.AddListener(ResetToDefault);

        closeButton.onClick.AddListener(OnReturnTitleClicked);
        returnYButton.onClick.AddListener(OnReturnTitleConfirmed);
        returnNButton.onClick.AddListener(() => returnConfirmPanel.SetActive(false));
    }

    private void OnEnable()
    {
        Refresh();

        if (GameManager.Instance != null)
            return;

        string closingText = GameManager.Instance.isTitle ? "나가기" : "타이틀로";
        closeText.text = closingText;
        returnConfirmPanel.SetActive(false);

        StartCoroutine(ScrollToTop());
    }

    private IEnumerator ScrollToTop()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    // 현재 설정값으로 UI 를 채움 (값을 바꾸는 이벤트가 다시 호출되지 않게 함)
    private void Refresh()
    {
        SettingManager settings = SettingManager.Instance;

        if (settings == null)
        {
            Debug.LogWarning("UISettings: SettingManager 가 없습니다.");
            return;
        }

        refreshing = true;

        SettingsData data = settings.Data;

        masterSlider.SetValueWithoutNotify(data.masterVolume);
        bgmSlider.SetValueWithoutNotify(data.bgmVolume);
        sfxSlider.SetValueWithoutNotify(data.sfxVolume);
        uiScaleSlider.SetValueWithoutNotify(data.uiScale);

        BuildResolutionOptions(data.screenWidth, data.screenHeight);
        BuildModeOptions(data.screenMode);

        UpdateLabels();

        refreshing = false;
    }

    private void BuildResolutionOptions(int currentWidth, int currentHeight)
    {
        resolutions.Clear();

        // 모니터가 지원하는 해상도 중 크기가 같은 것은 하나로 합침
        foreach (Resolution r in Screen.resolutions)
        {
            var size = new Vector2Int(r.width, r.height);

            if (!resolutions.Contains(size))
                resolutions.Add(size);
        }

        // 현재 값이 목록에 없으면 추가 (창 모드에서 임의 크기일 때)
        var current = new Vector2Int(currentWidth, currentHeight);

        if (!resolutions.Contains(current))
            resolutions.Add(current);

        resolutions.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        var options = new List<string>();

        foreach (var size in resolutions)
            options.Add($"{size.x} x {size.y}");

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(resolutions.IndexOf(current));
        resolutionDropdown.RefreshShownValue();
    }

    private void BuildModeOptions(int currentMode)
    {
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string>(ModeNames));

        int index = 2;   // 목록에 없는 모드(MaximizedWindow 등)는 창 모드로 표시

        for (int i = 0; i < Modes.Length; i++)
        {
            if ((int)Modes[i] == currentMode)
                index = i;
        }

        screenModeDropdown.SetValueWithoutNotify(index);
        screenModeDropdown.RefreshShownValue();
    }

    private void UpdateLabels()
    {
        if (masterLabel != null) masterLabel.text = Mathf.RoundToInt(masterSlider.value * 100f) + "%";
        if (bgmLabel != null) bgmLabel.text = Mathf.RoundToInt(bgmSlider.value * 100f) + "%";
        if (sfxLabel != null) sfxLabel.text = Mathf.RoundToInt(sfxSlider.value * 100f) + "%";
        if (uiScaleLabel != null) uiScaleLabel.text = Mathf.RoundToInt(uiScaleSlider.value * 100f) + "%";
    }

    private void OnVolumeChanged()
    {
        if (refreshing) return;

        SettingManager.Instance.SetVolumes(masterSlider.value, bgmSlider.value, sfxSlider.value);
        UpdateLabels();
    }

    private void OnScreenChanged()
    {
        if (refreshing) return;

        int resIndex = Mathf.Clamp(resolutionDropdown.value, 0, resolutions.Count - 1);
        int modeIndex = Mathf.Clamp(screenModeDropdown.value, 0, Modes.Length - 1);

        Vector2Int size = resolutions[resIndex];

        SettingManager.Instance.SetScreen(size.x, size.y, Modes[modeIndex]);
    }

    private void OnUIScaleChanged(float value)
    {
        if (refreshing) return;

        SettingManager.Instance.SetUIScale(value);
        UpdateLabels();
    }

    private void ResetToDefault()
    {
        SettingManager.Instance.ResetToDefault();
        Refresh();
    }

    private void OnReturnTitleClicked()
    {
        returnConfirmText.text = OriginDataManager.Instance.GetMessage(WarningID.ReturnToTitleConfirm);
        returnConfirmPanel.SetActive(true);
    }

    private void OnReturnTitleConfirmed()
    {
        returnConfirmPanel.SetActive(false);

        SettingManager.Instance.Save();
        GameManager.Instance.ReturnToTitle();
    }

    public void Close()
    {
        SettingManager.Instance.Save();
        gameObject.SetActive(false);
    }
}
