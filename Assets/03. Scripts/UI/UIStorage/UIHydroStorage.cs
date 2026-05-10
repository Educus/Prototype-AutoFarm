using UnityEngine;

public class UIHydroStorage : MonoBehaviour
{
    // π∞≈ ≈©
    [SerializeField] GameObject[] chargeObj;
    [SerializeField] GameObject[] noChargeObj;

    Sprite item;

    private void Start()
    {
        item = DataManager.Instance.GetItemImage(7011);
    }

    public void ChargeButton()
    {

    }

    public void AutoChargeutton()
    {

    }

    private void ChargeObj()
    {

    }
}
