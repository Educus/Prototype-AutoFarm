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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RocketInv()
    {
        rocketStorage.SetActive(!rocketStorage.activeSelf);
        npcInterface.ShowStaff();

        PopUpMode(rocketStorage);
    }
    public void HtdroInv()
    {
        htdroStorage.SetActive(!htdroStorage.activeSelf);

        PopUpMode(htdroStorage);
    }
    public void BuildingInv()
    {
        RocketInv();

        return;
        buildingStorage.SetActive(!buildingStorage.activeSelf);

        PopUpMode(buildingStorage);
    }

    public void TargetBuilding(string buildingName)
    {
        targetBuilding = buildingName;
    }

    public void NPCInv()
    {
        npcStorage.SetActive(!npcStorage.activeSelf);

        PopUpMode(npcStorage);
    }

    private void PopUpMode(GameObject obj)
    {
        GameManager.Instance.isPopUpMode = obj.activeSelf;
    }

}
