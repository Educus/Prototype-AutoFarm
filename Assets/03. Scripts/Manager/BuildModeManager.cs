using UnityEngine;

public class BuildModeManager : MonoBehaviour
{
    // 건설모드 On/Off 관리 버튼

    [SerializeField] private GameManager gameManager;
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private GameObject buildModeUI;

    public void ToggleBuildMode()
    {
        gameManager.isBuildMode = !gameManager.isBuildMode;

        buildModeUI.SetActive(gameManager.isBuildMode);

        if (!gameManager.isBuildMode)
        {
            buildingManager.CancelPlacement();
        }
    }
}
