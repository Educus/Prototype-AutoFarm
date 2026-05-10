using UnityEngine;

public class UIStorageManagement : MonoBehaviour
{
    [SerializeField] private GameObject rocketStorage;
    [SerializeField] private UINPCInterface npcInterface;
    [SerializeField] private GameObject htdroStorage;
    [SerializeField] private GameObject buildingStorage;
    [SerializeField] private GameObject npcStorage;

    public static UIStorageManagement Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RocketInv()
    {
        rocketStorage.SetActive(!rocketStorage.activeSelf);
        npcInterface.ShowStaff();
    }
    public void HtdroInv()
    {
        htdroStorage.SetActive(!htdroStorage.activeSelf);
    }
    public void BuildingInv()
    {
        buildingStorage.SetActive(!buildingStorage.activeSelf);
    }
    public void NPCInv()
    {
        npcStorage.SetActive(!npcStorage.activeSelf);
    }

}
