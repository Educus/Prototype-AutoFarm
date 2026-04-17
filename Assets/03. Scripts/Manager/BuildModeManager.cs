using UnityEngine;

public class BuildModeManager : MonoBehaviour
{
    // 건설모드 On/Off 관리 버튼

    [SerializeField] private GameManager gameManager;
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private BuildUIManager buildModeUI;

    private void Start()
    {
        buildModeUI.BuildMode(false);
    }

    public void ToggleBuildMode()
    {
        gameManager.isBuildMode = !gameManager.isBuildMode;

        buildModeUI.BuildMode(gameManager.isBuildMode);

        if (!gameManager.isBuildMode)
        {
            buildingManager.CancelPlacement();
        }
    }
}
