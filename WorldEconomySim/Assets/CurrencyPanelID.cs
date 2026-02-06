using UnityEngine;

public class CurrencyPanelID : MonoBehaviour
{
    public string currencyName = "";
    public double openingRate;
    public double currentExchangeRate;
    public double currencyBalance;
    public string countryName = "";
    public string currencySymbol = "";

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
