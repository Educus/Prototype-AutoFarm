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
        // 이미 BuildMode면 종료
        if (gameManager.IsMode(GameMode.Build))
        {
            buildingManager.ExitBuildMode();

            buildModeUI.BuildMode(false);

            return;
        }

        // 다른 모드 중이면 무시
        if (gameManager.IsBusy())
        {
            Debug.Log("다른 모드 진행 중");
            return;
        }

        // BuildMode 진입
        if (gameManager.EnterMode(GameMode.Build))
        {
            buildModeUI.BuildMode(true);
        }
    }
}
