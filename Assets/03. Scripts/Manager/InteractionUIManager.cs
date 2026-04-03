using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class InteractionUIManager : MonoBehaviour
{
    // 상호작용 UI를 관리
    // UI가 켜져있을 경우 빈 화면 클릭 시 켜져있는 UI끄기
    // Esc키 입력 시 켜져있는 UI 순서대로 끄기
    private List<GameObject> popUpUIList = new List<GameObject>();

    public void ShowUI(IInteractable interactable)
    {

    }

    public void AddUI(GameObject ActUI)
    {
        popUpUIList.Add(ActUI);
    }
    public void RemoveUI(GameObject ActUI)
    {
        popUpUIList.Remove(ActUI);
    }
}
