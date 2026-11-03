using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChartIcon : MonoBehaviour
{
    private DataManager dataManager;
    private UIChart uiChart;

    private int itemID;

    private Button button;

    [SerializeField] private GameObject[] backGround;   // 가격 상승 하락 배경 이미지(0하락, 1상승)
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemPrice;
    [SerializeField] private GameObject[] bookMark;     // 북마크 이미지(0 off, 1 on)

    private PlayerDataManager Player => DataManager.Instance.PlayerDataManager;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    #region 시세 갱신 구독

    private void OnEnable()
    {
        MarketManager.onPricesUpdated += OnPricesUpdated;

        if (DataManager.Instance != null && DataManager.Instance.PlayerDataManager != null)
            DataManager.Instance.PlayerDataManager.OnBookmarksChanged += RefreshBookMarkIcon;

        if (dataManager != null)
        {
            SetData();
            RefreshBookMarkIcon();
        }
    }

    private void OnDisable()
    {
        MarketManager.onPricesUpdated -= OnPricesUpdated;

        if (DataManager.Instance != null && DataManager.Instance.PlayerDataManager != null)
            DataManager.Instance.PlayerDataManager.OnBookmarksChanged -= RefreshBookMarkIcon;
    }

    private void OnPricesUpdated() 
    {
        if (dataManager != null) 
            SetData();
    }
    #endregion

    public void GetInfo(DataManager data, UIChart ui,int id)
    {
        dataManager = data;
        uiChart = ui;
        itemID = id;

        SetInfo();
        SetData();
        RefreshBookMarkIcon();
    }

    // 최초 정보 설정
    private void SetInfo()
    {
        OriginDataManager origin = OriginDataManager.Instance;

        button.onClick.AddListener(() => uiChart.OnClickChartButton(itemID));
        itemImage.sprite = origin.GetItemImage(itemID);
        itemName.text = origin.TryGetProduct(itemID, out Product product) ? 
            product.name : itemID.ToString();
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

        // 하락 / 상승 배경
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

    #region 북마크
    public void BookMark()
    {
        Player.ToggleBookmark(itemID);
    }
    private void RefreshBookMarkIcon()
    {
        bool marked = Player.IsBookmarked(itemID);

        bookMark[0].SetActive(!marked);
        bookMark[1].SetActive(marked);
    }

    public void OnBookMark()
    {
        gameObject.SetActive(Player.IsBookmarked(itemID));
    }
    public void OffBookMark()
    {
        gameObject.SetActive(true);
    }
    #endregion
}
