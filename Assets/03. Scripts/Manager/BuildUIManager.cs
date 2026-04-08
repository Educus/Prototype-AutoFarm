using UnityEngine;

public class BuildUIManager : MonoBehaviour
{
    // 건설 모드 중 건물 선택 시 UI 관리(버튼 관리)
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private ItemInfoUI itemInfoUI;

    public void OnClickBuildingButton(int itemID)
    {
        // 같은 버튼 클릭 시 취소
        if (buildingManager.CurrentItemID == itemID)
        {
            buildingManager.CancelPlacement();
            itemInfoUI.Hide();
            return;
        }

        buildingManager.StartPlacement(itemID);

        var building = buildingManager.GetBuilding(itemID);

        itemInfoUI.Show(itemID, building.data, building.icon);
    }
}
