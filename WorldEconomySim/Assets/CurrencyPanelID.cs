using UnityEngine;

public class CurrencyPanelID : MonoBehaviour
{
    [Header("Currency Info")]
    public string currencyName = "";
    public string countryName = "";
    public string currencySymbol = "";

    [Header("Economic Values")]
    public double openingRate;
    public double currentExchangeRate;
    public double currencyBalance;
    
    void Start()
{
    // Find the Manager
    CurrencyManager manager = FindAnyObjectByType<CurrencyManager>();
    
    // "Hello! I am a currency. Please add me to the list."
    if (manager != null && !manager.allCurrencies.Contains(this))
    {
        manager.allCurrencies.Add(this);
    }
}
}
