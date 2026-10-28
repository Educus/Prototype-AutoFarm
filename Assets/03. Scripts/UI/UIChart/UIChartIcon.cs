using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChartIcon : MonoBehaviour
{
    private DataManager dataManager;
    private UIChart uiChart;

    private ProductSubData productSubData;
    private int itemID;

    private Button button;

    [SerializeField] private GameObject[] backGround;   // 가격 상승 하락 배경 이미지
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemPrice;
    [SerializeField] private GameObject[] bookMark;     // 북마크 이미지

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    #region 시세 갱신 구독

    private void OnEnable()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.onHourEvent += OnHourChanged;

        // 숨겨져 있던 동안 바뀐 시세를 반영 (GetInfo가 호출되기 전에는 건너뜀)
        if (dataManager != null)
            SetData();
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.onHourEvent -= OnHourChanged;
    }

    private void OnHourChanged(int hour)
    {
        if (dataManager != null)
            SetData();
    }

    #endregion

    public void GetInfo(DataManager data, UIChart ui,int id)
    {
        dataManager = data;
        productSubData = dataManager.productSubData[id];
        uiChart = ui;
        itemID = id;

        SetInfo();
        SetData();
        SetBookMark();

        Debug.Log($"UIChartIcon GetInfo: {itemID}");
    }
    // 최초 정보 설정
    private void SetInfo()
    {
        button.onClick.AddListener(() => uiChart.OnClickChartButton(itemID));
        itemImage.sprite = dataManager.GetItemImage(itemID);
        itemName.text = dataManager.productsData[itemID].name;
    }

    // 데이터 갱신
    public void SetData()
    {
        if (!dataManager.productClosingData.TryGetValue(itemID, out ProductClosing closing) || !closing.HasData)
        {
            // 시세가 아직 만들어지지 않았음
            return;
        }

        int nowPrice = closing.CurrentPrice;
        int diff = nowPrice - closing.PreviousClose;

        itemPrice.text = nowPrice.ToString();

        // 하락 / 상승 배경 (같으면 둘 다 꺼서 이전 표시가 남지 않게 함)
        backGround[0].SetActive(diff < 0);
        backGround[1].SetActive(diff >= 0);

        if (diff < 0)
        {
            itemPrice.text += "<sprite=0>";
        }
        else if (diff > 0)
        {
            itemPrice.text += "<sprite=1>";
        }
    }

    public void BookMark()
    {
        productSubData.OnOffBookMark();
        bookMark[0].SetActive(!productSubData.isBookMarked);
        bookMark[1].SetActive(productSubData.isBookMarked);
    }
    private void SetBookMark()
    {
        productSubData.GetBookMark();
        bookMark[0].SetActive(!productSubData.isBookMarked);
        bookMark[1].SetActive(productSubData.isBookMarked);
    }

    public void OnBookMark()
    {
        gameObject.SetActive(productSubData.isBookMarked);
    }
    public void OffBookMark()
    {
        gameObject.SetActive(true);
    }
}
