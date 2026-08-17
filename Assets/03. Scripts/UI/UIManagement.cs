using System.Collections;
using UnityEngine;

public class UIManagement : MonoBehaviour
{
    [SerializeField] private GameObject management;
    [SerializeField] public DataManager dataManager;
    [SerializeField] public UIChart uiChart;
    [SerializeField] public UIShop uiShop;
    [SerializeField] public UIRobot uiRobot;

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
    private enum CurrentInsertedChip
    {
        None = -1,       //null 생성
        Chart = 0,
        Shop = 1,
        NPCManagement = 2
    }
    CurrentInsertedChip current;

    private GameObject[][] chips;
    private GameObject[][] icons;

    private int onChips;

    private void Start()
    {
        chips = new GameObject[][] { chartChips, shopChips, NPCManagementChips };
        icons = new GameObject[][] { icon0, icon1, icon2, icon3 };

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

        current = CurrentInsertedChip.None;

        ChangeChip(0);
        ChangeIcon(0);

        StartCoroutine(IEStart());
    }

    IEnumerator IEStart()
    {
        yield return null;

        ChangeIcon(0);
    }

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

        //management.SetActive(true);
        return management;
    }

    public void ChangeChip(int value)
    {
        onChips = value;

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

            ChangeIcon(0);
        }
    }

    // 이름으로 칩 변경 및 이후 디테일 애니메이션 작업을 위한 초석
    // SetActive가 아닌 movetoward 연출 사용
    public void ChangeChip(string name)
    {
        string chipname = name.ToLower();

        if(chipname.Contains("chart"))
        {
            current = CurrentInsertedChip.Chart;
            ChangeChip(0);
        }
        else if (chipname.Contains("shop"))
        {
            current = CurrentInsertedChip.Shop;
            ChangeChip(1);
        }
        else if (chipname.Contains("npcmanage"))
        {
            current = CurrentInsertedChip.NPCManagement;
            ChangeChip(2);
        }
    }

    public void ChangeIcon(int value)
    {
        int count = chipMenu[onChips].transform.childCount;

        int index = value;

        // 사용안함
        if (value == 3 && (onChips == 0 || onChips == 2)) return;

        // chart 즐겨찾기 || shop || robot
        if ((value == 1 && onChips == 0) || onChips != 0)
        {
            index = 0;
        }

        for (int i = 0; i < count; i++)
        {
            chipMenu[onChips].transform.GetChild(i).gameObject.SetActive(i == index);
        }

        if (onChips == 0)
        {
            if (value == 0)
            {
                // 즐겨찾기 모드 해제
                uiChart.OffBookMark();
            }

            else if (value == 1)
            {
                // 즐겨찾기 모드 사용
                uiChart.OnBookMark();
            }

            else if (value == 2)
            {
                uiChart.ChartNews();
            }
        }

        if (onChips == 1)
        {
            uiShop.ViewShopItem(value);
        }

        if (onChips == 2)
        {
            uiRobot.ViewRobot(value);
        }

        choiceB.transform.position = icons[value][0].transform.position;
    }

    public void ExitButton()
    {
        //management.SetActive(false);
        GameManager.Instance.ExitMode();
        ChangeIcon(0);
    }
}
