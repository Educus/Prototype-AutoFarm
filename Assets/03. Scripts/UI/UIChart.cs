using System.Collections;
using UnityEngine;

public class UIChart : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private UIChartView uiChart;

    [SerializeField] private GameObject chartIconPrefab;
    [SerializeField] private GameObject chartIconContent;

    void Start()
    {
        StartCoroutine(IEStart());
    }
    
    private IEnumerator IEStart()
    {
        // product의 아이템 수만큼 게임 오브젝트 생성
        // onDayEvent에 등록하여 매일 데이터 갱신
        yield return null;

        foreach (var product in dataManager.productsData.Values)
        {
            GameObject chartIcon = Instantiate(chartIconPrefab, chartIconContent.transform);
            chartIcon.GetComponent<UIChartIcon>().GetInfo(dataManager, this, product.itemID);

            timeManager.onDayEvent += () =>
            {
                chartIcon.GetComponent<UIChartIcon>().SetData();
            };
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            uiChart.DrawChart(dataManager.productsData[1022].itemID);
        }
    }

    public void OnClickChartButton(int itemID)
    {
        uiChart.gameObject.SetActive(true);

        uiChart.DrawChart(dataManager.productsData[itemID].itemID);
    }
}
