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
    [SerializeField] private Button plusButton;   // + 버튼
    [SerializeField] private Button minusButton;  // - 버튼
    [SerializeField] private Button buyButton;   // 구매 버튼
    [SerializeField] private Button exitButton;   // 끄기 버튼

    private int currentNum;
    private int currentPrice;
    private int maxBuyCount;
    private ItemData currentItem;

    private void Awake()
    {
        plusButton.onClick.AddListener(() => ChoiceNum(true));
        minusButton.onClick.AddListener(() => ChoiceNum(false));

        buyButton.onClick.AddListener(BuyItem);
        exitButton.onClick.AddListener(Exit);

        // 입력 완료 시 호출
        priceNum.onEndEdit.AddListener(OnInputChanged);

        // 숫자만 입력 허용
        priceNum.contentType = TMP_InputField.ContentType.IntegerNumber;
    }

    // 팝업 시 아이템 정보 띄우기
    public void SetItem(ItemData itemData, Sprite itemSprite)
    {
        itemImage.sprite = itemSprite;
        itemName.text = itemData.name;

        currentItem = itemData;
        currentPrice = itemData.basicPrice;

        itemPrice.text = currentPrice.ToString();

        // 현재 보유 골드
        int gold =
            DataManager.Instance.CurrencyManager.Get();

        // 최대 구매 가능 수량
        if (currentPrice <= 0)
        {
            // 무료 아이템
            maxBuyCount = 999;
        }
        else
        {
            maxBuyCount = gold / currentPrice;
        }

        // 구매 불가능
        if (maxBuyCount <= 0)
        {
            currentNum = 0;
        }
        else
        {
            currentNum = 1;
        }

        RefreshUI();
    }

    // InputField 입력 완료
    private void OnInputChanged(string value)
    {
        // 구매 불가능 상태
        if (maxBuyCount <= 0)
        {
            currentNum = 0;

            RefreshUI();
            return;
        }

        int result;

        if (string.IsNullOrEmpty(value))
        {
            result = 1;
        }
        else if (!int.TryParse(value, out result))
        {
            result = 1;
        }

        currentNum = Mathf.Clamp(result, 0, maxBuyCount);

        RefreshUI();
    }

    // +, - 버튼 클릭
    public void ChoiceNum(bool isPlus)
    {
        // 구매 불가능 상태
        if (maxBuyCount <= 0)
        {
            return;
        }

        if (isPlus)
        {
            currentNum++;
        }
        else
        {
            currentNum--;
        }

        currentNum = Mathf.Clamp(currentNum, 1, maxBuyCount);

        RefreshUI();
    }


    // UI 갱신
    private void RefreshUI()
    {
        priceNum.SetTextWithoutNotify(currentNum.ToString());

        itemTotalPrice.text = (currentPrice * currentNum).ToString();
    }

    // 구매 버튼 클릭
    public void BuyItem()
    {
        if (currentNum <= 0)
        {
            return;
        }

        int itemID = currentItem.ID;
        int totalPrice = currentPrice * currentNum;

        // 개발자용 아이템 예외 처리
        if (itemID == 5011) 
        {
            DataManager.Instance.CurrencyManager.AddMoney(100000 * currentNum); // 골드 10만 추가
            Exit();
            return;
        }

        ShopManager shop = DataManager.Instance.ShopManager;   // var storage = ... 대체

        // 영수증 칸 수 제한 (CanAddBuyItem 대체)
        if (!shop.CanAddToCart(itemID, currentNum))
        {
            Debug.Log("구매 가능한 슬롯 수를 초과했습니다.");
            return;
        }

        // 돈 부족 검사: 영수증에 이미 담긴 금액까지 포함 (결제는 로켓 출발 때)
        int available = DataManager.Instance.CurrencyManager.Get() - shop.GetCartTotal();

        if (available < totalPrice)
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        // 영수증에 추가 (buyItems 직접 수정 대체). 골드는 여기서 차감하지 않음
        shop.TryAddToCart(itemID, currentNum);

        Exit();
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}
