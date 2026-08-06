using Newtonsoft.Json.Bson;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{

    private List<Button> Buttons;

    private void Awake()
    {
        init();
    }

    private void init()
    {
        //버튼 수집
        Button[] buttons = UIManager.Instance.CollectButtons();
        Buttons = new List<Button>(buttons);

        ButtonEventInitialize();
    }

    //버튼 이벤트 등록
    private void ButtonEventInitialize()
    {
        foreach (var button in Buttons)
        {
            if (button.gameObject.name == "PopUp")
            {
                button.onClick.AddListener(() =>
                {
                    UIManager.Instance.PlaySFX("SFX_GUI_Button");
                    UIManager.Instance.OpenPopUpPage();
                }
                );
            }

            else if (button.gameObject.name == "BuildingMod")
            {
                button.onClick.AddListener(() =>
                {
                    UIManager.Instance.PlaySFX("SFX_GUI_Button");
                    UIManager.Instance.ToggleBuildMode();
                }
                );
            }

            // datamanager에서 할당 받아서 사용하도록 수정 필요
            else if(button.gameObject.name == "BuildSlot1")
            {
                button.onClick.AddListener(() =>
                {
                    UIManager.Instance.PlaySFX("SFX_GUI_Button");
                    UIManager.Instance.SelectBuildingKit(9001);
                }
                );
            }

            else if (button.gameObject.name == "BuildSlot2")
            {
                button.onClick.AddListener(() =>
                {
                    UIManager.Instance.PlaySFX("SFX_GUI_Button");
                    UIManager.Instance.SelectBuildingKit(9011);
                }
                );
            }

            else if (button.gameObject.name == "BuildSlot3")
            {
                button.onClick.AddListener(() =>
                {
                    UIManager.Instance.PlaySFX("SFX_GUI_Button");
                    UIManager.Instance.SelectBuildingKit(9021);
                }
                );
            }

            else if (button.gameObject.name == "Exit")
            {
                button.onClick.AddListener(() =>
                {
                    UIManager.Instance.PlaySFX("SFX_GUI_Button");
                    UIManager.Instance.ExitPopUpPage();
                }
                );
            }
        }
    }
}
