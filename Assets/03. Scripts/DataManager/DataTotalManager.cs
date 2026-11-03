using UnityEngine;

[DefaultExecutionOrder(-200)]
public class DataTotalManager : MonoBehaviour
{
    public static DataTotalManager Instance { get; private set; }

    [SerializeField] private OriginDataManager origin;
    [SerializeField] private DataManager data;
    [SerializeField] private SaveLoadManager saveLoad;

    public OriginDataManager Origin => origin;
    public DataManager Data => data;
    public SaveLoadManager SaveLoad => saveLoad;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (origin == null) Debug.LogError("DataTotalManager: OriginDataManager 연결이 없습니다.");
        if (data == null) Debug.LogError("DataTotalManager: DataManager 연결이 없습니다.");
        if (saveLoad == null) Debug.LogError("DataTotalManager: SaveLoadManager 연결이 없습니다.");
    }
}
