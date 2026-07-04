using UnityEngine;
using TMPro; 

public class SupplyManager : MonoBehaviour
{
    private static SupplyManager instance;

    public TextMeshProUGUI supplyText; 

    private int currentSupplies = 0;
    private int targetSupplies = 5; 

    public static SupplyManager Instance => instance;

    void Awake()
    {
        instance = this;
        ResolveSupplyText();
    }

    void Start()
    {
        ResolveSupplyText();
        UpdateSupplyUI();
    }

    public void GetSupply()
    {
        SetSupplyProgress(currentSupplies + 1, targetSupplies);
    }

    public void SetSupplyProgress(int currentCount, int targetCount)
    {
        currentSupplies = Mathf.Max(0, currentCount);
        targetSupplies = Mathf.Max(1, targetCount);
        UpdateSupplyUI();

        if (currentSupplies >= targetSupplies)
        {
            Debug.Log("탈출 가능!");
        }
    }

    void UpdateSupplyUI()
    {
        ResolveSupplyText();
        if (supplyText == null)
        {
            return;
        }

        supplyText.text = $"{currentSupplies}/{targetSupplies}";
    }

    private void ResolveSupplyText()
    {
        if (supplyText != null)
        {
            return;
        }

        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name == "supplyManagerText")
            {
                supplyText = text;
                return;
            }
        }
    }
}
