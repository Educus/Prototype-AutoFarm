using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITitle : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject[] hideWhileTitle;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Confirm")]
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Option")]
    [Range(0f, 1f)]
    [SerializeField] private float disabledAlpha = 0.4f;   // 불러오기 비활성일 때 투명도

    private CanvasGroup continueGroup;

    private SaveLoadManager SaveLoad =>
        DataTotalManager.Instance != null ? DataTotalManager.Instance.SaveLoad : null;

    private void Awake()
    {
        // 불러오기 버튼을 흐리게 만들기 위한 CanvasGroup (없으면 추가)
        continueGroup = continueButton.GetComponent<CanvasGroup>();

        if (continueGroup == null)
            continueGroup = continueButton.gameObject.AddComponent<CanvasGroup>();

        continueButton.onClick.AddListener(OnContinue);
        newGameButton.onClick.AddListener(OnNewGame);
        settingsButton.onClick.AddListener(OnSettings);
        quitButton.onClick.AddListener(OnQuit);

        confirmYesButton.onClick.AddListener(OnConfirmNewGame);
        confirmNoButton.onClick.AddListener(() => confirmPanel.SetActive(false));
    }

    private void OnEnable()
    {
        ShowTitle();
    }

    private void ShowTitle()
    {
        titlePanel.SetActive(true);
        confirmPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        foreach (var obj in hideWhileTitle)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // 타이틀 동안은 게임을 진행하지 않음
        if (GameManager.Instance != null)
            GameManager.Instance.isPlay = false;

        RefreshContinue();
    }

    // 저장 파일이 있을 때만 불러오기 활성화
    private void RefreshContinue()
    {
        bool hasSave = SaveLoad != null && SaveLoad.HasSaveFile();

        continueButton.interactable = hasSave;
        continueGroup.alpha = hasSave ? 1f : disabledAlpha;
    }

    #region Buttons
    private void OnContinue()
    {
        if (SaveLoad == null || !SaveLoad.HasSaveFile())
            return;

        if (SaveLoad.ContinueGame())
            StartPlaying();
        else
            Debug.LogWarning("[UITitle] 저장 파일을 불러오지 못했습니다.");   // 파일이 깨진 경우. 타이틀에 남음
    }

    private void OnNewGame()
    {
        if (SaveLoad != null && SaveLoad.HasSaveFile())
        {
            confirmText.text = "저장된 게임이 있습니다.\n새 게임을 시작하면 기존 저장 파일이 삭제됩니다.\n계속할까요?";
            confirmPanel.SetActive(true);
            return;
        }

        StartNewGame();
    }

    private void OnConfirmNewGame()
    {
        confirmPanel.SetActive(false);

        if (SaveLoad != null)
            SaveLoad.DeleteSave();

        StartNewGame();
    }

    private void OnSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void OnQuit()
    {
        if (SaveLoad != null)
            SaveLoad.QuitWithoutSave();
        else
            Application.Quit();
    }
    #endregion

    private void StartNewGame()
    {
        // 씬의 처음 상태가 곧 새 게임의 초기 상태
        StartPlaying();
    }

    private void StartPlaying()
    {
        titlePanel.SetActive(false);

        foreach (var obj in hideWhileTitle)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        GameManager.Instance.StartPlay();
    }
}
