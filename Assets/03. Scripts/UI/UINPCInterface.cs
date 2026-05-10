using TMPro;
using UnityEngine;

public class UINPCInterface : MonoBehaviour
{
    // UI 스태프, 앵커 관리
    [SerializeField] GameObject staff;
    [SerializeField] GameObject anchor;
    [SerializeField] GameObject textObj;
    [SerializeField] TMP_Text text;

    public void ShowStaff()
    {
        staff.SetActive(true);
        anchor.SetActive(false);
    }

    public void ShowAnchor()
    {
        staff.SetActive(false);
        anchor.SetActive(true);
    }

    public void ShowText(string text)
    {
        this.text.gameObject.SetActive(true);
        this.text.text = text;
    }

    public void OffUI()
    {
        staff.SetActive(false);
        anchor.SetActive(false);
        text.gameObject.SetActive(false);
    }
}
