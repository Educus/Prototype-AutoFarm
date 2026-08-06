using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UIManager : MonoBehaviour
{
    //싱글톤 
    public static UIManager Instance;

    //캔버스 UI
    #region[Screen Space]
    [SerializeField] private GameObject Canvas;                     // 캔버스 UI 관리

    [SerializeField] private GameObject ButtonManager;              // 버튼 UI 관리
    [SerializeField] private GameObject GUIManagement;              // GUI UI 관리
    [SerializeField] private GameObject BuildingMode;               // 건설 모드 UI 관리
    [SerializeField] private GameObject UIManagement;               // UI 관리
    [SerializeField] private GameObject UIStorageManagement;        // 창고 UI 관리
    [SerializeField] private GameObject NPCInterface;               // NPC UI 관리
    [SerializeField] private GameObject TextUIManagement;           // 텍스트 UI 관리
    [SerializeField] private GameObject Setting;                    // 설정 UI 관리
    #endregion

    //오브젝트 UI
    #region[World Space]
    [SerializeField] private GameObject Player;                      // 플레이어 UI 관리
    [SerializeField] private GameObject NPC;                         // NPC UI 관리
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

    //UI 스택
    private Stack<GameObject> PopUpStack = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        //초기화
        init();
    }

    //초기화 함수
    private void init()
    {
        soundManager = SoundManager.GetComponent<SoundManager>();
        buildingManager = BuildingManager.GetComponent<BuildingManager>();
        buildMod = BuildingMode.GetComponent<UIBuildMod>();

        buttonManager = ButtonManager.GetComponent<ButtonManager>();
        guiManagement = GUIManagement.GetComponent<GUIManagement>();
        uiManagement = UIManagement.GetComponent<UIManagement>();   
        storageManagement = UIStorageManagement.GetComponent<UIStorageManagement>();
    }

    public Button[] CollectButtons()
    {
        Button[] buttons = Canvas.GetComponentsInChildren<Button>(true);
        return buttons;
    }

    public void PlaySFX(string filename)
    {
        soundManager.PlaySFX(filename);
    }

    public void OpenPopUpPage()
    {
        GameObject popup;
        popup = uiManagement.OpenManagement();
        if (popup != null)
        {
            popup.SetActive(true);
            PopUpStack.Push(popup);
        }
    }

    public void ExitPopUpPage()
    {
        GameObject popup;
        popup = PopUpStack.Pop();
        if (popup != null)
        {
            if(popup.transform.parent == UIManagement.transform)
            {
                popup.SetActive(false);
                uiManagement.ExitButton();
            }

            else if (popup.transform.parent == UIStorageManagement.transform)
            {
                storageManagement.CloseRocketInv();
            }
        }
    }

    public void InputPopUpOpen(GameObject popup)
    {
        PopUpStack.Push(popup);
    }

    public void SelectBuildingKit(int itemID)
    {
        buildMod.OnClickBuildingButton(itemID);
    }

    public void ToggleBuildMode()
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
