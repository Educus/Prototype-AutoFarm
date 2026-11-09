using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIKeyBindingPanel : MonoBehaviour
{
    [RequiredRef, SerializeField] private UIKeyBindingRow rowPrefab;
    [RequiredRef, SerializeField] private Transform content;
    [SerializeField] private Button resetButton;

    private readonly List<UIKeyBindingRow> rows = new List<UIKeyBindingRow>();
    private UIKeyBindingRow capturingRow;
    private bool built;

    #region Unity
    private void Awake()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(() =>
                DataManager.Instance.PlayerDataManager.ResetKeyBindings());
    }

    private void OnEnable()
    {
        Build();
        RefreshAll();

        DataManager.Instance.PlayerDataManager.OnKeyBindingsChanged += RefreshAll;
    }

    private void OnDisable()
    {
        if (DataManager.Instance != null && DataManager.Instance.PlayerDataManager != null)
            DataManager.Instance.PlayerDataManager.OnKeyBindingsChanged -= RefreshAll;
    }
    #endregion

    // 최초 1회성 실행
    private void Build()
    {
        if (built)
            return;

        KeyCodeDataManager table = OriginDataManager.Instance.KeyCodes;

        if (table == null)
        {
            Debug.LogError("[UIKeyBindingPanel] : OriginData에 KeyCodeDataManager 없음");
            return;
        }

        var list = new List<KeyCodeData>(table.Table.Values);
        list.Sort((a, b) => a.ID.CompareTo(b.ID));

        foreach (KeyCodeData data in list)
        {
            UIKeyBindingRow row = Instantiate(rowPrefab, content);
            row.Setup(this, data, table.IsRebindable(data.Name));
            rows.Add(row);
        }

        built = true;
    }

    private void RefreshAll()
    {
        foreach (var row in rows)
            row.Refresh();
    }

    public bool TryBeginCapture(UIKeyBindingRow row)
    {
        if (capturingRow != null && capturingRow != row)
            return false;

        capturingRow = row;

        return true;
    }

    public void EndCapture(UIKeyBindingRow row)
    {
        if (capturingRow == row)
            capturingRow = null;
    }
}
