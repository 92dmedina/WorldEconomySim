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

        // Check if this panel has a CurrencyID
        CurrencyPanelID id = panelToOpen.GetComponent<CurrencyPanelID>();

        // Show the requested panel
        panelToOpen.SetActive(true);

        // If the panel we just opened is inside the Trading folder...
        if (id != null && !string.IsNullOrEmpty(id.currencyName))
        {
                currencyManager.currencyPanelID = id;
                currencyManager.currencyText.text = $"{id.currencyName}: {id.currencySymbol}{id.currencyBalance:F0}";
                currencyManager.UpdateExchangeRate(id.currentExchangeRate);
                currencyManager.RefreshUI();
        }

        // Manage the Start Button visibility
        if (id != null && !currencyManager.marketOpen)
        {
            currencyManager.startDayButton.SetActive(true);
        }
        else
        {
            currencyManager.startDayButton.SetActive(false); // Hide it on Map/Settings
        }
    } 
}


