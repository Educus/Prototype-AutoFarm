using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingMenu : MonoBehaviour
{
    public static UIBuildingMenu Instance;

    [SerializeField] private RectTransform panel;
    [SerializeField] private Button destroyButton;
    [SerializeField] private TMP_Text reasonText;   // 선택: 파괴할 수 없는 이유를 표시
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 60f);   // 건물 중심에서 얼마나 위에 띄울지

    private BuildingBase target;

    // 메뉴를 연 프레임에는 같은 클릭으로 닫히지 않게 함
    private int shownFrame = -1;

    public bool IsOpen => panel.gameObject.activeSelf;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        destroyButton.onClick.AddListener(OnDestroyClicked);
        panel.gameObject.SetActive(false);
    }

    public void Show(BuildingBase building)
    {
        if (building == null)
            return;

        target = building;
        shownFrame = Time.frameCount;

        // 파괴할 수 없으면 버튼을 끄고 이유를 보여줌 (로켓, 기본 창고, 물건이 든 창고 등)
        string reason = building.GetDestroyBlockReason();

        destroyButton.interactable = reason == null;

        if (reasonText != null)
        {
            reasonText.gameObject.SetActive(reason != null);
            reasonText.text = reason ?? string.Empty;
        }

        // 건물 위치 위에 표시
        Vector3 screenPos = Camera.main.WorldToScreenPoint(building.transform.position);
        panel.position = screenPos + (Vector3)screenOffset;

        panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        target = null;
        panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        // 대상 건물이 사라졌거나 건설 모드가 끝났으면 닫음
        if (target == null || !GameManager.Instance.IsMode(GameMode.Build))
        {
            Hide();
            return;
        }

        // 메뉴 밖을 클릭하면 닫음
        if (Time.frameCount != shownFrame
            && Input.GetMouseButtonDown(0)
            && !IsPointerOverMenu())
        {
            Hide();
        }
    }

    private bool IsPointerOverMenu()
    {
        // Overlay 캔버스는 카메라가 null
        return RectTransformUtility.RectangleContainsScreenPoint(panel, Input.mousePosition, null);
    }

    private void OnDestroyClicked()
    {
        BuildingBase building = target;

        Hide();

        if (building != null)
            DataManager.Instance.BuildingManager.DestroyBuilding(building);
    }
}
