using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChart : MonoBehaviour
{
    [Header("Chart")]
    [RequiredRef, SerializeField] private UIChartView uiChart;

    [RequiredRef, SerializeField] private GameObject chartIconPrefab;
    [RequiredRef, SerializeField] private GameObject chartIconContent;

    public event Action eventOnBookMark;
    public event Action eventOffBookMark;

    [Header("Event")]
    [RequiredRef, SerializeField] private UIEventView eventMain;
    [RequiredRef, SerializeField] private ScrollRect eventMenuScroll;
    [RequiredRef, SerializeField] private UIEventMenuItem eventMenuItemPrefab;

    private readonly List<UIEventMenuItem> eventMenuItems = new List<UIEventMenuItem>();

    #region Unity
    void Start()
    {
        StartCoroutine(IEUIChartStart());
    }

    private IEnumerator IEUIChartStart()
    {
        // product의 아이템 수만큼 게임 오브젝트 생성
        yield return null;

        OriginDataManager origin = OriginDataManager.Instance;

        foreach (var product in origin.productsData.Values)
        {
            if (!origin.TryGetItem(product.ID, out ItemData item) || !item.UseToDemo)
                continue;

            GameObject chartIcon = Instantiate(chartIconPrefab, chartIconContent.transform);
            UIChartIcon icon = chartIcon.GetComponent<UIChartIcon>();
            icon.GetInfo(this, product.ID);

            eventOnBookMark += () => icon.OnBookMark();
            eventOffBookMark += () => icon.OffBookMark();
        }
    }

    private void OnEnable()
    {
        uiChart.gameObject.SetActive(false);

        MarketManager.onNewsChanged += RefreshEventMenu;

        if (DataManager.Instance != null)
            RefreshEventMenu();
    }

    private void OnDisable()
    {
        MarketManager.onNewsChanged -= RefreshEventMenu;
    }
    #endregion

    #region Chart
    public void OnClickChartButton(int itemID)
    {
        uiChart.gameObject.SetActive(true);

        uiChart.DrawChart(itemID);
    }

    public void OnBookMark()
    {
        eventOnBookMark?.Invoke();
    }
    public void OffBookMark()
    {
        eventOffBookMark?.Invoke();
    }

    // 3번 기능
    public void ChartNews()
    {
        RefreshEventMenu();
    }
    #endregion

    #region Event
    private void RefreshEventMenu()
    {
        if (DataManager.Instance == null || DataManager.Instance.MarketManager == null)
            return;

        MarketManager market = DataManager.Instance.MarketManager;

        foreach (UIEventMenuItem item in eventMenuItems)
            Destroy(item.gameObject);

        eventMenuItems.Clear();

        OriginDataManager origin = OriginDataManager.Instance;

        foreach (MarketNews news in market.TodayNews)
        {
            UIEventMenuItem item = Instantiate(eventMenuItemPrefab, eventMenuScroll.content);

            market.TryGetNewsText(news, out string title, out _);

            item.SetNews(news, title, origin.GetItemImage(news.itemID), IsEventUp(news.eventID), OnClickEventMenuItem);

            eventMenuItems.Add(item);
        }

        if (eventMenuItems.Count > 0)
            OnClickEventMenuItem(eventMenuItems[0]);
        else
            eventMain.Clear();

        // 목록 맨 위로
        eventMenuScroll.verticalNormalizedPosition = 1f;
    }

    private void OnClickEventMenuItem(UIEventMenuItem item)
    {
        MarketNews news = item.News;

        if (!DataManager.Instance.MarketManager.TryGetNewsText(news, out string title, out string contents))
        {
            eventMain.Clear();
            return;
        }

        eventMain.Show(title, contents, OriginDataManager.Instance.GetItemImage(news.itemID), IsEventUp(news.eventID));
    }

    private bool IsEventUp(int eventID)
    {
        return !(OriginDataManager.Instance.TryGetEvent(eventID, out EventData data) && data.Average < 0f);
    }
    #endregion
}
