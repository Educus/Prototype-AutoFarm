using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// eventMenu(오른쪽 목록)의 한 칸 : 상품 이미지 + 색상만 표시. 클릭하면 mainevent 에 상세 내용을 보여줌
// 프리팹 구성 : 루트에 Button + 이 스크립트, 자식에 배경 Image(색상)와 상품 Image
[RequireComponent(typeof(Button))]
public class UIEventMenuItem : MonoBehaviour
{
    [RequiredRef, SerializeField] private Image backGround;   // 상승 / 하락 이미지가 적용될 배경
    [RequiredRef, SerializeField] private Image itemImage;
    [RequiredRef, SerializeField] private TMP_Text titleText;

    [RequiredRef, SerializeField] private Sprite upSprite;
    [RequiredRef, SerializeField] private Sprite downSprite;
    
    private Button button;
    private MarketNews news;
    private Action<UIEventMenuItem> onClick;

    public MarketNews News => news;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => onClick?.Invoke(this));
    }

    public void SetNews(MarketNews value, string title, Sprite sprite, bool isUp, Action<UIEventMenuItem> clickCallback)
    {
        news = value;
        onClick = clickCallback;

        titleText.text = title;
        itemImage.sprite = sprite;
        backGround.sprite = isUp ? upSprite : downSprite;
    }
}
