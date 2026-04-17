using UnityEngine;
using TMPro;

public class TestInputM : MonoBehaviour
{
    // 테스트용 키 텍스트
    [SerializeField] private TMP_Text testKeyText;
    [SerializeField] private TimeManager timeManager;

    [SerializeField] private MarketManager marketManager;

    void Update()
    {
        // 임시 테스트용 키
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            testKeyText.text = "1번 키 입력\n시간 30분 추가";
            timeManager.TestAddTime(30);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            testKeyText.text = "2번 키 입력\n매일 이벤트 갱신(날짜변경X)";
            timeManager.CheckDayEvent();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            testKeyText.text = "3번 키 입력";
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            testKeyText.text = "4번 키 입력";
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            testKeyText.text = "5번 키 입력";
        }
    }
}
