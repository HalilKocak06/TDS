using Microsoft.Win32.SafeHandles;
using TMPro;
using UnityEngine;

public class EconomyHudUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI dayText;
    [SerializeField] TextMeshProUGUI stockText;

    void Start()
    {
        if(EconomyManager.I == null)
        {
            Debug.LogWarning("[HUD] EconomyManager yok! Sahneye EconomyManager ekle.");
            return;
        }

        RefreshAll();

        EconomyManager.I.OnMoneyChanged += HandleMoney;
        EconomyManager.I.OnDayChanged += HandleDay;
        EconomyManager.I.OnInventoryChanged += HandleInv;
    }

    void OnDestroy()
    {
        if(EconomyManager.I == null) return;
        EconomyManager.I.OnMoneyChanged -= HandleMoney;
        EconomyManager.I.OnDayChanged -= HandleDay;
        EconomyManager.I.OnInventoryChanged -= HandleInv;
    }

    void RefreshAll()
    {
        HandleMoney(EconomyManager.I.Money);
        HandleDay(EconomyManager.I.Day);
        HandleInv();
    }

    void HandleMoney(int m)
    {
        if (moneyText) moneyText.text = $"Money: {m}";
    }

    void HandleDay(int d)
    {
        if (dayText) dayText.text = $"Day: {d}";
    }

    void HandleInv()
    {
        if (stockText) stockText.text = EconomyManager.I.BuildStockSummary(10);
    }
}
