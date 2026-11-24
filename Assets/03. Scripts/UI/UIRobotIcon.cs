using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRobotIcon : MonoBehaviour
{
    [SerializeField] TMP_Text robotName;
    [SerializeField] GameObject[] robotWorkIcon;
    [SerializeField] Image workItem;

    private OriginDataManager originDataManager => OriginDataManager.Instance;
    private DataManager dataManager => DataManager.Instance;

    UIRobot uiRobot;
    NPCJobConfig config;
    string id;

    private void Update()
    {
        SetInfo();
    }

    public void GetInfo(UIRobot ui, string id)
    {
        uiRobot = ui;
        this.id = id;

        config = dataManager.NPCManager.npcs[id].job;
        robotName.text = dataManager.NPCManager.npcs[id].entityName;
    }

    public void SetInfo()
    {
        if (config == null || !dataManager.NPCManager.npcs.ContainsKey(id))
            return;

        if (config.productItemID <= 0)
        {
            robotWorkIcon[0].SetActive(true);
            robotWorkIcon[1].SetActive(false);
        }
        else
        {
            robotWorkIcon[0].SetActive(false);
            robotWorkIcon[1].SetActive(true);
            workItem.sprite = originDataManager.GetItemImage(config.productItemID);
        }

        robotName.text = dataManager.NPCManager.npcs[id].entityName;
    }
}
