using UnityEngine;
using TMPro;
public class CurrencyManager : MonoBehaviour
{
    // --- Our Game Variables ---
    [Header("Currency Values")]
    public double usdBalance = 100.0;
    public double jpyBalance = 0.0;

    [Header("Transaction Settings")]
    public double jpyExchangeRate = 150.0; // 1 USD = 150 JPY
    public double usdTransactionAmount = 10.0;

    [Header("UI Text Elements")]
    public TextMeshProUGUI usdText;
    public TextMeshProUGUI jpyText;
    public TextMeshProUGUI rateText;
    void Start()
    {
        // Initialize balances
        usdBalance = 100.0;
        jpyBalance = 0.0;
        Debug.Log("Currency Manager has started!");
        Debug.Log("Starting USD: " + usdBalance);
        // We'll set the text to display the current balance
        UpdateTextDisplays();
    }
    void Update()
    {
        // We'll leave this empty for now.
        // We will create new functions for our buttons.
    }

        // This function's job is to update all the text on the screen
    void UpdateTextDisplays()
    {
        usdText.text = "USD Balance: " + usdBalance.ToString("F2");
        jpyText.text = "JPY Balance: " + jpyBalance.ToString("F0");
        rateText.text = "Exchange Rate: 1 USD = " + jpyExchangeRate.ToString("F0") + " JPY";
    }
        // Button Functions
    public void BuyJPY()
    {
        // 1. Check if we have enough USD
        if (usdBalance >= usdTransactionAmount)
        {
            // 2. Do the math
            usdBalance -= usdTransactionAmount;
            jpyBalance += usdTransactionAmount * jpyExchangeRate;
            // 3. Log to the console to make sure it worked
            Debug.Log("BOUGHT!");
            // We'll set the text to display the current balance
            UpdateTextDisplays();
        }
        else
        {
            Debug.Log("Not enough USD!");
        }
    }
    public void SellJPY()
    {
        double jpyAmountToSell = usdTransactionAmount * jpyExchangeRate;
        // 1. Check if we have enough JPY
        if (jpyBalance >= jpyAmountToSell)
        {
            // 2. Do the math (the reverse of buying)
            jpyBalance -= jpyAmountToSell;
            usdBalance += usdTransactionAmount;
            // 3. Log to the console to make sure it worked
            Debug.Log("SOLD!");
            // 4. Update the text on screen
            UpdateTextDisplays();
        }
        else
        {
            Debug.Log("Not enough JPY to sell!");
        }
    }
}