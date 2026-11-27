using TMPro;
using UnityEngine;
using UnityEngine.UI;

// mainevent(왼쪽 큰 화면) : 선택한 뉴스의 이미지 / 색상 / 제목 / 내용
public class UIEventView : MonoBehaviour
{
    [RequiredRef, SerializeField] private Image backGround;
    [RequiredRef, SerializeField] private Image itemImage;
    [RequiredRef, SerializeField] private TMP_Text titleText;
    [RequiredRef, SerializeField] private TMP_Text contentsText;

    [RequiredRef, SerializeField] private Sprite upSprite;
    [RequiredRef, SerializeField] private Sprite downSprite;

    public void Show(string title, string contents, Sprite sprite, bool isUp)
    {
        SetContentActive(true);

        backGround.sprite = isUp ? upSprite : downSprite;
        itemImage.sprite = sprite;
        titleText.text = title;
        contentsText.text = contents;
    }

    public void Clear()
    {
        SetContentActive(false);
    }

    private void SetContentActive(bool hasNews)
    {
        backGround.gameObject.SetActive(hasNews);
        itemImage.gameObject.SetActive(hasNews);
        titleText.gameObject.SetActive(hasNews);
        contentsText.gameObject.SetActive(hasNews);
    }
}
