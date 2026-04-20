using System.Collections;
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

    public void GetInfo(DataManager data, UIChart ui,int id)
    {
        dataManager = data;
        productSubData = dataManager.productSubData[id];
        uiChart = ui;
        itemID = id;

        SetInfo();
        SetData();
        SetBookMark();
    }
    // 최초 정보 설정
    private void SetInfo()
    {
        button.onClick.AddListener(() => uiChart.OnClickChartButton(itemID));
        itemImage.sprite = dataManager.GetItemImage(itemID);
        itemName.text = dataManager.productsData[itemID].itemName;
    }

    // 데이터 갱신
    public void SetData()
    {
        int count = dataManager.productClosingData[itemID].productsClosingPrice.Count;

        int todayPrice = dataManager.productClosingData[itemID].productsClosingPrice[count - 1];
        int yesterdayPrice = dataManager.productClosingData[itemID].productsClosingPrice.Count > 1 ? dataManager.productClosingData[itemID].productsClosingPrice[count - 2] : todayPrice;

        itemPrice.text = todayPrice.ToString();

        if (todayPrice < yesterdayPrice)
        {
            backGround[0].SetActive(true);
            backGround[1].SetActive(false);

            itemPrice.text += "<sprite=0>";
        }
        else if (todayPrice > yesterdayPrice)
        {
            backGround[0].SetActive(false);
            backGround[1].SetActive(true);

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
