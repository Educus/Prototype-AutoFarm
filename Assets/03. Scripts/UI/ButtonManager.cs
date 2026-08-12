using Newtonsoft.Json.Bson;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{

    private List<Button> Buttons;

    private void Start()
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
            //오브젝트 명에 해당 단어가 포함되어 있는지 확인 후 이벤트 등록
            // ToLowerInvariant()를 사용하여 대소문자 구분 없이 비교
            string buttonName = button.gameObject.name.ToLowerInvariant();

            if (buttonName.Contains("gui"))
            {
                GUIButtonInit(buttonName, button);
            }

            else if (buttonName.Contains("uimanagement"))
            {
                UIDataChipInit(buttonName, button);
            }

            else if (buttonName.Contains("exit"))
            {
                button.onClick.AddListener(() =>
                {
                    UIManager.Instance.PlaySFX("SFX_GUI_Button");
                    UIManager.Instance.ExitPopUpPage();
                }
                );
            }

            else
            {
                Debug.Log("버튼 이벤트 등록 실패 : " + buttonName);
            }
        }
    }

    private void GUIButtonInit(string buttonName, Button button)
    {

        //모드 진입 버튼
        if (buttonName.Contains("management"))
        {
            button.onClick.AddListener(() =>
            {
                UIManager.Instance.PlaySFX("SFX_GUI_Button");
                UIManager.Instance.OpenPopUpPage();
            }
            );
        }

        else if (buttonName.Contains("buildingmod"))
        {
            button.onClick.AddListener(() =>
            {
                UIManager.Instance.PlaySFX("SFX_GUI_Button");
                UIManager.Instance.ToggleBuildMod();
            }
            );
        }

        //건축 키트 선택 버튼
        // string으로 버튼 이름 입력 받아서 이벤트 할당
        // 예: "field", "storage", "barn" 등
        //LowerInvariant()를 사용하여 대소문자 구분 없이 비교
        //버튼 오브젝트에는 "field", "storage", "barn" 등의 아이템 이름이 포함되어 있어야 함
        else if (buttonName.Contains("field"))
        {
            button.onClick.AddListener(() =>
            {
                int itemID = DataManager.Instance.GetItemIDNumber("field");
                UIManager.Instance.PlaySFX("SFX_GUI_Button");
                UIManager.Instance.SelectBuildingKit(itemID);
            }
            );
        }

        else if (buttonName.Contains("storage"))
        {
            button.onClick.AddListener(() =>
            {
                int itemID = DataManager.Instance.GetItemIDNumber("storage");
                UIManager.Instance.PlaySFX("SFX_GUI_Button");
                UIManager.Instance.SelectBuildingKit(itemID);
            }
            );
        }

        else if (buttonName.Contains("barn"))
        {
            button.onClick.AddListener(() =>
            {
                int itemID = DataManager.Instance.GetItemIDNumber("barn");
                UIManager.Instance.PlaySFX("SFX_GUI_Button");
                UIManager.Instance.SelectBuildingKit(itemID);
            }
            );
        }
    }

    private void UIDataChipInit(string buttonName, Button button)
    {
        if(buttonName.Contains("chart"))
        {
            button.onClick.AddListener(() =>
            {
                UIManager.Instance.PlaySFX("SFX_UI_PopUp_DataChip");

            }
            );
        }
    }

}
