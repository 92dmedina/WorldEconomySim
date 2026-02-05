using UnityEngine;

public class BankPanelID : MonoBehaviour
{
    [Header("Bank Account Details")]
    public string bankName = "Federal Reserve";
    public double savingsBalance = 0.00; // Money inside the bank

    [Header("Interest Settings")]
    public double dailyInterestRate = 0.001; // 0.1% daily interest
}
