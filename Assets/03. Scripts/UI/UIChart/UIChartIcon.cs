using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChartIcon : MonoBehaviour
{
    private UIChart uiChart;

    private int itemID;
    private bool ready;

    private Button button;

    // [0] 하락, [1] 유지, [2] 상승
    [RequiredRef, SerializeField] private GameObject[] backGround;
    [RequiredRef, SerializeField] private Image itemImage;
    [RequiredRef, SerializeField] private TMP_Text itemName;
    [RequiredRef, SerializeField] private TMP_Text itemPrice;
    [RequiredRef, SerializeField] private GameObject bookMark;     // 북마크 이미지(0 off, 1 on)

    private PlayerDataManager Player => DataManager.Instance.PlayerDataManager;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (backGround == null || backGround.Length < 3)
            Debug.LogError("[UIChartIcon] : backGround의 3개 필요", this);
    }

    #region 시세 갱신 구독
    private void OnEnable()
    {
        MarketManager.onPricesUpdated += OnPricesUpdated;

        if (DataManager.Instance != null && DataManager.Instance.PlayerDataManager != null)
            DataManager.Instance.PlayerDataManager.OnBookmarksChanged += RefreshBookMarkIcon;

        if (ready)
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
        if (ready) 
            SetData();
    }
    #endregion

    public void GetInfo(UIChart ui,int id)
    {
        uiChart = ui;
        itemID = id;

        SetInfo();

        ready = true;

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
        if (!DataManager.Instance.productClosingData.TryGetValue(itemID, out ProductClosing closing) || !closing.HasData)
        {
            // 시세가 아직 만들어지지 않았음
            return;
        }

        int nowPrice = closing.CurrentPrice;
        int diff = nowPrice - closing.PreviousClose;

        itemPrice.text = nowPrice.ToString();

        // 하락 / 유지 / 상승 배경
        backGround[0].SetActive(diff < 0);
        backGround[1].SetActive(diff == 0);
        backGround[2].SetActive(diff > 0);

        if (diff < 0)
        {
            itemPrice.text += "<sprite=1>";
        }
        else if (diff > 0)
        {
            itemPrice.text += "<sprite=0>";
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

        bookMark.SetActive(marked);
    }

    // 북마크 필터
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
