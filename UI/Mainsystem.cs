using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// DialogSystemController — UIDocument'e bağlayın.
/// UXML: DialogSystem-Tam.uxml baz alınarak yazılmıştır.
/// Inspector'dan stockCount, marketPrice, changePercent, costPrice ve
/// customerMessages listesini doldurun.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class mainSystem : MonoBehaviour
{
    // ── Inspector Alanları ─────────────────────────────────────────────
    [Header("Stok & Fiyat")]
    [SerializeField] private int   stockCount    = 247;
    [SerializeField] private int   maxStock      = 500;
    [SerializeField] private float marketPrice   = 1240f;
    [SerializeField] private float changePercent = 3.2f;
    [SerializeField] private float costPrice     = 0f;

    [Header("Müşteri Mesajları (sırayla gelir)")]
    [SerializeField] private List<string> customerMessages = new()
    {
        "Merhaba, bu ürünün fiyatı hakkında bilgi alabilir miyim?",
        "Stok durumu nedir, hemen teslim edilebilir mi?",
        "En düşük fiyatınız kaç?"
    };

    [Header("Pencere Başlangıç Konumu")]
    [SerializeField] private Vector2 windowStartPos = new Vector2(120f, 60f);

    // ── Durum ──────────────────────────────────────────────────────────
    private int _customerIndex   = 1;
    private int _totalCallsToday = 0;
    private int _selectedReply   = 0;

    // ── Pencere Sürükleme ─────────────────────────────────────────────
    private bool    _isDragging;
    private Vector2 _dragStartMouse;
    private Vector2 _dragStartWindowPos;

    // ── Minimize ──────────────────────────────────────────────────────
    private bool _isMinimized;

    // ── UI Element Referansları ────────────────────────────────────────
    private VisualElement _window;
    private VisualElement _windowBody;

    private Label         _customerTitle;
    private Label         _avatarText;
    private Label         _customerStatus;
    private Label         _dailyCount;
    private Label         _customerMessageText;

    private VisualElement _reply1;
    private VisualElement _reply2;
    private VisualElement _reply3;

    private TextField     _priceInput;

    private Label         _stockValue;
    private VisualElement _stockBarFill;
    private Label         _stockStatus;

    private Label         _marketPriceLabel;
    private Label         _priceDeltaLabel;

    private Label         _costPriceLabel;
    private Label         _costDeltaLabel;

    private Label         _statusText;

    public bool IsOpen { get; private set; }

    // ── Awake ──────────────────────────────────────────────────────────
    private void Awake()
    {
        var doc  = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        _window     = root.Q<VisualElement>("window");
        _windowBody = root.Q<VisualElement>("window-body");

        _window.style.left = windowStartPos.x;
        _window.style.top  = windowStartPos.y;

        _customerTitle       = root.Q<Label>("customer-title");
        _avatarText          = root.Q<Label>("avatar-text");
        _customerStatus      = root.Q<Label>("customer-status");
        _dailyCount          = root.Q<Label>("daily-count");
        _customerMessageText = root.Q<Label>("customer-message-text");

        _reply1 = root.Q<VisualElement>("reply-1");
        _reply2 = root.Q<VisualElement>("reply-2");
        _reply3 = root.Q<VisualElement>("reply-3");

        _priceInput = root.Q<TextField>("price-input");

        _stockValue   = root.Q<Label>("stock-value");
        _stockBarFill = root.Q<VisualElement>("stock-bar-fill");
        _stockStatus  = root.Q<Label>("stock-status");

        _marketPriceLabel = root.Q<Label>("market-price");
        _priceDeltaLabel  = root.Q<Label>("price-delta");

        _costPriceLabel = root.Q<Label>("cost-price");
        _costDeltaLabel = root.Q<Label>("cost-delta");

        _statusText = root.Q<Label>("status-text");

        // Title Bar sürükleme
        var titleBar = root.Q<VisualElement>("title-bar");
        titleBar.RegisterCallback<MouseDownEvent>(OnTitleBarMouseDown);
        titleBar.RegisterCallback<MouseMoveEvent>(OnTitleBarMouseMove);
        titleBar.RegisterCallback<MouseUpEvent>(OnTitleBarMouseUp);

        // Pencere kontrol butonları
        root.Q<Button>("btn-minimize").clicked += OnMinimizeClicked;
        root.Q<Button>("btn-close").clicked    += OnCloseClicked;

        // Cevap seçimi
        _reply1.RegisterCallback<ClickEvent>(_ => SelectReply(1));
        _reply2.RegisterCallback<ClickEvent>(_ => SelectReply(2));
        _reply3.RegisterCallback<ClickEvent>(_ => SelectReply(3));

        // Onayla butonu
        root.Q<Button>("btn-confirm").clicked += OnConfirmClicked;

        // İlk çizim
        RefreshUI();
        UpdateStockUI();
        UpdateMarketPriceUI();
        UpdateCostUI();

        // Play başlarken kapalı — Show() ile açılır
        Hide();
    }

    // ══════════════════════════════════════════════════════════════════
    //  PENCERE SÜRÜKLEME
    // ══════════════════════════════════════════════════════════════════

    private void OnTitleBarMouseDown(MouseDownEvent e)
    {
        if (e.button != 0) return;
        _isDragging         = true;
        _dragStartMouse     = new Vector2(e.mousePosition.x, e.mousePosition.y);
        _dragStartWindowPos = new Vector2(_window.resolvedStyle.left, _window.resolvedStyle.top);
        _window.Q<VisualElement>("title-bar").CaptureMouse();
        e.StopPropagation();
    }

    private void OnTitleBarMouseMove(MouseMoveEvent e)
    {
        if (!_isDragging) return;
        Vector2 delta = new Vector2(e.mousePosition.x, e.mousePosition.y) - _dragStartMouse;
        _window.style.left = Mathf.Max(0f, _dragStartWindowPos.x + delta.x);
        _window.style.top  = Mathf.Max(0f, _dragStartWindowPos.y + delta.y);
        e.StopPropagation();
    }

    private void OnTitleBarMouseUp(MouseUpEvent e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        var tb = _window.Q<VisualElement>("title-bar");
        if (tb.HasMouseCapture()) tb.ReleaseMouse();
        e.StopPropagation();
    }

    // ══════════════════════════════════════════════════════════════════
    //  MİNİMİZE / KAPAT
    // ══════════════════════════════════════════════════════════════════

    private void OnMinimizeClicked()
    {
        _isMinimized = !_isMinimized;
        _windowBody.style.display = _isMinimized ? DisplayStyle.None : DisplayStyle.Flex;
        SetStatus(_isMinimized ? "Pencere küçültüldü." : "Pencere geri getirildi.");
    }

    private void OnCloseClicked()
    {
        Hide();
        Debug.Log("[DialogSystem] Pencere kapatıldı.");
    }

    public void OpenWindow()
    {
        _window.style.display     = DisplayStyle.Flex;
        _windowBody.style.display = DisplayStyle.Flex;
        _isMinimized              = false;
    }

    // ══════════════════════════════════════════════════════════════════
    //  CEVAP SEÇİMİ
    // ══════════════════════════════════════════════════════════════════

    private void SelectReply(int index)
    {
        _selectedReply = index;
        _reply1.RemoveFromClassList("selected");
        _reply2.RemoveFromClassList("selected");
        _reply3.RemoveFromClassList("selected");

        if      (index == 1) _reply1.AddToClassList("selected");
        else if (index == 2) _reply2.AddToClassList("selected");
        else if (index == 3) _reply3.AddToClassList("selected");

        SetStatus($"Cevap #{index} seçildi.");
    }

    // ══════════════════════════════════════════════════════════════════
    //  ONAYLA BUTONU
    // ══════════════════════════════════════════════════════════════════

    private void OnConfirmClicked()
    {
        if (_selectedReply == 0)
        {
            SetStatus("⚠  Lütfen önce bir cevap seçin.");
            return;
        }

        string raw = _priceInput.value.Trim().Replace(",", ".");
        if (!float.TryParse(raw,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out float price) || price <= 0)
        {
            SetStatus("⚠  Geçerli bir fiyat giriniz.");
            return;
        }

        string priceStr = $"₺{price:N0}";
        SetStatus($"✓ Müşteri #{_customerIndex:D2} — Cevap #{_selectedReply}, {priceStr} onaylandı.");
    }

    // ══════════════════════════════════════════════════════════════════
    //  UI GÜNCELLEME
    // ══════════════════════════════════════════════════════════════════

    private void RefreshUI()
    {
        _customerTitle.text = $"MÜŞTERİ #{_customerIndex:D2}";
        _avatarText.text    = _customerIndex.ToString("D2");
        _dailyCount.text    = $"{_customerIndex} MÜŞTERİ";

        if (customerMessages.Count > 0)
        {
            int msgIdx = (_customerIndex - 1) % customerMessages.Count;
            _customerMessageText.text = customerMessages[msgIdx];
        }

        _reply1.RemoveFromClassList("selected");
        _reply2.RemoveFromClassList("selected");
        _reply3.RemoveFromClassList("selected");
        _priceInput.SetValueWithoutNotify("");

        SetStatus($"Müşteri #{_customerIndex:D2} görüşmeye hazır.");
    }

    private void UpdateStockUI()
    {
        _stockValue.text = stockCount.ToString();

        float ratio = Mathf.Clamp01((float)stockCount / maxStock);
        _stockBarFill.style.width = Length.Percent(ratio * 100f);

        _stockStatus.RemoveFromClassList("status-ok");
        _stockStatus.RemoveFromClassList("status-warn");
        _stockStatus.RemoveFromClassList("status-crit");

        if (ratio > 0.5f)
        {
            _stockStatus.text = "Stok sağlıklı";
            _stockStatus.AddToClassList("status-ok");
            _stockBarFill.style.backgroundColor = new Color(0.18f, 0.80f, 0.44f);
        }
        else if (ratio > 0.2f)
        {
            _stockStatus.text = "Stok azalıyor";
            _stockStatus.AddToClassList("status-warn");
            _stockBarFill.style.backgroundColor = new Color(0.95f, 0.61f, 0.07f);
        }
        else
        {
            _stockStatus.text = "Kritik stok!";
            _stockStatus.AddToClassList("status-crit");
            _stockBarFill.style.backgroundColor = new Color(0.91f, 0.30f, 0.24f);
        }
    }

    private void UpdateMarketPriceUI()
    {
        _marketPriceLabel.text = $"₺{marketPrice:N0}";

        if (changePercent >= 0)
        {
            _priceDeltaLabel.text = $"▲ %{changePercent:F1}";
            _priceDeltaLabel.RemoveFromClassList("delta-down");
            _priceDeltaLabel.AddToClassList("delta-up");
        }
        else
        {
            _priceDeltaLabel.text = $"▼ %{Mathf.Abs(changePercent):F1}";
            _priceDeltaLabel.RemoveFromClassList("delta-up");
            _priceDeltaLabel.AddToClassList("delta-down");
        }
    }

    private void UpdateCostUI()
    {
        if (_costPriceLabel == null) return;

        _costPriceLabel.text = $"₺{costPrice:N0}";

        if (costPrice > 0 && marketPrice > 0)
        {
            float pct = ((marketPrice - costPrice) / costPrice) * 100f;
            if (pct >= 0)
            {
                _costDeltaLabel.text        = $"▲ %{pct:F1} kâr";
                _costDeltaLabel.style.color = new StyleColor(new Color(0.31f, 0.63f, 0.31f));
            }
            else
            {
                _costDeltaLabel.text        = $"▼ %{Mathf.Abs(pct):F1} zarar";
                _costDeltaLabel.style.color = new StyleColor(new Color(0.85f, 0.25f, 0.15f));
            }
        }
        else
        {
            _costDeltaLabel.text = "— %0";
        }
    }

    private void SetStatus(string msg)
        => _statusText.text = $"[{DateTime.Now:HH:mm:ss}]  {msg}";

    // ══════════════════════════════════════════════════════════════════
    //  HARİCİ API
    // ══════════════════════════════════════════════════════════════════

    public void SetStock(int count)
    {
        stockCount = count;
        UpdateStockUI();
    }

    public void SetMarketPrice(float price, float change)
    {
        marketPrice   = price;
        changePercent = change;
        UpdateMarketPriceUI();
    }

    public void SetCostPrice(float cost)
    {
        costPrice = cost;
        UpdateCostUI();
    }

    public void SetCurrentCustomerMessage(string message)
        => _customerMessageText.text = message;

    public void NextCustomer()
    {
        _customerIndex++;
        _totalCallsToday++;
        _selectedReply = 0;
        RefreshUI();
    }

    public void SetWindowPosition(float x, float y)
    {
        _window.style.left = x;
        _window.style.top  = y;
    }

    public void Show()
    {
        IsOpen = true;
        OpenWindow();
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible   = true;
    }

    public void Hide()
    {
        IsOpen                = false;
        _window.style.display = DisplayStyle.None;
        UnityEngine.Cursor.lockState      = CursorLockMode.Locked;
        UnityEngine.Cursor.visible        = false;
    }

    public void RefreshEconomyPanel(TireOrder order)
    {
        if(order == null)
        {
            Debug.LogWarning("[DialogSystem] RefreshEconomyPanel -> order null");
            return;
        }

        if(EconomyManager.I == null)
        {
            Debug.LogWarning("[DialogSystem] RefreshEconomyPanel -> EconomyManager null");
            return;
        }

        int stock = EconomyManager.I.GetStockForOrder(order);
        int unitCost = EconomyManager.I.GetUnitCost(order);
        int marketUnitPrice = EconomyManager.I.GetMarketUnitPrice(order);
        int markupPercent = EconomyManager.I.GetMarketMarkupPercent(order);

        SetStock(stock);
        SetMarketPrice(marketUnitPrice, 20f);
        SetCostPrice(unitCost);

        SetCurrentCustomerMessage(BuildCustomerOrderMessage(order));

        SetStatus(
        $"Ekonomi paneli güncellendi | stok={stock} | maliyet={unitCost} | piyasa={marketUnitPrice} | kâr=%{markupPercent}"
        );

    }

    public string BuildCustomerOrderMessage(TireOrder order)
    {
        if (order == null) return "Sipariş bilgisi yok.";

        return $"{order.brand} {order.size.width}/{order.size.aspect}R{order.size.rim} {order.season} istiyorum.";
    }
}
