using UnityEngine;
using TMPro; 

public class SupplyManager : MonoBehaviour
{
    public TextMeshProUGUI supplyText; 

    private int currentSupplies = 0;
    private int targetSupplies = 5; 

    void Start()
    {
        UpdateSupplyUI();
    }

    public void GetSupply()
    {
        currentSupplies++;
        UpdateSupplyUI();

        if (currentSupplies >= targetSupplies)
        {
            Debug.Log("탈출 가능!");
        }
    }

    void UpdateSupplyUI()
    {
        supplyText.text = $"물자: {currentSupplies} / {targetSupplies}";
    }
}