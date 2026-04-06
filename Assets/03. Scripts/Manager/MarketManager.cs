using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    // 가격변동을 담당
    [SerializeField] DataManager dataManager;
    [SerializeField] TimeManager timeManager;

    private void Start()
    {
        // 타임매니저에서 하루마다 가격 변동 이벤트 발생
        timeManager.onDayEvent += UpdatePrice;

        UpdatePrice();
    }

    public List<int> testList = new List<int> { 0, 1, 2, 3, 0, 1, 2 };

    public void UpdatePrice()
    {

        foreach (var pair in dataManager.productClosingData)
        {
            // 데이터가 없을 경우
            if (pair.Value.productsClosingPrice.Count == 0)
            {
                // 초기 가격 설정 (기본 가격)
                pair.Value.productsClosingPrice.Add(dataManager.productsData[pair.Key].basicCost);
            }
            else
            {
                // 가격 변동
                // 1.가격 변동 후 = 변동 전 * (1 + 평균값 + 표준편차 * 난수)
                // 2.가격 변동 후 = 변동 전 * e^(평균값 + 표준편차 * 난수)

                // 변동 전 가격
                float beforePrice = pair.Value.productsClosingPrice[pair.Value.productsClosingPrice.Count - 1];
                // 평균값
                float mean = 0f;
                // float mean = dataManager.eventsData[-1].average;
                // 표준편차
                float stdDev = dataManager.productsData[pair.Key].priceStdDev;

                // 변동 후 가격
                int newPrice = (int)GetNextPrice(beforePrice, mean, stdDev);

                // 종가 업데이트
                pair.Value.productsClosingPrice.Add(newPrice);

                // 7일치 가격만 저장
                if (pair.Value.productsClosingPrice.Count > 7)
                {
                    pair.Value.productsClosingPrice.RemoveAt(0);
                }
            }
        }
    }

    // 가격 변동 공식
    float GetNextPrice(float currentPrice, float mean, float stdDev)
    {
        float rand = GetStandardNormal();

        float exponent = mean + stdDev * rand;
        float multiplier = Mathf.Exp(exponent);

        // 변동 제한 (최대 20% 상승 또는 하락) // 필요 없으면 삭제
        // multiplier = Mathf.Clamp(multiplier, 0.8f, 1.2f);

        float newPrice = currentPrice * multiplier;

        // 최소 가격 제한 (1 이상)
        return Mathf.Max(1f, newPrice);
    }

    // 정규 분포 난수 생성 (Box-Muller 변환)
    float GetStandardNormal()
    {
        float u1 = 1.0f - UnityEngine.Random.value;
        float u2 = 1.0f - UnityEngine.Random.value;

        return Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
               Mathf.Sin(2.0f * Mathf.PI * u2);
    }
}