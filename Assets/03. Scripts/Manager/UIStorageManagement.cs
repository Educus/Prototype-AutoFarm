using UnityEngine;

public class UIStorageManagement : MonoBehaviour
{
    public static UIStorageManagement Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void NPCInv()
    {

    }

    public void StorageBuildingInv()
    {

    }
}
