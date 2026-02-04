using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    [Header("Script References")]
    public CurrencyPanelID currencyPanelID;

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

    void Start()
    {
        InvokeRepeating(nameof(UpdateMarketPulse), 0.5f, 0.5f);
        UpdateStatusDisplay("Press 'Start Day' to begin trading.");
        marketStatusText.text = "Market Status: CLOSED";
    }

    void Update()
    {
        // This allows the percentages to update even when the market is closed
        if (buyPercentageText != null && buySlider != null)
            buyPercentageText.text = (buySlider.value / buySlider.maxValue * 100f).ToString("F0") + "%";

        if (sellPercentageText != null && sellSlider != null)
            sellPercentageText.text = (sellSlider.value / sellSlider.maxValue * 100f).ToString("F0") + "%";


        // If the market isn't open, we stop here.
        if (!marketOpen) return;

        // Track the time
        currentTimeInDay += Time.deltaTime;

        UpdateClockMath();
        UpdateNetWorthDisplay();

        //  Daily News Trigger
        if (!eventFiredToday && currentTimeInDay >= scheduledEventTime)
        {
            TriggerEconomyEvent();
            eventFiredToday = true;
        }

        // 5. End of Day Check
        if (currentTimeInDay >= dayDuration)
        {
            EndTradingDay();
        }
    }

    void StartNewDay()
    {
        currentTimeInDay = 0f;
        eventFiredToday = false;
        marketOpen = true;
        marketStatusText.text = "Market Status: OPEN";
        // DECAY SYSTEM: Reduce the trend by 50% overnight
        marketTrend *= 0.5;
        if (newsText != null)
        {
            // If there's still a significant trend carrying over
            if (marketTrend >= 0.001) newsText.text = "Market Showing USD momentum from yesterday.";
            else if (marketTrend <= -0.001) newsText.text = "Market Showing JPY momentum from yesterday.";
            else newsText.text = "Market currently showing no significant trend.";
        }
        scheduledEventTime = UnityEngine.Random.Range(5f, dayDuration - 5f);

        if (startDayButton != null) startDayButton.SetActive(false);
        Debug.Log($"Today's news will trigger at: {scheduledEventTime:F1}s");
    }

    void EndTradingDay()
    {
        currentDay++;
        SetClockText(5, 0, "PM");
        marketOpen = false; // Stop the market

        // Calculate Interest and log result
        CalculateDailyInterest();

        if (startDayButton != null) startDayButton.SetActive(true);
    }

    public void OnStartNextDayClicked()
    {
        StartNewDay();
    }

    void CalculateDailyInterest()
    {
        // Calculate interest
        double interestRate = 0.001; // 0.1%
        double interestEarned = usdBalance * interestRate;

        usdBalance += interestEarned;
        UpdateBalanceDisplays();
        marketStatusText.text = $"Market Closed. You earned ${interestEarned:F2} in bank interest.";
    }

    void UpdateMarketPulse()
    {
        // Don't wiggle the numbers if the market is closed!
        if (!marketOpen) return;

        double noise = UnityEngine.Random.Range(-0.0005f, 0.0005f);
        currencyPanelID.currentExchangeRate += marketTrend + noise;
        UpdateExchangeRate(currencyPanelID.currentExchangeRate);
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
        int eventRoll = UnityEngine.Random.Range(1, 21);

        if (eventRoll == 1)
        {
            marketTrend = (UnityEngine.Random.value > 0.5f) ? 0.01 : -0.01;
            newsText.text = (marketTrend > 0) ? "FLASH: USD Skyrocketing!" : $"FLASH: {currencyPanelID.currencyName} Surge Incoming!";
        }
        else if (eventRoll >=2 && eventRoll <=7)
        {
            marketTrend = (UnityEngine.Random.value > 0.5f) ? 0.004 : -0.004;
            newsText.text = (marketTrend > 0) ? "Strong USD sentiment today." : $"Strong {currencyPanelID.currencyName} exports reported.";
        }
        else if (eventRoll >= 8 && eventRoll <= 13)
        {
            marketTrend = (UnityEngine.Random.value > 0.5f) ? 0.002 : -0.002;
            newsText.text = (marketTrend > 0) ? "US markets showing modest gains." : $"{currencyPanelID.countryName} tourism boosting the {currencyPanelID.currencyName}.";
        }
        else
        {
            if (marketTrend == 0.0) 
            {
                newsText.text = "Global markets remain steady. No major news today.";
            }
            else
            {
                marketTrend = 0.0; 
                newsText.text = "Global markets steady out. No major news today.";
            }
        }

        Debug.Log($"EVENT TRIGGERED: {newsText.text} (Trend: {marketTrend} & {eventRoll})");
    }

    void UpdateNetWorthDisplay()
    {
        if (totalNetWorthText == null || currencyPanelID == null) return;

        // Start with base USD cash
        double totalInUsd = usdBalance;

        // Convert active currency to USD and add to total
        double assestsToUSD = currencyPanelID.currencyBalance / currencyPanelID.currentExchangeRate;
        totalInUsd += assestsToUSD;

        totalNetWorthText.text = $"Total Net Worth: ${totalInUsd:F3} (USD)";
    }

    void UpdateBalanceDisplays()
    {
        usdText.text = $"USD: ${usdBalance:F2}";
         
        currencyText.text = $"{currencyPanelID.currencyName}: {currencyPanelID.currencySymbol}{currencyPanelID.currencyBalance:F0}";
    }

    public void UpdateExchangeRate(double exchangeRate)
    {
        // Calculate % change
        double ratePercentage = (exchangeRate - currencyPanelID.openingRate) / currencyPanelID.openingRate * 100;

        // Format the string
        string sign = (ratePercentage >= 0) ? "+" : ""; 
        string colorHex = (ratePercentage >= 0) ? "green" : "red";
        rateText.text = $"Rate: 1 USD = {exchangeRate:F4} {currencyPanelID.currencyName} <color={colorHex}>({sign}{ratePercentage:F3}%)</color>";
    }

    void SetClockText(int hour, int min, string period)
    {
        if (clockText != null)
        {
            clockText.text = $"{hour:D2}:{min:D2} {period}";
        }
    }

    void UpdateStatusDisplay(string message)
    {
        if (newsText != null) newsText.text = message;
    }

    public void RefreshUI()
    {
        UpdateBalanceDisplays();
        UpdateNetWorthDisplay();
        UpdateClockMath();
        dayText.text = $"Day: {currentDay}";
    }

    // --- Button Functions ---

    public void BuyDynamicCurrency()
    {
        // Guard: If no panel is active, we can't buy anything
        if (currencyPanelID == null) return;
        // Instead of multiplying by the raw value, 
        // we multiply by the fraction
        double fraction = (double)buySlider.value / (double)buySlider.maxValue;
        double usdToSpend = usdBalance * fraction;

        if (usdToSpend > 0.01)
        {
            double currencyGained = usdToSpend * currencyPanelID.currentExchangeRate;
            usdBalance -= usdToSpend;
            currencyPanelID.currencyBalance += currencyGained;
            UpdateBalanceDisplays();
            Debug.Log($"Buy: Spent {fraction*100:F0}% (${usdToSpend:F2}) for ¥{currencyGained:F0}");
        } else {
            Debug.Log("Not enough USD to buy JPY.");
        }
    }

    public void SellDynamicCurrency()
    {
        // Guard: If no panel is active, we can't buy anything
        if (currencyPanelID == null) return;
        // Instead of multiplying by the raw value, 
        // we multiply by the fraction
        double fraction = (double)sellSlider.value / (double)sellSlider.maxValue;
        double currencyToSpend = currencyPanelID.currencyBalance * fraction;
        if (currencyToSpend > 0.01)
        {
            double usdGained = currencyToSpend / currencyPanelID.currentExchangeRate;
            usdBalance += usdGained;
            currencyPanelID.currencyBalance -= currencyToSpend;
            UpdateBalanceDisplays();
            Debug.Log($"Sell: Spent {fraction*100:F0}% (¥{currencyToSpend:F0}) for ${usdGained:F2}");
        } else {
            Debug.Log("Not enough JPY to sell for USD.");
        }
    }  
}