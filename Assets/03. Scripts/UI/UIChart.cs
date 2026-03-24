using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIChart : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private RectTransform chartArea;
    [SerializeField] private List<GameObject> pointObj; // 7개
    [SerializeField] private List<GameObject> lineObj;  // 6개

    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemDescription;

    public void DrawChart(int itemID)
    {

        // 7일치 종가 데이터 가져오기 
        List<int> pointData = dataManager.productClosingData[itemID].productsClosingPrice;
        int countValue = pointData.Count;

        if (countValue == 0) return;        // 데이터가 없는 경우 종료

        float widthArea = chartArea.rect.width;
        float heightArea = chartArea.rect.height;

        float cellWidth = widthArea / 8f;   // 8등분 (7포인트 사이의 간격, 각 끝 포인트는 사용x)

        float basicCost = dataManager.productsData[itemID].basicCost;
        float minCost = basicCost * 0.7f;   // 최소 가격 : 기본 가격의 70%
        float maxCost = basicCost * 1.3f;   // 최대 가격 : 기본 가격의 130%

        Vector2 prevPos = Vector2.zero;
        int prevValue = 0;

        for (int i = 0; i < countValue; i++)
        {
            int dataValue = pointData[i]; // 최신 데이터부터 역순

            // Y 정규화 및 범위 제한
            float normalizedY = (dataValue - minCost) / (maxCost - minCost);
            normalizedY = Mathf.Clamp01(normalizedY);

            // X축 우측 정렬
            float xPos = (-3 + (7 - countValue) + i) * cellWidth;

            // Y축 위치 계산
            float yPos = (normalizedY - 0.5f) * heightArea;

            Vector2 pos = new Vector2(xPos, yPos);

            // 포인트 설정
            var point = pointObj[i];
            var pointRect = point.GetComponent<RectTransform>();
            pointRect.anchoredPosition = pos;

            var pointImage = point.GetComponent<UnityEngine.UI.Image>();
            if (pointImage != null)
            {
                if (i == 0)
                    pointImage.color = Color.gray;
                else if (dataValue > prevValue)
                    pointImage.color = Color.red;
                else if (dataValue < prevValue)
                    pointImage.color = Color.blue;
                else
                    pointImage.color = Color.gray;
            }

            // 라인 설정
            if (i > 0)
            {
                SetLine(lineObj[i - 1], prevPos, pos);
                lineObj[i - 1].SetActive(true);
            }

            point.SetActive(true);

            prevPos = pos;
            prevValue = dataValue;
        }

        // 남은 포인트와 라인 비활성화
        for (int i = 0; i < 7; i++)
        {
            pointObj[i].SetActive(i < countValue);
        }
        for (int i = 0; i < 6; i++)
        {
            lineObj[i].SetActive(i < countValue - 1);
        }

        // 아이템 정보
        SetItemInfo(itemID, countValue);    
    }

    private void SetLine(GameObject line, Vector2 startPos, Vector2 endPos)
    {
        RectTransform rect = line.GetComponent<RectTransform>();

        Vector2 dir = endPos - startPos;
        float distance = dir.magnitude;

        rect.anchoredPosition = startPos;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        rect.rotation = Quaternion.Euler(0, 0, angle);

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, distance);

        // 색상 설정 (가격 상승은 빨간색, 하락은 파란색, 동일하면 회색)
        var image = line.GetComponent<Image>();
        if (image != null)
        {
            if (endPos.y > startPos.y)
                image.color = Color.red;
            else if (endPos.y < startPos.y)
                image.color = Color.blue;
            else
                image.color = Color.gray;
        }
    }

    private void SetItemInfo(int itemID, int countValue)
    {
        var itemData = dataManager.itemsData[itemID];
        var nowPrice = dataManager.productClosingData[itemID].productsClosingPrice[countValue - 1];
        
        Color priceColor = pointObj[countValue - 1].GetComponent<Image>().color;
        string colorHex = ColorUtility.ToHtmlStringRGB(priceColor);

        var priceText = priceColor == Color.red ? "▲" : priceColor == Color.blue ? "▼" : "■";
        Debug.Log(priceColor);

        // 아이템 이미지
        // itemImage.sprite = // 으... 아이템 이미지... 데이터 테이블...

        // 아이템 텍스트
        itemDescription.text = 
            $"{itemData.itemName}\n" +
            $"<color=#{colorHex}>{nowPrice}{priceText}</color>\n" +
            $"보유수량:XX";
    }
}
