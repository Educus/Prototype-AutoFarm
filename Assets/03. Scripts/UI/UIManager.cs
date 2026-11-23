using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //캔버스 UI
    #region Screen Space
    [SerializeField] private GameObject Canvas;                     // 캔버스 UI 관리

    [SerializeField] private GameObject ButtonManager;              // 버튼 UI 관리
    [SerializeField] private GameObject GUIManagement;              // GUI UI 관리
    [SerializeField] private GameObject BuildingMode;               // 건설 모드 UI 관리
    [SerializeField] private GameObject UIManagement;               // UI 관리
    [SerializeField] private GameObject UIStorageManagement;        // 창고 UI 관리
    [SerializeField] private GameObject TextUIManagement;           // 텍스트 UI 관리
    [SerializeField] private GameObject Setting;                    // 설정 UI 관리
    #endregion

    //Manager 오브젝트
    [SerializeField] private GameObject SoundManager;
    [SerializeField] private GameObject BuildingManager;

    //스크립트
    private SoundManager soundManager;
    private BuildingManager buildingManager;
    private ButtonManager buttonManager;
    private GUIManagement guiManagement;
    private UIBuildMod buildMod;
    private UIManagement uiManagement;
    private UIStorageManagement storageManagement;
    private UISettings settingsUI;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //초기화
        init();
    }

    //초기화 함수
    private void init()
    {
        //스크립트 초기화
        soundManager = SoundManager.GetComponent<SoundManager>();
        buildingManager = BuildingManager.GetComponent<BuildingManager>();
        buildMod = BuildingMode.GetComponent<UIBuildMod>();

        buttonManager = ButtonManager.GetComponent<ButtonManager>();
        guiManagement = GUIManagement.GetComponent<GUIManagement>();
        uiManagement = UIManagement.GetComponent<UIManagement>();   
        storageManagement = UIStorageManagement.GetComponent<UIStorageManagement>();
        settingsUI = Setting.GetComponent<UISettings>();

        //버튼 이벤트 초기화
        buttonManager.init();
    }

    public Button[] CollectButtons()
    {
        Button[] buttons = Canvas.GetComponentsInChildren<Button>(true);
        return buttons;
    }

    public void PlaySFX(string filename)
    {
        if (soundManager != null)
            soundManager.PlaySFX(filename);
    }

    // 설정창 열기/닫기. 다른 창이 열려 있으면 열지 않음
    public void OpenSettings()
    {
        if (settingsUI == null || GameManager.Instance.isTitle)
            return;

        if (settingsUI.IsOpen)
        {
            settingsUI.Close();
            return;
        }

        if (!GameManager.Instance.EnterMode(GameMode.Popup))
            return;

        settingsUI.gameObject.SetActive(true);
        PlaySFX(SFXName.PopUpOpen);
    }

    // UIManagement에서 관리 팝업을 열 때 스택에 추가하는 함수
    public void OpenPopUpPage()
    {
        GameObject popup = uiManagement.OpenManagement();

        if (popup != null)
        {
            popup.SetActive(true);
            PlaySFX(SFXName.PopUpOpen);
        }
    }

    public void ExitPopUpPage()
    {
        CloseCurrentPopup();
    }

    public bool CloseCurrentPopup()
    {
        if (settingsUI != null && settingsUI.IsOpen)
            return settingsUI.CloseTop();

        if (uiManagement.CloseTop())
            return true;

        if (storageManagement.IsAnyOpen)
        {
            storageManagement.CloseStorageManagement();
            return true;
        }

        return false;
    }

    // UIManagement에서 관리 팝업의 칩을 클릭해 변경하는 버튼 함수
    public void ChangeChip(string name)
    {
        uiManagement.ChangeChip(name);
    }

    // 건물 설치 시에 어떤 건물을 선택했는지 ID로 체크
    public void SelectBuildingKit(int itemID)
    {
        buildMod.OnClickBuildingButton(itemID);
    }

    public void ToggleBuildMod()
    {
        // 이미 BuildMode면 종료
        if (GameManager.Instance.IsMode(GameMode.Build))
        {
            buildingManager.ExitBuildMode();

            buildMod.BuildMode(false);

            return;
        }

        // 다른 모드 중이면 무시
        if (GameManager.Instance.IsBusy())
        {
            Debug.Log("다른 모드 진행 중");
            return;
        }

        // BuildMode 진입
        if (GameManager.Instance.EnterMode(GameMode.Build))
        {
            buildMod.BuildMode(true);
        }
    }
}
