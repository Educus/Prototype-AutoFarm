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
    public GameObject RocketStorage => rocketStorage;

    public bool IsNPCInvOpen => npcStorage != null && npcStorage.activeSelf;
    public bool IsAnyOpen =>
        rocketStorage.activeSelf ||
        htdroStorage.activeSelf ||
        buildingStorage.activeSelf ||
        npcStorage.activeSelf;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #region Open

    public void RocketInv()
    {
        if (!OpenPopup())
            return;

        SoundManager.Play(SFXName.PopUpOpen);

        rocketStorage.SetActive(true);
        buildingStorage.SetActive(true);

        npcInterface.ShowStaff();
    }

    public void HtdroInv()
    {
        if (!OpenPopup())
            return;

        SoundManager.Play(SFXName.PopUpOpen);

        htdroStorage.SetActive(true);
    }

    public void BuildingInv()
    {
        if (!OpenPopup())
            return;

        SoundManager.Play(SFXName.PopUpOpen);

        rocketStorage.SetActive(false);
        buildingStorage.SetActive(true);
    }

    public void NPCInv()
    {
        if (!OpenPopup())
            return;

        SoundManager.Play(SFXName.PopUpOpen);

        npcStorage.SetActive(true);
    }

    #endregion

    #region Close
    private void ReleaseNPCCamera()
    {
        GameManager gameManager = GameManager.Instance;

        if (gameManager.selectedNPC != null && gameManager.targetLock == gameManager.selectedNPC.gameObject)
            gameManager.targetLock = null;
    }

    public void CloseStorageManagement()
    {
        if (npcStorage.activeSelf)
            ReleaseNPCCamera();

        rocketStorage.SetActive(false);
        buildingStorage.SetActive(false);
        npcInterface.OffUI();

        htdroStorage.SetActive(false);

        npcStorage.SetActive(false);

        CheckPopupState();

        targetBuilding = null;
        GameManager.Instance.ExitMode();

        SoundManager.Play(SFXName.PopUpClose);
    }

    public void CloseRocketInv()
    {
        rocketStorage.SetActive(false);
        buildingStorage.SetActive(false);
        npcInterface.OffUI();

        CheckPopupState();

        targetBuilding = null;
        GameManager.Instance.ExitMode();

        SoundManager.Play(SFXName.PopUpClose);
    }

    public void CloseHtdroInv()
    {
        htdroStorage.SetActive(false);

        CheckPopupState();

        GameManager.Instance.ExitMode();

        SoundManager.Play(SFXName.PopUpClose);
    }

    public void CloseNPCInv()
    {
        ReleaseNPCCamera();

        npcStorage.SetActive(false);

        CheckPopupState();

        GameManager.Instance.ExitMode();

        SoundManager.Play(SFXName.PopUpClose);
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
    }
    #endregion

    public void TargetBuilding(string buildingName)
    {
        targetBuilding = buildingName;
    }
}