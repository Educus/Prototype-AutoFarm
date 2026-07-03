using UnityEngine;

public class Setting : MonoBehaviour
{
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    public void OnClickSetting()
    {
        Debug.Log("설정 버튼 클릭");
        gameObject.SetActive(true);
    }

    public void OnClickClose()
    {
        Debug.Log("설정창 닫기 버튼 클릭");
        gameObject.SetActive(false);
    }
}
