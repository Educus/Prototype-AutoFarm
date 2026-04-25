using System.Collections.Generic;
using UnityEngine;




public class PropertyManager : MonoBehaviour
{
    // 플레이어의 재산, 재물을 관리 및 저장
    [SerializeField] private DataManager dataManager;

    private int haveRobot;
    private int haveObject;
    private int haveMoney;

    // 값 가져오기
    public int getRobot() { return haveRobot; }
    public int getObject() { return haveObject; }
    public int getMoney() { return haveMoney; }

    // 값 설정하기
    public void addRobot(int value) { haveRobot += value; }
    public void addObject(int value) { haveObject += value; }
    public void addMoney(int value) { haveMoney += value; }

}