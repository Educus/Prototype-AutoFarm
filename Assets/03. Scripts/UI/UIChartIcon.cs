using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChartIcon : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private UIChart uIChart;

    private int itemID;

    private Button button;

    [SerializeField] private GameObject[] backGround;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemPrice;
    [SerializeField] private GameObject[] bookMark;

    public bool isBookMark;

    private void Awake()
    {
        button = GetComponent<Button>();
        isBookMark = true;
        BookMark();
    }

    public void GetInfo(DataManager data, UIChart ui,int id)
    {
        dataManager = data;
        uIChart = ui;
        itemID = id;

        SetInfo();
        SetData();
    }
    // 최초 정보 설정
    private void SetInfo()
    {
        button.onClick.AddListener(() => uIChart.OnClickChartButton(itemID));
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
        isBookMark = !isBookMark;
        bookMark[0].SetActive(!isBookMark);
        bookMark[1].SetActive(isBookMark);
    }

    public void OnBookMark()
    {
        gameObject.SetActive(isBookMark);
    }
    public void OffBookMark()
    {
        gameObject.SetActive(true);
    }
}
