using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    [Header("Script References")]
    public CurrencyPanelID currencyPanelID;
    public BankPanelID activeBankID;

    [Header("Currency Values")]
    public double usdBalance = 100.0;

    [Header("Market Dynamics")]
    public double marketTrend = 0.0;

    [Header("Day Cycle Settings")]
    public float dayDuration = 180f;
    private float currentTimeInDay = 0f;
    private float scheduledEventTime;
    private int currentDay = 1;
    private bool eventFiredToday = false;
    public bool marketOpen = false;

    [Header("Dynamic Trading")]
    public Slider buySlider;
    public Slider sellSlider;
    public GameObject startDayButton;
    public TMP_InputField buyAmountInput;
    public TextMeshProUGUI buyPreviewText;
    public TMP_InputField sellAmountInput;
    public TextMeshProUGUI sellPreviewText;
    public TextMeshProUGUI sellButtonText;
    public TextMeshProUGUI buyButtonText;

    [Header("Bank UI Elements")]
    public TextMeshProUGUI bankSavingsText;
    public TextMeshProUGUI bankWalletText;
    public TMP_InputField bankTransferInput; // The box where you type "$500"
    public TextMeshProUGUI bankInterestRateText;

    [Header("Global Economy")]
    public List<CurrencyPanelID> allCurrencies = new();

    public TextUI textUI;

    void Start()
    {
        InvokeRepeating(nameof(UpdateMarketPulse), 0.5f, 0.5f);
        UpdateStatusDisplay("Press 'Start Day' to begin trading.");
    }

    void Update()
    {
        // This allows the percentages to update even when the market is closed
        if (textUI.buyPercentageText != null && buySlider != null)
            {
                textUI.buyPercentageText.text = (buySlider.value / buySlider.maxValue * 100f).ToString("F0") + "%";
            }

        if (textUI.sellPercentageText != null && sellSlider != null)
            {
                textUI.sellPercentageText.text = (sellSlider.value / sellSlider.maxValue * 100f).ToString("F0") + "%";
            }

        // If the market isn't open, we stop here.
        if (!marketOpen) return;

        // Track the time
        currentTimeInDay += Time.deltaTime;
        UpdateClockMath();
        // Update Net Worth Display
        UpdateNetWorthDisplay();

        //  Daily News Trigger
        if (!eventFiredToday && currentTimeInDay >= scheduledEventTime)
        {
            TriggerEconomyEvent();
            eventFiredToday = true;
        }

        // 5. End of Day Check
        if (currentTimeInDay >= dayDuration) { EndTradingDay(); }
    }

    // Core Game Loop Functions
    void StartNewDay()
    {
        currentTimeInDay = 0f;
        eventFiredToday = false;
        marketOpen = true;
        if (textUI.marketStatusText != null) { textUI.marketStatusText.text = "Market Status: OPEN"; }
        if (textUI.dayText != null) { textUI.dayText.text = $"Day: {currentDay}"; }
            // DECAY SYSTEM: Reduce the trend by 50% overnight
        marketTrend *= 0.5;
        if (textUI.newsText != null)
        {
            // If there's still a significant trend carrying over
            if (marketTrend >= 0.001) { textUI.newsText.text = "Market Showing USD momentum from yesterday."; }
            else if (marketTrend <= -0.001) { textUI.newsText.text = "Market Showing JPY momentum from yesterday."; }
            else { textUI.newsText.text = "Market currently showing no significant trend."; }
        }
        scheduledEventTime = UnityEngine.Random.Range(5f, dayDuration - 5f);

        if (startDayButton != null) { startDayButton.SetActive(false); }
        Debug.Log($"Today's news will trigger at: {scheduledEventTime:F1}s");
    }

    void EndTradingDay()
    {
        currentDay++;
        SetClockText(5, 0, "PM");
        marketOpen = false; // Stop the market

        // Calculate Interest and log result
        CalculateDailyInterest();
        if (startDayButton != null) { startDayButton.SetActive(true); }
    }

    public void OnStartNextDayClicked()
    {
        StartNewDay();
    }

    void UpdateMarketPulse()
    {
        // Don't wiggle the numbers if the market is closed!
        if (!marketOpen) return;

        if (currencyPanelID == null) return;

        double noise = UnityEngine.Random.Range(-0.0005f, 0.0005f);
        currencyPanelID.currentExchangeRate += marketTrend + noise;
        UpdateExchangeRate(currencyPanelID.currentExchangeRate);
    }

    public void UpdateExchangeRate(double exchangeRate)
    {
        if (currencyPanelID == null || textUI.rateText == null) return;

        if (Math.Abs(currencyPanelID.openingRate) < double.Epsilon)
        {
            textUI.rateText.text = $"Rate: 1 USD = {exchangeRate:F4} {currencyPanelID.currencyName} (N/A)";
            return;
        }

        // Calculate % change
        double ratePercentage = (exchangeRate - currencyPanelID.openingRate) / currencyPanelID.openingRate * 100;

        // Format the string
        string sign = (ratePercentage >= 0) ? "+" : "";
        string colorHex = (ratePercentage >= 0) ? "green" : "red";
        textUI.rateText.text = $"Rate: 1 USD = {exchangeRate:F4} {currencyPanelID.currencyName} <color={colorHex}>({sign}{ratePercentage:F3}%)</color>";
    }

    // Economy Event Functions
    void CalculateDailyInterest()
    {
        if (activeBankID != null)
        {
            // Calculate interest based on SAVINGS, not Wallet balance
            double interestEarned = activeBankID.savingsBalance * activeBankID.dailyInterestRate;
            // Add the money directly to the bank account, not the wallet balance
            activeBankID.savingsBalance += interestEarned;
            if (textUI.marketStatusText != null)
            {
                textUI.marketStatusText.text = $"Market Closed. Bank paid ${interestEarned:F2} in interest.";
            }
            Debug.Log($"Interest Paid: ${interestEarned:F2} added to {activeBankID.bankName}");
        }
        else
        {
            if (textUI.marketStatusText != null)
            {
                textUI.marketStatusText.text = "Market Closed. (No Bank Account Active)";
            }
        }
        UpdateBalanceDisplays();
    }

    void UpdateClockMath()
    {
        float dayPercentage = currentTimeInDay / dayDuration;
        float currentHourDecimal = 9.0f + (dayPercentage * 8.0f);

        int hours = Mathf.FloorToInt(currentHourDecimal);
        int minutes = Mathf.FloorToInt((currentHourDecimal - hours) * 60);

        string period = (hours >= 12) ? "PM" : "AM";
        int displayHour = (hours > 12) ? hours - 12 : hours;

        SetClockText(displayHour, minutes, period);
    }

    void TriggerEconomyEvent()
    {
        if (currencyPanelID == null)
{
            Debug.LogWarning("EVENT SKIPPED: currencyPanelID not assigned.");
            return;
        }

        int eventRoll = UnityEngine.Random.Range(1, 21);
        string newText;
        if (eventRoll == 1)
        {
            marketTrend = (UnityEngine.Random.value > 0.5f) ? 0.01 : -0.01;
            newText = (marketTrend > 0) ? "FLASH: USD Skyrocketing!" : $"FLASH: {currencyPanelID.currencyName} Surge Incoming!";
        }
        else if (eventRoll >=2 && eventRoll <=7)
        {
            marketTrend = (UnityEngine.Random.value > 0.5f) ? 0.004 : -0.004;
            newText = (marketTrend > 0) ? "Strong USD sentiment today." : $"Strong {currencyPanelID.currencyName} exports reported.";
        }
        else if (eventRoll >= 8 && eventRoll <= 13)
        {
            marketTrend = (UnityEngine.Random.value > 0.5f) ? 0.002 : -0.002;
            newText = (marketTrend > 0) ? "US markets showing modest gains." : $"{currencyPanelID.countryName} tourism boosting {currencyPanelID.currencyName}.";
        }
        else
        {
            if (marketTrend == 0.0) 
            {
                newText = "Global markets remain steady. No major news today.";
            }
            else
            {
                marketTrend = 0.0; 
                newText = "Global markets steady out. No major news today.";
            }
        }

        if (textUI.newsText != null) {textUI.newsText.text = newText;}
        Debug.Log($"EVENT TRIGGERED: {newText} (Trend: {marketTrend} & {eventRoll})");
    }

    // UI Update Functions
    void UpdateNetWorthDisplay()
    {
        if (textUI.totalNetWorthText == null) return;

        // Start with wallet + bank savings
        double totalInUsd = usdBalance;
        if (activeBankID != null) {totalInUsd += activeBankID.savingsBalance;}

        // loop through all currencies and convert to USD
        foreach (CurrencyPanelID currency in allCurrencies)
        {
            if (currency != null && Math.Abs(currency.currentExchangeRate) > double.Epsilon)
            {
                double currencyInUsd = currency.currencyBalance / currency.currentExchangeRate;
                totalInUsd += currencyInUsd;
            }
        }

        textUI.totalNetWorthText.text = $"Total Net Worth: ${totalInUsd:F2} (USD)";
    }

    void UpdateBalanceDisplays()
    {
        if (textUI.usdText != null) {textUI.usdText.text = $"USD: ${usdBalance:F2}";}
         
        if (textUI.currencyText != null && currencyPanelID != null)
        {
            textUI.currencyText.text = $"{currencyPanelID.currencyName}: {currencyPanelID.currencySymbol}{currencyPanelID.currencyBalance:F0}";
        }
    }

    void SetClockText(int hour, int min, string period)
    {
        if (textUI.clockText != null)
        {
            textUI.clockText.text = $"{hour:D2}:{min:D2} {period}";
        }
    }

    public void UpdateStatusDisplay(string message)
    {
        if (textUI.newsText != null) {textUI.newsText.text = message;}
        string status = marketOpen ? "OPEN" : "CLOSED";
        if (textUI.marketStatusText != null) {textUI.marketStatusText.text = $"Market Status: {status}";}
        if (textUI.dayText != null) {textUI.dayText.text = $"Day: {currentDay}";}
    }

    void UpdateBankUI()
    {
        // Safety Check: If we aren't looking at a bank, don't try to update it
        if (activeBankID == null) return;

        if (bankSavingsText != null) { bankSavingsText.text = $"Savings: ${activeBankID.savingsBalance:F2}"; }
    
        if (bankWalletText != null) { bankWalletText.text = $"Wallet: ${usdBalance:F2}"; }
    }

    public void RefreshUI()
    {
        // Global Updates (Always run)
        UpdateBalanceDisplays();
        UpdateNetWorthDisplay();
        UpdateClockMath();
        UpdateBankUI();

        // Reset Inputs (Clean slate for the new screen)
        if (buySlider != null) { buySlider.value = 0; OnBuySliderDrag(); }
        if (sellSlider != null) { sellSlider.value = 0; OnSellSliderDrag(); }

        // 3. Context-Specific Updates (Trading vs Empty)
        if (currencyPanelID != null)
        {
            UpdateExchangeRate(currencyPanelID.currentExchangeRate);
        
            // Update Title: "Euro: €500"
            if (textUI.currencyText != null)
            {
            textUI.currencyText.text = $"{currencyPanelID.currencyName}: {currencyPanelID.currencySymbol}{currencyPanelID.currencyBalance:F0}";
            }
            // Update Symbol: "€"
            if (textUI.sellSymbolLabel != null)
            {
            textUI.sellSymbolLabel.text = currencyPanelID.currencySymbol;
            }
            // Update Button Text
            if (buyButtonText != null) { buyButtonText.text = $"Buy {currencyPanelID.currencyName}"; }
            if (sellButtonText != null) { sellButtonText.text = $"Sell {currencyPanelID.currencyName}"; }
        }
        else
        {
            // We are on Map/Bank
            if (textUI.currencyText != null) { textUI.currencyText.text = "Market Overview"; } // Or "Select a Market"
            if (textUI.sellSymbolLabel != null) { textUI.sellSymbolLabel.text = ""; }
        }

        // 4. Bank Specifics
        if (activeBankID != null && bankInterestRateText != null)
        {
            bankInterestRateText.text = $"Daily Interest Rate: {activeBankID.dailyInterestRate}%";
        }
    }
    // Slider and Input Field Functions
    public void OnBuySliderDrag() 
    {
        // Guard slider
        if (buySlider == null || buySlider.maxValue <= 0) return;

        // 1. Calculate the math based on where the slider is NOW
        double fraction = (double)buySlider.value / (double)buySlider.maxValue;
        double usdCost = usdBalance * fraction;

        // 2. Calculate USD to Receive (Live Preview)
        if (currencyPanelID != null)
        {
            double expectedYield = usdCost * currencyPanelID.currentExchangeRate;
        
            // Update both displays
            if (buyAmountInput != null) { buyAmountInput.SetTextWithoutNotify(usdCost.ToString("F2")); }
            
            if (buyPreviewText != null) { buyPreviewText.text = $"You Get: {currencyPanelID.currencySymbol}{expectedYield:F2}"; }
        }
        else
        {
            if (buyAmountInput != null) { buyAmountInput.SetTextWithoutNotify(usdCost.ToString("F2")); }
            if (buyPreviewText != null) { buyPreviewText.text = $"You Get: -"; }
        }
    }

    public void OnBuyInputType(string text)
    {
        if (currencyPanelID == null) return;
        // 1. Try to understand what the user typed
        if (double.TryParse(text, out double amount))
        {
            // 2. Clamp it: They can't type $500 if they only have $100
            if (amount > usdBalance) { amount = usdBalance; }
            if (amount < 0) { amount = 0; }

            // 3. Reverse math: Calculate where the slider should be
            float fraction = 0f;
            if (usdBalance > 0)
            {
                fraction = (float)(amount / usdBalance);
            }
        
            // 4. Move the slider to match their typing
            if (buySlider != null) { buySlider.SetValueWithoutNotify(fraction * buySlider.maxValue); }
            // 5. Update the preview text
            if (currencyPanelID != null)
            {
                double expectedYield = amount * currencyPanelID.currentExchangeRate;
                if (buyPreviewText != null)
                {
                    buyPreviewText.text = $"You Get: {currencyPanelID.currencySymbol}{expectedYield:F2}";
                }
            }
            else
            {
                if (buyPreviewText != null) buyPreviewText.text = $"You Get: -";
            }
        }
    }

    public void SetMaxBuyAmount()
    {
        // Guard: Can't sell if we don't know what currency we are looking at
        if (currencyPanelID == null) return;

        // 1. Set the slider to its maximum value (100%)
        if (buySlider != null) { buySlider.value = buySlider.maxValue; }
    
        // 2. Force the update so the Text Box and Preview Label sync up
        OnBuySliderDrag();
    }

    public void OnSellSliderDrag() 
    {
        // Guard slider
        if (sellSlider == null || sellSlider.maxValue <= 0) return;

        // 1. Calculate fraction from slider
        double fraction = (double)sellSlider.value / (double)sellSlider.maxValue;

        if (currencyPanelID != null)
        {
            double amountToSell = currencyPanelID.currencyBalance * fraction;
            double expectedYield = 0.0;

            if (Math.Abs(currencyPanelID.currentExchangeRate) > double.Epsilon)
            {
                expectedYield = amountToSell / currencyPanelID.currentExchangeRate;
            }

            if (sellAmountInput != null) { sellAmountInput.SetTextWithoutNotify(amountToSell.ToString("F0")); }
            
            if (sellPreviewText != null) { sellPreviewText.text = $"You Get: ${expectedYield:F2}"; }
        }
        else
        {
            if (sellAmountInput != null) { sellAmountInput.SetTextWithoutNotify("0"); }
            if (sellPreviewText != null) { sellPreviewText.text = $"You Get: $0.00"; }
        }
    }

    public void OnSellInputType(string text)
    {
        // Guard: Can't sell if we don't know what currency we are looking at
        if (currencyPanelID == null) return;

        if (double.TryParse(text, out double amount))
        {
            // 1. Clamp to the actual foreign balance (e.g., You can't sell 10,000 Yen if you only have 5,000)
            if (amount > currencyPanelID.currencyBalance) { amount = currencyPanelID.currencyBalance; }
            if (amount < 0) { amount = 0; }

            // 2. Calculate the fraction for the slider
            // Safety check: Avoid dividing by zero if balance is empty
            float fraction = 0f;

            if (currencyPanelID.currencyBalance > 0)
            {
                fraction = (float)(amount / currencyPanelID.currencyBalance);
            }

            if (sellSlider != null) { sellSlider.SetValueWithoutNotify(fraction * sellSlider.maxValue); }

            double expectedYield = 0.0;

            if (Math.Abs(currencyPanelID.currentExchangeRate) > double.Epsilon)
            {
                expectedYield = amount / currencyPanelID.currentExchangeRate;
            }

            if (sellPreviewText != null) { sellPreviewText.text = $"You Get: ${expectedYield:F2}"; }
        }
    }

    public void SetMaxSellAmount()
    {
        // Guard: Can't sell if we don't know what currency we are looking at
        if (currencyPanelID == null) return;

        // 1. Set the slider to its maximum value (100%)
        if (sellSlider != null) { sellSlider.value = sellSlider.maxValue; }
    
        // 2. Force the update so the Text Box and Preview Label sync up
        OnSellSliderDrag();
    }

    // --- Button Functions ---

    public void BuyDynamicCurrency()
    {
        // Guard: If no panel is active, we can't buy anything
        if (currencyPanelID == null) return;
        if (buySlider == null || buySlider.maxValue <= 0) 
        {
            Debug.Log("Buy failed: Buy slider not configured.");
            return;
        }
        // Instead of multiplying by the raw value, 
        // we multiply by the fraction
        double fraction = (double)buySlider.value / (double)buySlider.maxValue;
        double usdToSpend = usdBalance * fraction;

        if (usdToSpend > 0.01 && Math.Abs(currencyPanelID.currentExchangeRate) > double.Epsilon)
        {
            double currencyGained = usdToSpend * currencyPanelID.currentExchangeRate;
            usdBalance -= usdToSpend;
            currencyPanelID.currencyBalance += currencyGained;
            UpdateBalanceDisplays();
            buySlider.value = 0; // Reset slider after purchase
            OnBuySliderDrag(); // Update preview
            Debug.Log($"Buy: Spent {fraction*100:F0}% (${usdToSpend:F2}) for {currencyPanelID.currencySymbol}{currencyGained:F0}");
        }
        else
        {
            Debug.Log("Not enough USD to buy currency or exchange rate unavailable.");
        }
    }

    public void SellDynamicCurrency()
    {
        // Guard: If no panel is active, we can't buy anything
        if (currencyPanelID == null) return;
        if (sellSlider == null || sellSlider.maxValue <= 0)
        {
            Debug.Log("Sell failed: Sell slider not configured.");
            return;
        }
        // Instead of multiplying by the raw value, 
        // we multiply by the fraction
        double fraction = (double)sellSlider.value / (double)sellSlider.maxValue;
        double currencyToSpend = currencyPanelID.currencyBalance * fraction;
        if (currencyToSpend > 0.01 && Math.Abs(currencyPanelID.currentExchangeRate) > double.Epsilon)
        {
            double usdGained = currencyToSpend / currencyPanelID.currentExchangeRate;
            usdBalance += usdGained;
            currencyPanelID.currencyBalance -= currencyToSpend;
            UpdateBalanceDisplays();
            sellSlider.value = 0; // Reset slider after sale
            OnSellSliderDrag(); // Update preview
            Debug.Log($"Sell: Spent {fraction*100:F0}% ({currencyPanelID.currencySymbol}{currencyToSpend:F0}) for ${usdGained:F2}");
        }
        else
        {
            Debug.Log("Not enough Currency to sell for USD or exchange rate unavailable.");
        }
    } 

    // Bank Transfer Function
    public void DepositToBank()
    {
        if (activeBankID == null || bankTransferInput == null) return;

        // Parse the input text
        if (double.TryParse(bankTransferInput.text, out double amount))
        {
            // Validation 1: Positive numbers only
            if (amount <= 0) return; 
        
            // Validation 2: Can't deposit money you don't have
            if (amount > usdBalance) { amount = usdBalance; }

            // The Transaction
            usdBalance -= amount;
            activeBankID.savingsBalance += amount;

            // Cleanup
            bankTransferInput.text = ""; // Clear the box
            UpdateBankUI();        // Refresh Bank Text
            UpdateBalanceDisplays(); // Refresh USD Text
            Debug.Log($"Deposited ${amount:F2}. New Savings: ${activeBankID.savingsBalance:F2}");
        }
    }

    public void WithdrawFromBank()
    {
        if (activeBankID == null || bankTransferInput == null) return;

        if (double.TryParse(bankTransferInput.text, out double amount))
        {
            // Validation 1: Positive numbers only
        if (amount <= 0) return;

            // Validation 2: Can't withdraw money the bank doesn't have
            if (amount > activeBankID.savingsBalance) { amount = activeBankID.savingsBalance; }

            // The Transaction
            activeBankID.savingsBalance -= amount;
            usdBalance += amount;

            // Cleanup
            bankTransferInput.text = ""; 
            UpdateBankUI();
            UpdateBalanceDisplays();
            Debug.Log($"Withdrew ${amount:F2}. New Wallet: ${usdBalance:F2}");
        }
    }

    [Serializable]
    public class TextUI
    {
        [Header("UI Text Elements")]
        public TextMeshProUGUI usdText;
        public TextMeshProUGUI currencyText;
        public TextMeshProUGUI rateText;
        public TextMeshProUGUI dayText;
        public TextMeshProUGUI newsText;
        public TextMeshProUGUI clockText;
        public TextMeshProUGUI buyPercentageText;
        public TextMeshProUGUI sellPercentageText;
        public TextMeshProUGUI totalNetWorthText;
        public TextMeshProUGUI marketStatusText;
        public TextMeshProUGUI sellSymbolLabel;
    }
}