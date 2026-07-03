using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRobotIcon : MonoBehaviour
{
    [SerializeField] TMP_Text robotName;
    [SerializeField] GameObject[] robotWorkIcon;
    [SerializeField] Image workItem;

    private DataManager dataManager;

    UIRobot uiRobot;
    NPCJobConfig config;
    string id;

    private void Update()
    {
        SetInfo();
    }

    public void GetInfo(DataManager data, UIRobot ui, string id)
    {
        dataManager = data;
        uiRobot = ui;
        this.id = id;

        config = dataManager.NPCManager.npcs[id].job;
        robotName.text = dataManager.NPCManager.npcs[id].entityName;
    }

    public void SetInfo()
    {
        if (config.productItemID <= 0)
        {
            robotWorkIcon[0].SetActive(true);
            robotWorkIcon[1].SetActive(false);
        }
        else
        {
            robotWorkIcon[0].SetActive(false);
            robotWorkIcon[1].SetActive(true);
            workItem.sprite = dataManager.GetItemImage(config.productItemID);
        }

        robotName.text = dataManager.NPCManager.npcs[id].entityName;
    }
}
