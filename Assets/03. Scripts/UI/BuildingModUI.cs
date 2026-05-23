using UnityEngine;

public class BuildingModUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [SerializeField] private GameObject invenMode;
    [SerializeField] private GameObject buildingMode;

    [SerializeField] private GameObject buildCancel;
    [SerializeField] private GameObject otherButton;

    private void Start()
    {
        RefreshUI(gameManager.CurrentMode);

        gameManager.onModeChanged += RefreshUI;
    }

    private void OnDestroy()
    {
        gameManager.onModeChanged -= RefreshUI;
    }

    public void RefreshUI(GameMode mode)
    {
        bool isBuildMode =
            mode == GameMode.Build;

        invenMode.SetActive(!isBuildMode);
        buildingMode.SetActive(isBuildMode);

        buildCancel.SetActive(isBuildMode);
        otherButton.SetActive(!isBuildMode);
    }
}