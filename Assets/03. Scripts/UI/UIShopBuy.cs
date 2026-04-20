using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopBuy : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemPrice;
    [SerializeField] private TMP_Text itemTotalPrice;

    [SerializeField] private TMP_InputField priceNum;   // 수량 입력 필드
    [SerializeField] private Button priceNumButton;  // 수량 입력 필드 버튼
    [SerializeField] private Button plusButton;   // + 버튼
    [SerializeField] private Button minusButton;  // - 버튼
    [SerializeField] private Button buyButton;   // 구매 버튼
    [SerializeField] private Button exitButton;   // 끄기 버튼

    private void Awake()
    {
        // 버튼 클릭 이벤트 등록
        plusButton.onClick.AddListener(() => ChoiceNum(true));
        minusButton.onClick.AddListener(() => ChoiceNum(false));
        buyButton.onClick.AddListener(() => BuyItem());
        exitButton.onClick.AddListener(Exit);
    }

    private void Update()
    {
        // 가격 정보 갱신
        priceNum.text = Mathf.Clamp(int.Parse(priceNum.text), 1, 999).ToString();
        itemTotalPrice.text = (int.Parse(itemPrice.text) * int.Parse(priceNum.text)).ToString();
    }


    // 팝업 시 아이템 정보 띄우기
    public void SetItem(ItemData itemData, Sprite itmeImage)
    {
        priceNum.text = "1";  // 수량 초기화

        itemImage.sprite = itmeImage;
        itemName.text = itemData.itemName;
        itemPrice.text = itemData.basicPrice.ToString();
        itemTotalPrice.text = (itemData.basicPrice * int.Parse(priceNum.text)).ToString();
    }

    // +, - 버튼 클릭 시 수량 조절
    public void ChoiceNum(bool isPlus)
    {
        int num = int.Parse(priceNum.text);

        if (isPlus)
        {
            num++;
        }
        else
        {
            num--;
        }

        priceNum.text = num.ToString();
    }

    // 구매 버튼 클릭 시
    public void BuyItem()
    {


        Exit();
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}
