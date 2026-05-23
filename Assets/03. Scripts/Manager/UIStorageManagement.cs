using UnityEngine;

public class UIStorageManagement : MonoBehaviour
{
    [SerializeField] private GameObject rocketStorage;
    [SerializeField] private UINPCInterface npcInterface;
    [SerializeField] private GameObject htdroStorage;
    [SerializeField] private GameObject buildingStorage;
    [SerializeField] private GameObject npcStorage;

    public static UIStorageManagement Instance;
    public string targetBuilding { get; private set; }

    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
            Destroy(gameObject);
    }

    #region Open

    public void RocketInv()
    {
        if (!OpenPopup())
            return;

        rocketStorage.SetActive(true);
        buildingStorage.SetActive(true);

        npcInterface.gameObject.SetActive(true);
        npcInterface.ShowStaff();
    }

    public void HtdroInv()
    {
        if (!OpenPopup())
            return;

        htdroStorage.SetActive(true);
    }

    public void BuildingInv()
    {
        if (!OpenPopup())
            return;

        rocketStorage.SetActive(false);
        buildingStorage.SetActive(true);
    }

    public void NPCInv()
    {
        if (!OpenPopup())
            return;

        npcStorage.SetActive(true);
    }

    #endregion

    #region Close

    public void CloseRocketInv()
    {
        rocketStorage.SetActive(false);
        buildingStorage.SetActive(false);
        npcInterface.gameObject.SetActive(false);

        CheckPopupState();
    }

    public void CloseHtdroInv()
    {
        htdroStorage.SetActive(false);

        CheckPopupState();
    }

    public void CloseNPCInv()
    {
        npcStorage.SetActive(false);

        CheckPopupState();
    }

    #endregion

    #region Popup

    private bool OpenPopup()
    {
        // 이미 Popup 모드면 허용
        if (GameManager.Instance.IsMode(GameMode.Popup))
        {
            return true;
        }

        return GameManager.Instance.EnterMode(GameMode.Popup);
    }

    private void CheckPopupState()
    {
        bool hasPopup =
            rocketStorage.activeSelf ||
            htdroStorage.activeSelf ||
            buildingStorage.activeSelf ||
            npcStorage.activeSelf;

        if (!hasPopup)
        {
            GameManager.Instance.ExitMode();
        }
    }

    #endregion

    public void TargetBuilding(string buildingName)
    {
        targetBuilding = buildingName;
    }
}