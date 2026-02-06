using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Script References")]
    public CurrencyManager currencyManager;
    public CurrencyPanelID currencyPanelID;

    [Header("Containers")]
    public GameObject menuPanelsContainer;    // Folder for Map, Portfolio, etc.
    public GameObject tradingPanelsContainer; // Folder for Japan, Europe, etc.
    public GameObject tradingDashboard;       // The dashboard that appears when any trading panel is open

    private readonly List<GameObject> allPanels = new();

    void Awake()
    {
        // 1. Auto-register all Menu Panels
        foreach (Transform child in menuPanelsContainer.transform)
        {
            allPanels.Add(child.gameObject);
        }

        // 2. Auto-register all Trading Panels
        foreach (Transform child in tradingPanelsContainer.transform)
        {
            allPanels.Add(child.gameObject);
        }
    }

    public void OpenPanel(GameObject panelToOpen)
    {
        // Hide everything in both folders
        foreach (GameObject panel in allPanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        // Show the requested panel
        panelToOpen.SetActive(true);

        // Check if this panel has a CurrencyID or BankPanelID and update the CurrencyManager accordingly
        CurrencyPanelID tradeID = panelToOpen.GetComponent<CurrencyPanelID>();
        BankPanelID bankID = panelToOpen.GetComponent<BankPanelID>();

        // If the panel we just opened has a CurrencyPanelID, update the CurrencyManager UI
        if (tradeID != null)
        {
                tradingDashboard.SetActive(true);
                currencyManager.currencyPanelID = tradeID;

                if (!currencyManager.marketOpen)
                {
                    currencyManager.startDayButton.SetActive(true);
                }
        }
        else if (bankID != null)
        {
            tradingDashboard.SetActive(false);
            currencyManager.activeBankID = bankID;
            currencyManager.startDayButton.SetActive(false);
        }
        else
        {
            tradingDashboard.SetActive(false);
            currencyManager.startDayButton.SetActive(false);
            currencyManager.currencyPanelID = null;
        }
        currencyManager.RefreshUI();
    }

} 


