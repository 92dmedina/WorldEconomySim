using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
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
    private int currentDay = 1;
    private bool eventFiredToday = false;
    private bool marketOpen = true;

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

    void Start()
    {
        usdBalance = 100.0;
        jpyBalance = 0.0;
        
        // Hide the Start Day button since we start in progress
        if (startDayButton != null) startDayButton.SetActive(false);

        InvokeRepeating("UpdateMarketPulse", 0.5f, 0.5f);
        StartNewDay();
    }

    void Update()
    {
        // This allows the percentages to update even when the market is closed
        if (buyPercentageText != null && buySlider != null)
            buyPercentageText.text = (buySlider.value / buySlider.maxValue * 100f).ToString("F0") + "%";

        if (sellPercentageText != null && sellSlider != null)
            sellPercentageText.text = (sellSlider.value / sellSlider.maxValue * 100f).ToString("F0") + "%";


        // Everything BELOW this line will only happen if the market is open
        if (!marketOpen) return;


        // CLOCK & GAME LOGIC
        currentTimeInDay += Time.deltaTime;

        // Clock Display Logic (9 AM - 5 PM)
        if (clockText != null)
        {
            float dayPercentage = currentTimeInDay / dayDuration;
            float currentHourDecimal = 9.0f + (dayPercentage * 8.0f);
            int hours = Mathf.FloorToInt(currentHourDecimal);
            int minutes = Mathf.FloorToInt((currentHourDecimal - hours) * 60);
            string period = (hours >= 12) ? "PM" : "AM";
            int displayHour = (hours > 12) ? hours - 12 : hours;
            clockText.text = $"{displayHour:D2}:{minutes:D2} {period}";
        }

        // Daily News Trigger
        if (!eventFiredToday && currentTimeInDay >= scheduledEventTime)
        {
        TriggerEconomyEvent();
        eventFiredToday = true; 
        }

        // End of Day Check
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
        scheduledEventTime = Random.Range(5f, dayDuration - 5f);

        if (startDayButton != null) startDayButton.SetActive(false);
        Debug.Log($"Today's news will trigger at: {scheduledEventTime:F1}s");
        UpdateTextDisplays();
    }

    void EndTradingDay()
    {
        marketOpen = false;
        if (startDayButton != null) startDayButton.SetActive(true);
    }

    public void OnStartNextDayClicked()
    {
        currentDay++;
        StartNewDay();
    }

    void UpdateMarketPulse()
    {
        // Don't wiggle the numbers if the market is closed!
        if (!marketOpen) return;

        double noise = Random.Range(-0.01f, 0.01f); 
        jpyExchangeRate += marketTrend + noise;
        if (jpyExchangeRate < 75.0) jpyExchangeRate = 75.0;
        UpdateTextDisplays();
    }

    void TriggerEconomyEvent()
    {
        // Increased range to allow for more variety in news intensity
        int eventRoll = Random.Range(1, 7); 

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
        // Instead of multiplying by the raw value, 
        // we multiply by the fraction
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
        // Instead of multiplying by the raw value, 
        // we multiply by the fraction
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