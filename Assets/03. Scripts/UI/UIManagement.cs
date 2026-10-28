using System.Collections;
using UnityEngine;

public class UIManagement : MonoBehaviour
{
    [SerializeField] private GameObject management;

    public DataManager dataManager;
    public UIChart uiChart;
    public UIShop uiShop;
    public UIRobot uiRobot;

    [SerializeField] private GameObject[] chartChips;
    [SerializeField] private GameObject[] shopChips;
    [SerializeField] private GameObject[] NPCManagementChips;

    [SerializeField] private GameObject[] icon0;
    [SerializeField] private GameObject[] icon1;
    [SerializeField] private GameObject[] icon2;
    [SerializeField] private GameObject[] icon3;

    [SerializeField] private GameObject[] chipMenu;

    [SerializeField] private GameObject choiceB;

    /// <summary>
    /// index버튼은 어쩔 수 없이 버튼 4개로 나뉘기 때문에 자식 오브젝트 순서로 구분
    /// int 값으로 조정하되, 눈으로 보기 쉽게 enum으로 current chip 내용으로 구분하기.
    /// </summary>
    private enum Chip
    {
        None = -1,       //null 생성
        Chart = 0,
        Shop = 1,
        NPCManagement = 2
    }

    private Chip current = Chip.None;

    private GameObject[][] chips;
    private GameObject[][] icons;

    #region Unity
    private void Start()
    {
        chips = new GameObject[][] { chartChips, shopChips, NPCManagementChips };
        icons = new GameObject[][] { icon0, icon1, icon2, icon3 };

        ResetVisualState();

        ChangeChip(0);
        ChangeIcon(0);

        StartCoroutine(RefreshIconNextFrame());
    }

    // 칩/아이콘 처음 상태
    private void ResetVisualState()
    {
        foreach (var chip in chips)
        {
            chip[0].SetActive(true);
            chip[1].SetActive(false);
        }

        foreach (var icon in icons)
        {
            icon[0].SetActive(true);
            icon[1].SetActive(false);
            icon[2].SetActive(false);
        }
    }

    private IEnumerator RefreshIconNextFrame()
    {
        yield return null;

        ChangeIcon(0);
    }
    #endregion

    #region Open / Close
    public GameObject OpenManagement()
    {
        // 이미 열려있으면 닫기
        if (management.activeSelf)
        {
            ExitButton();
            return null;
        }

        // 다른 모드 중이면 열지 않음
        if (!GameManager.Instance.EnterMode(GameMode.Popup))
        {
            return null;
        }

        // 활성화는 호출한 쪽(UIManager)에서 처리
        return management;
    }

    public void ExitButton()
    {
        GameManager.Instance.ExitMode();
        ChangeIcon(0);
    }
    #endregion

    #region Chip
    public void ChangeChip(int value)
    {
        if (value < 0 || value >= chips.Length)
            return;

        current = (Chip)value;

        for (int i = 0; i < chips.Length; i++)
        {
            bool isActive = (i == value);

            chips[i][0].SetActive(!isActive);
            chips[i][1].SetActive(isActive);

            foreach (var icon in icons)
            {
                icon[i].SetActive(isActive);
            }

            chipMenu[i].SetActive(isActive);
        }
        
        ChangeIcon(0);
    }

    // 이름으로 칩 변경 및 이후 디테일 애니메이션 작업을 위한 초석
    // SetActive가 아닌 movetoward 연출 사용
    public void ChangeChip(string chipName)
    {
        string lowered = chipName.ToLower();

        if(lowered.Contains("chart"))
        {
            ChangeChip((int)Chip.Chart);
        }
        else if (lowered.Contains("shop"))
        {
            ChangeChip((int)Chip.Shop);
        }
        else if (lowered.Contains("npcmanage"))
        {
            ChangeChip((int)Chip.NPCManagement);
        }
    }
    #endregion

    #region Icon
    public void ChangeIcon(int value)
    {
        if (current == Chip.None)
            return;

        if (value < 0 || value >= icons.Length)
            return;

        // 사용 안 함: chart와 NPC 관리에는 네 번째 아이콘이 없음
        if (value == 3 && (current == Chip.Chart || current == Chip.NPCManagement))
            return;

        // 메뉴 페이지: chart의 즐겨찾기(1)와 shop, robot은 첫 번째 페이지를 그대로 사용
        bool usesFirstPage = (current == Chip.Chart && value == 1) || current != Chip.Chart;
        int page = usesFirstPage ? 0 : value;

        ShowMenuPage(page);
        ApplyIconToCurrentChip(value);

        choiceB.transform.position = icons[value][0].transform.position;
    }

    // 현재 칩의 메뉴에서 해당 페이지(자식)만 켬
    private void ShowMenuPage(int page)
    {
        Transform menu = chipMenu[(int)current].transform;

        for (int i = 0; i < menu.childCount; i++)
        {
            menu.GetChild(i).gameObject.SetActive(i == page);
        }
    }

    // 아이콘 선택을 칩별 화면에 전달
    private void ApplyIconToCurrentChip(int value)
    {
        switch (current)
        {
            case Chip.Chart:
                ApplyChartIcon(value);
                break;

            case Chip.Shop:
                uiShop.ViewShopItem(value);
                break;

            case Chip.NPCManagement:
                uiRobot.ViewRobot(value);
                break;
        }
    }

    private void ApplyChartIcon(int value)
    {
        switch (value)
        {
            case 0:
                uiChart.OffBookMark();   // 즐겨찾기 모드 해제
                break;

            case 1:
                uiChart.OnBookMark();    // 즐겨찾기 모드 사용
                break;

            case 2:
                uiChart.ChartNews();
                break;
        }
    }
    #endregion
}