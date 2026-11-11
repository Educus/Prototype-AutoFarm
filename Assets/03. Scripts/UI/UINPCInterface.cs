using TMPro;
using UnityEngine;

public class UINPCInterface : MonoBehaviour
{
    // UI 스태프, 앵커 관리
    [SerializeField] GameObject panel;
    [SerializeField] GameObject npcBackGround;
    [SerializeField] GameObject staff;
    [SerializeField] GameObject anchor;
    [SerializeField] GameObject player;
    [SerializeField] GameObject receipt;
    [SerializeField] GameObject textObj;
    [SerializeField] TMP_Text text;

    private void Awake()
    {
        OffUI();
    }

    public void ShowStaff()
    {
        OffUI();

        panel.SetActive(true);
        npcBackGround.SetActive(true);
        staff.SetActive(true);
    }

    public void ShowAnchor()
    {
        OffUI();

        panel.SetActive(true);
        npcBackGround.SetActive(true);
        anchor.SetActive(true);
    }

    public void ShowPlayer()
    {
        OffUI();

        panel.SetActive(true);
        npcBackGround.SetActive(true);
        player.SetActive(true);
    }

    public void ShowReceipt()
    {
        OffUI();

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
        npcBackGround.SetActive(false);
        staff.SetActive(false);
        anchor.SetActive(false);
        player.SetActive(false);
        receipt.SetActive(false);
        text.gameObject.SetActive(false);
    }
}
