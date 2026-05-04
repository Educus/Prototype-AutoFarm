using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRobot : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private UIManagement uiManagement;

    [SerializeField] private GameObject robotIconPrefab;
    [SerializeField] private GameObject robotIconContent;

    private Dictionary<string, GameObject> robots = new Dictionary<string, GameObject>();

    private void Start()
    {
        dataManager = DataManager.Instance;
    }
    void Update()
    {
        UIRobotUpdate();
    }
    private void UIRobotUpdate()
    {
        if (dataManager.NPCManager.npcs.Count == 0) return;
        if (robots.Count == dataManager.NPCManager.npcs.Count) return;

        foreach (var npcs in dataManager.NPCManager.npcs.Values)
        {
            if (!robots.ContainsKey(npcs.id))
            {
                GameObject robotIcon = Instantiate(robotIconPrefab, robotIconContent.transform);
                robots[npcs.id] = robotIcon;

                robotIcon.GetComponent<UIRobotIcon>().GetInfo(dataManager, this, npcs.id);
                robotIcon.GetComponent<Button>().onClick.AddListener(() => OnClickTargetRobot(npcs.id));
            }

        }
    }

    public void ViewRobot(int value)
    {
        foreach (var robot in robots)
        {
            switch (value)
            {
                case 0:
                    robot.Value.gameObject.SetActive(true);
                    break;

                case 1:
                case 2:
                    if (dataManager.NPCManager.npcs[robot.Key].job.productItemID == 0)
                        robot.Value.gameObject.SetActive(value == 2);
                    else robot.Value.gameObject.SetActive(value == 1);
                        break;

                default:
                    break;
            }
        }
    }

    // 로봇 클릭 시 이벤트
    public void OnClickTargetRobot(string value)
    {
        // 해당 위치로 이동 및 상점창 off
        GameObject target = dataManager.NPCManager.npcs[value].gameObject;
        GameManager.Instance.targetLock = target;

        uiManagement.ExitButton();
    }
}
