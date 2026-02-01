using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    [Header("Screen Navigation")]
    public GameObject mapHubPanel;
    public GameObject japanTradingPanel;
    public GameObject portfolioPanel;

    [Header("Currency Values")]
    public double usdBalance = 100.0;
    public double jpyBalance = 0.0;

    [Header("Market Dynamics")]
    public double jpyExchangeRate = 150.0;
    public double marketTrend = 0.0;

    [Header("Day Cycle Settings")]
    public float dayDuration = 180f;
    private float currentTimeInDay = 0f;
    private float scheduledEventTime;
    private int currentDay = 0;
    private bool eventFiredToday = false;
    private bool marketOpen = false;

    [Header("Dynamic Trading")]
    public Slider buySlider;
    public Slider sellSlider;
    public GameObject startDayButton;

    [Header("UI Text Elements")]
    public TextMeshProUGUI usdText;
    public TextMeshProUGUI jpyText;
    public TextMeshProUGUI rateText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI newsText;
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI buyPercentageText;
    public TextMeshProUGUI sellPercentageText;
    public TextMeshProUGUI totalNetWorthText;

    void Start()
    {
        ShowMapHub();
        InvokeRepeating(nameof(UpdateMarketPulse), 0.5f, 0.5f);
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
        // DECAY SYSTEM: Reduce the trend by 50% overnight
        marketTrend *= 0.5;
        if (newsText != null)
        {
            // If there's still a significant trend carrying over
            if (marketTrend >= 0.005) newsText.text = "USD showing overnight strength.";
            else if (marketTrend <= -0.005) newsText.text = "JPY carrying momentum from yesterday.";
            else newsText.text = "Markets open with no significant news.";
        }
        scheduledEventTime = UnityEngine.Random.Range(5f, dayDuration - 5f);
        currentDay++;
        dayText.text = $"Day: {currentDay}";

        if (startDayButton != null) startDayButton.SetActive(false);
        Debug.Log($"Today's news will trigger at: {scheduledEventTime:F1}s");
    }

    void EndTradingDay()
    {
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
        UpdateStatusDisplay($"Market Closed. You earned ${interestEarned:F2} in bank interest.");
    }

    void UpdateMarketPulse()
    {
        // Don't wiggle the numbers if the market is closed!
        if (!marketOpen) return;

        double noise = UnityEngine.Random.Range(-0.01f, 0.01f);
        jpyExchangeRate += marketTrend + noise;
        if (jpyExchangeRate < 75.0) jpyExchangeRate = 75.0;
        UpdateExchangeRate();
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
        int eventRoll = UnityEngine.Random.Range(1, 7);

        if (eventRoll == 1)
        {
            marketTrend = 0.025;
            newsText.text = "US Federal Reserve hints at rate hikes. USD climbing.";
        }
        else if (eventRoll == 2)
        {
            marketTrend = 0.01;
            newsText.text = "Consumer spending rises in the US. USD seeing steady gains.";
        }
        else if (eventRoll == 3)
        {
            marketTrend = -0.025;
            newsText.text = "Japan's tech exports hit record highs. JPY strengthening.";
        }
        else if (eventRoll == 4)
        {
            marketTrend = -0.01;
            newsText.text = "Tourism surge boosts local Japanese economy. JPY up slightly.";
        }
        else
        {
            marketTrend = 0.0;
            newsText.text = "Global markets remain steady. No major news today.";
        }

        Debug.Log($"EVENT TRIGGERED: {newsText.text} (Trend: {marketTrend})");
    }

    void UpdateNetWorthDisplay()
    {
        if (totalNetWorthText == null) return;

        // Start with base USD cash
        double totalInUsd = usdBalance;

        // Convert JPY holding to USD and add to total
        double jpyValueInUsd = jpyBalance / jpyExchangeRate;
        totalInUsd += jpyValueInUsd;

        totalNetWorthText.text = $"Total Net Worth: ${totalInUsd:F2} (USD)";
    }

    void UpdateBalanceDisplays()
    {
        usdText.text = $"USD: ${usdBalance:F2}";
        jpyText.text = $"JPY: ¥{jpyBalance:F0}";
    }

    void UpdateExchangeRate()
    {
        rateText.text = $"Rate: 1 USD = {jpyExchangeRate:F2} JPY";
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

    // --- Menu Functions ---
    public void ShowJapanTrading()
    {
        mapHubPanel.SetActive(false);
        japanTradingPanel.SetActive(true);

        // Only show the button if the market is NOT open
        if (startDayButton != null)
        {
            startDayButton.SetActive(!marketOpen);
        }

        // Only reset the clock to 9:00 AM if the day hasn't started yet
        // Otherwise, entering the menu would 'teleport' the clock back to the start.
        if (!marketOpen)
        {
            currentTimeInDay = 0f;
            SetClockText(9, 0, "AM"); // Sets 09:00 AM
            UpdateStatusDisplay("Market Closed. Press START to begin.");
        } else
        {
            // If the market is already running, run the math once to catch up the UI
            UpdateClockMath();
        }

            // Ensure the UI shows the current state (balances, 09:00 AM clock, etc.)
            dayText.text = $"Day: {currentDay}";
        UpdateBalanceDisplays(); // Set Currency Balances
        UpdateExchangeRate(); // Set Exchange Rate
    }

    public void ShowPortfolio()
    {
        mapHubPanel.SetActive(false);
        portfolioPanel.SetActive(true);

        // Show total networth
        UpdateNetWorthDisplay();
    }

    public void ShowMapHub()
    {
        mapHubPanel.SetActive(true);
        japanTradingPanel.SetActive(false);
        portfolioPanel.SetActive(false);
    }

    // --- Button Functions ---

    public void BuyDynamicJPY()
    {
        // Instead of multiplying by the raw value, 
        // we multiply by the fraction
        double fraction = (double)buySlider.value / (double)buySlider.maxValue;
        double usdToSpend = usdBalance * fraction;

        if (usdToSpend > 0.01)
        {
            double jpyGained = usdToSpend * jpyExchangeRate;
            usdBalance -= usdToSpend;
            jpyBalance += jpyGained;
            UpdateBalanceDisplays();
            Debug.Log($"Buy: Spent {fraction*100:F0}% (${usdToSpend:F2}) for ¥{jpyGained:F0}");
        } else {
            Debug.Log("Not enough USD to buy JPY.");
        }
    }

    public void SellDynamicJPY()
    {
        // Instead of multiplying by the raw value, 
        // we multiply by the fraction
        double fraction = (double)sellSlider.value / (double)sellSlider.maxValue;
        double jpyToSpend = jpyBalance * fraction;

        if (jpyToSpend > 0.01)
        {
            double usdGained = jpyToSpend / jpyExchangeRate;
            usdBalance += usdGained;
            jpyBalance -= jpyToSpend;
            UpdateBalanceDisplays();
            Debug.Log($"Sell: Spent {fraction*100:F0}% (¥{jpyToSpend:F0}) for ${usdGained:F2}");
        } else {
            Debug.Log("Not enough JPY to sell for USD.");
        }
    }  
}