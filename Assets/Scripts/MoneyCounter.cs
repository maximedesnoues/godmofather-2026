using UnityEngine;

public class MoneyCounter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void UpdateText(int amount)
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = GameManager.Instance.Data.currentMoney + "/" + GameManager.Instance.Data.moneyQuota;
    }
}
