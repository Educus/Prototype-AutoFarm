using UnityEngine;
using UnityEngine.UI;

public class UICandle : MonoBehaviour
{
    [SerializeField] private RectTransform wick;
    [SerializeField] private RectTransform body;

    // 세로 최소 두께(시가 = 종가 일때)
    [SerializeField] private float minBodyHeight = 2f;

    private RectTransform rect;
    private Image wickImage;
    private Image bodyImage;
    private bool initialized;

    private void Init()
    {
        if (initialized)
            return;

        rect = (RectTransform)transform;
        wickImage = wick.GetComponent<Image>();
        bodyImage = wick.GetComponent<Image>();

        Center(rect);
        Center(wick);
        Center(body);

        initialized = true;
    }

    private static void Center(RectTransform target)
    {
        target.anchorMin = new Vector2(0.5f, 0.5f);
        target.anchorMax = new Vector2(0.5f, 0.5f);
        target.pivot = new Vector2(0.5f, 0.5f);
    }

    // x : 차트 가운데 기준 가로 위치
    // y : 차트 가운데 기준 세로 위치
    public void Set(
        float x, 
        float bodyWidth, float wickWidth,
        float yOpen, float yClose, float yHigh, float yLow,
        Color color
        )
    {
        Init();

        // 가로는 여기서 정하고, 세로는 차트 기준 직접 배치
        rect.anchoredPosition = new Vector2(x, 0f);

        // 꼬리 : 고가 ~ 저가
        wick.anchoredPosition = new Vector2(0f, (yHigh + yLow) * 0.5f);
        wick.sizeDelta = new Vector2(wickWidth, Mathf.Max(yHigh - yLow, 1f));

        // 몸통 : 시가 ~ 종가
        float bodyHeight = Mathf.Max(Mathf.Abs(yClose - yOpen), minBodyHeight);
        body.anchoredPosition = new Vector2(0f, (yOpen + yClose) * 0.5f);
        body.sizeDelta = new Vector2(bodyWidth, bodyHeight);

        wickImage.color = color;
        bodyImage.color = color;
    }

}
