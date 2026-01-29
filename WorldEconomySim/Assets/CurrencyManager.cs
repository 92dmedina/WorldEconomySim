using UnityEngine;
using TMPro;
using UnityEngine.UI; // Required for the Slider

public class CurrencyManager : MonoBehaviour
{
    [Header("Currency Values")]
    public double usdBalance = 100.0;
    public double jpyBalance = 0.0;

    [Header("Market Dynamics")]
    public double jpyExchangeRate = 150.0;
    public double marketTrend = 0.0;

    [Header("Day Cycle Settings")]
    public float dayDuration = 60f; 
    private float currentTimeInDay = 0f;
    private float scheduledEventTime;
    private int currentDay = 1;
    private bool eventFiredToday = false;

    [Header("Dynamic Trading")]
    public Slider buySlider;
    public Slider sellSlider;

    [Header("UI Text Elements")]
    public TextMeshProUGUI usdText;
    public TextMeshProUGUI jpyText;
    public TextMeshProUGUI rateText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI newsText;
    public TextMeshProUGUI buyPercentageText;
    public TextMeshProUGUI sellPercentageText;

    // --- Unity Functions ---

    void Start()
    {
        // Initialize balances
        usdBalance = 100.0;
        jpyBalance = 0.0;

        // The PULSE: Creates constant market vibration every 0.5s
        InvokeRepeating("UpdateMarketPulse", 0.5f, 0.5f);

        StartNewDay();
    }

    void Update()
    {
        // 1. Advance the clock for the Day Cycle
        currentTimeInDay += Time.deltaTime;

        // 2. Check if it's time for the daily news event
        if (!eventFiredToday && currentTimeInDay >= scheduledEventTime)
        {
            TriggerEconomyEvent();
            eventFiredToday = true; 
        }

        // 3. Check if the day is over
        if (currentTimeInDay >= dayDuration)
        {
            currentDay++;
            StartNewDay();
        }

        // 4. Update the Slider percentage text
        if (buyPercentageText != null && buySlider != null)
        {
            // We divide the slider value by its own Max Value (2) to get the fraction
            float fraction = buySlider.value / buySlider.maxValue; 
            float percent = fraction * 100f;
        
            buyPercentageText.text = percent.ToString("F0") + "%";
        }

        if (sellPercentageText != null && sellSlider != null)
        {
            float fraction = sellSlider.value / sellSlider.maxValue; 
            float percent = fraction * 100f;
        
            sellPercentageText.text = percent.ToString("F0") + "%";
        }
        
    }

    // --- Internal Logic (The Brain) ---

    void StartNewDay()
    {
        currentTimeInDay = 0f;
        eventFiredToday = false;

        // Pick a random time for news to hit (between 5s and 55s)
        scheduledEventTime = Random.Range(5f, dayDuration - 5f);

        Debug.Log($"DAY {currentDay} STARTED.");
        Debug.Log($"News at: {scheduledEventTime:F1}s");
        UpdateTextDisplays();
    }

    void UpdateMarketPulse()
    {
        // Add tiny random vibration + current trend direction
        double noise = Random.Range(-0.03f, 0.03f); 
        jpyExchangeRate += marketTrend + noise;

        // Keep a realistic floor (Yen shouldn't be free!)
        if (jpyExchangeRate < 100.0) jpyExchangeRate = 100.0;

        UpdateTextDisplays();
    }

    void TriggerEconomyEvent()
    {
        int eventRoll = Random.Range(1, 4);
        string message = "";

        if (eventRoll == 1) // US BOOM
        {
            marketTrend = 0.1;
            message = "US Tech Sector reports record profits. USD strengthening.";
        }
        else if (eventRoll == 2) // JAPAN BOOM
        {
            marketTrend = -0.1;
            message = "Japan manufacturing surge beats estimates. JPY strengthening.";
        }
        else // STAGNATION
        {
            marketTrend = 0.0;
            message = "Global markets remain steady. No major news today.";
        }

        if (newsText != null) newsText.text = message;
        Debug.Log("EVENT: " + message);
    }

    void UpdateTextDisplays()
    {
        // Using prefixes so the labels always stay visible
        usdText.text = $"USD: ${usdBalance:F2}";
        jpyText.text = $"JPY: ¥{jpyBalance:F0}";
        rateText.text = $"Rate: 1 USD = {jpyExchangeRate:F2} JPY";
        
        if (dayText != null) dayText.text = "Day: " + currentDay;
    }

    // --- Button Functions ---

    public void BuyDynamicJPY()
    {
        // Instead of multiplying by the raw value (0-2), 
        // we multiply by the fraction (0-1)
        double fraction = (double)buySlider.value / (double)buySlider.maxValue;
        double usdToSpend = usdBalance * fraction;

        if (usdToSpend > 0.01)
        {
            double jpyGained = usdToSpend * jpyExchangeRate;
            usdBalance -= usdToSpend;
            jpyBalance += jpyGained;
            UpdateTextDisplays();
            Debug.Log($"Buy: Spent {fraction*100:F0}% (${usdToSpend:F2}) for ¥{jpyGained:F0}");
        } else {
            Debug.Log("Not enough USD to buy JPY.");
        }
    }

    public void SellDynamicJPY()
    {
        // Instead of multiplying by the raw value (0-2), 
        // we multiply by the fraction (0-1)
        double fraction = (double)sellSlider.value / (double)sellSlider.maxValue;
        double jpyToSpend = jpyBalance * fraction;

        if (jpyToSpend > 0.01)
        {
            double usdGained = jpyToSpend / jpyExchangeRate;
            usdBalance += usdGained;
            jpyBalance -= jpyToSpend;
            UpdateTextDisplays();
            Debug.Log($"Sell: Spent {fraction*100:F0}% (¥{jpyToSpend:F0}) for ${usdGained:F2}");
        } else {
            Debug.Log("Not enough JPY to sell for USD.");
        }
    }  
}