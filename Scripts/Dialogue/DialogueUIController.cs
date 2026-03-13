using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] GameObject root; // DialogueRoot

    [Header("Main")]
    [SerializeField] TextMeshProUGUI npcText;
    [SerializeField] Transform choicesContainer;
    [SerializeField] Button closeButton;

    [Header("Offer Input (v0)")]
    [SerializeField] TMP_InputField offerInput;
    [SerializeField] Button offerSubmitButton;
    [SerializeField] TextMeshProUGUI offerErrorText; // opsiyonel (yoksa null bırak)

    [Header("Info Panel (v0 fake)")]
    [SerializeField] TextMeshProUGUI wantedText;
    [SerializeField] TextMeshProUGUI stockText;
    [SerializeField] TextMeshProUGUI marketText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] TextMeshProUGUI marketHintText;
    [SerializeField] TextMeshProUGUI costHintText;


    [Header("Choice Prefab")]
    [SerializeField] Button choiceButtonPrefab; // TMP'li Button prefab

    readonly List<Button> spawnedChoices = new List<Button>();

    public event Action OnCloseClicked;
    public event Action<int> OnOfferSubmitted;

    void Awake()
    {
        if (closeButton)
            closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());

        if (offerSubmitButton)
            offerSubmitButton.onClick.AddListener(SubmitOffer);

        if (offerInput)
            offerInput.onSubmit.AddListener(_ => SubmitOffer()); // Enter ile submit

        Hide();
    }

    public void Show()
    {
        if (root) root.SetActive(true);
        ClearOfferError();
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
        ClearChoices();
        ClearOfferError();
    }

    public void SetNpcLine(string line)
    {
        if (npcText) npcText.text = line;
    }

    public void SetInfo(string wanted, string stock, string market)
    {
        if (wantedText) wantedText.text = $"İstenen: {wanted}";
        if (stockText) stockText.text = $"Stok: {stock}";
        if (marketText) marketText.text = $"Piyasa: {market}";
    }

    public void ClearChoices()
    {
        for (int i = 0; i < spawnedChoices.Count; i++)
        {
            if (spawnedChoices[i] != null)
                Destroy(spawnedChoices[i].gameObject);
        }
        spawnedChoices.Clear();
    }

    public void AddChoice(string label, Action onClick)
    {
        if (choiceButtonPrefab == null || choicesContainer == null)
        {
            Debug.LogWarning("[DialogueUI] choiceButtonPrefab / choicesContainer missing!");
            return;
        }

        var btn = Instantiate(choiceButtonPrefab, choicesContainer);
        spawnedChoices.Add(btn);

        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp) tmp.text = label;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    // ---- Offer input helpers ----

    public void SetOfferUIVisible(bool visible)
    {
        if (offerInput) offerInput.gameObject.SetActive(visible);
        if (offerSubmitButton) offerSubmitButton.gameObject.SetActive(visible);
        ClearOfferError();
    }

    public void SetOfferPlaceholder(string text)
    {
        if (offerInput == null) return;
        var ph = offerInput.placeholder as TextMeshProUGUI;
        if (ph) ph.text = text;
    }

    public void SetOfferText(string text)
    {
        if (offerInput) offerInput.text = text;
    }

    void SubmitOffer()
    {
        ClearOfferError();

        if (offerInput == null)
        {
            Debug.LogWarning("[DialogueUI] OfferInput missing!");
            return;
        }

        string raw = offerInput.text?.Trim();

        if (string.IsNullOrEmpty(raw))
        {
            ShowOfferError("Fiyat gir.");
            return;
        }

        // "130", "130₺", "130 birim" gibi şeyleri tolere edelim: sadece sayıları çek
        int value = ExtractFirstInt(raw);
        if (value <= 0)
        {
            ShowOfferError("Geçerli bir sayı gir (örn: 130).");
            return;
        }

        OnOfferSubmitted?.Invoke(value);
    }

    int ExtractFirstInt(string s)
    {
        int result = 0;
        bool found = false;

        for (int i = 0; i < s.Length; i++)
        {
            if (char.IsDigit(s[i]))
            {
                found = true;
                result = result * 10 + (s[i] - '0');
            }
            else if (found)
            {
                break; // ilk sayı bloğu bitti
            }
        }

        return result;
    }

    void ShowOfferError(string msg)
    {
        if (offerErrorText)
        {
            offerErrorText.gameObject.SetActive(true);
            offerErrorText.text = msg;
        }
        else
        {
            Debug.LogWarning("[DialogueUI] " + msg);
        }
    }

    void ClearOfferError()
    {
        if (offerErrorText)
        {
            offerErrorText.text = "";
            offerErrorText.gameObject.SetActive(false);
        }
    }

    public void SetEconomyInfo(
        string wanted,
        int stock,
        int marketUnitPrice,
        int costUnitPrice,
        string marketHint = "",
        string costHint = "")
    {
        if(wantedText) wantedText.text = $"İstenen: {wanted}";
        if (stockText) stockText.text = stock.ToString();
        if (marketText) marketText.text = $"₺{marketUnitPrice}";
        if (costText) costText.text = $"₺{costUnitPrice}";
        if (marketHintText) marketHintText.text = marketHint;
        if (costHintText) costHintText.text = costHint;

    }

    public void RefreshEconomyPanel(TireOrder order)
    {
        if (order == null) return;
        if (EconomyManager.I == null) return;

        int stock = EconomyManager.I.GetStockForOrder(order);
        int cost = EconomyManager.I.GetUnitCost(order);
        int market = EconomyManager.I.GetMarketUnitPrice(order);
        int markupPct = EconomyManager.I.GetMarketMarkupPercent(order);
        string wanted = EconomyManager.I.BuildWantedLabel(order);

        SetEconomyInfo(
            wanted: wanted,
            stock: stock,
            marketUnitPrice: market,
            costUnitPrice: cost,
            marketHint: "▲ %20",
            costHint: $"▲ %{markupPct} kâr"
        );
    }    

}