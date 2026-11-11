using TMPro;
using UnityEngine;

public class UINPCInterface : MonoBehaviour
{
    // UI 스태프, 앵커 관리
    [SerializeField] GameObject panel;
    [SerializeField] GameObject staff;
    [SerializeField] GameObject anchor;
    [SerializeField] GameObject receipt;
    [SerializeField] GameObject textObj;
    [SerializeField] TMP_Text text;

    private void Awake()
    {
        OffUI();
    }

    public void ShowStaff()
    {
        panel.SetActive(true);
        staff.SetActive(true);
    }

    public void ShowAnchor()
    {
        panel.SetActive(true);
        anchor.SetActive(true);
    }

    public void ShowReceipt()
    {
        panel.SetActive(true);
        receipt.SetActive(true);
    }

    public void ShowText(string text)
    {
        this.text.gameObject.SetActive(true);
        this.text.text = text;
    }

    public void OffUI()
    {
        panel.SetActive(false);
        staff.SetActive(false);
        anchor.SetActive(false);
        receipt.SetActive(false);
        text.gameObject.SetActive(false);
    }
}
