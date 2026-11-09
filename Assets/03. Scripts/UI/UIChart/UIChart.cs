using System;
using System.Collections;
using UnityEngine;

public class UIChart : MonoBehaviour
{
    [RequiredRef, SerializeField] private UIChartView uiChart;

    [RequiredRef, SerializeField] private GameObject chartIconPrefab;
    [RequiredRef, SerializeField] private GameObject chartIconContent;

    public event Action eventOnBookMark;
    public event Action eventOffBookMark;

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
    }

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

    }
}
