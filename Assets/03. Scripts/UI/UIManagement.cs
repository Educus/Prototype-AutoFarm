using UnityEngine;

public class UIManagement : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private UIChart uiChart;
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            uiChart.DrawChart(dataManager.productsData[1022].itemID);
        }
    }
}
