using UnityEngine;
using System;

public class MarketManager : MonoBehaviour
{
    // 가격변동을 담당
    [SerializeField] DataManager dataManager;

    [Tooltip("평균값")]    // 기본값이 0이며, 이벤트에 따라 상승 및 하락
    public float average { get; private set; } = 0;
    [Tooltip("표준편차")]  // 작물의 등급에 따라 고벨류작물은 높고 저벨류작물은 낮음
    public float standard { get; private set; } = 0;
    [Tooltip("난수")]     
    public float randomNum { get; private set; } = 0;

    [Tooltip("종가")]
    public float closingPrice { get; private set; } = 0;
    [Tooltip("시가")]
    public float marketPrice { get; private set; } = 0;


    public void UpdatePrice()
    {
        // 가격 변동 후 = 변동 전 * (1 + 평균값 + 표준편차 * 난수)
        marketPrice = closingPrice * (1 + average + standard * randomNum);

        foreach (var pair in dataManager.cropsDict)
        {
            // pair.Value.price = (int)(pair.Value.price * (1 + average + pair.Value.cropsClass * randomNum));
            pair.Value.price = (int)(pair.Value.price * (1 + 1 + pair.Value.cropsClass * 1));
            print(pair.Value.price);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            UpdatePrice();
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