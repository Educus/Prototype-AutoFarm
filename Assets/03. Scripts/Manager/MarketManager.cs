using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    // 가격변동을 담당
    [SerializeField] DataManager dataManager;
    [SerializeField] TimeManager timeManager;

    [Tooltip("평균값")]    // 기본값이 0이며, 이벤트에 따라 상승 및 하락 (이벤트 처리)
    public float average { get; private set; } = 0;
    [Tooltip("표준편차")]  // 작물의 등급에 따라 고벨류작물은 높고 저벨류작물은 낮음
    public float standard { get; private set; } = 0;
    [Tooltip("난수")]     // (이벤트 처리)
    public float randomNum { get; private set; } = 0;

    [Tooltip("종가")]
    public float closingPrice { get; private set; } = 0;
    [Tooltip("시가")]
    public float marketPrice { get; private set; } = 0;

    private void Start()
    {
        // 타임매니저에서 일주일마다 가격 변동 이벤트 발생
        timeManager.onWeekEvent += UpdatePrice;
    }
    private void Update()
    {
        // 테스트
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("가격 변동 이벤트");
            UpdatePrice();
        }
    }

    public List<int> testList = new List<int> { 0, 1, 2, 3, 0, 1, 2 };

    public void UpdatePrice()
    {

        foreach (var pair in dataManager.productClosingData)
        {
            // 데이터가 없을 경우
            if (pair.Value.productsPriorPrice.Count == 0)
            {
                // 초기 가격 설정 (기본 가격)
                pair.Value.productsPriorPrice.Add(dataManager.productsData[pair.Key].basicCost);
            }

            // 가격 변동(수정 예정)
            // 가격 변동 후 = 변동 전 * (1 + 평균값 + 표준편차 * 난수)
            marketPrice = closingPrice * (1 + average + standard * randomNum);

            // 종가 업데이트
            pair.Value.productsPriorPrice.Add((int)marketPrice);

            // 7일치 가격만 저장
            if (pair.Value.productsPriorPrice.Count > 7)
            {
                pair.Value.productsPriorPrice.RemoveAt(0);
            }
        }
    }
}


/*
    ## 개요

    농장에서 생성된 아이템은 게임 내 시간 기준으로 일 주일 마다 상승장이나 하락장이 결정되며, 1일에 한 번 가격이 변동된다. 
    정규 분포를 활용하여 이벤트에 따른 변동 가격 수준과 변동 폭을 조절한다.

    ## 가격 변동 공식

    평균 = average 
    표준편차 = standard
    난수 = randomNum

    시가 = 전일 종가 × (1 + 평균 + 표준편차 × 난수)
 */