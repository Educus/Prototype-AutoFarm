using UnityEngine;

public class BuildingModUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [SerializeField] private GameObject invenMod;
    [SerializeField] private GameObject buildingMod;

    [SerializeField] private GameObject buildCancle;
    [SerializeField] private GameObject otherButton;

    private void Update()
    {
        invenMod.SetActive(!gameManager.isBuildMode);
        buildingMod.SetActive(gameManager.isBuildMode);

        buildCancle.SetActive(gameManager.isBuildMode);
        otherButton.SetActive(!gameManager.isBuildMode);
    }
}