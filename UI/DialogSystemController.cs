using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// DialogSystemController — UIDocument'e bağlayın.
/// Pencere sürükleme, minimize ve kapatma destekler.
/// Inspector'dan stockCount, marketPrice, changePercent ve
/// customerMessages listesini doldurun.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class DialogSystemController : MonoBehaviour
{
    // ── Inspector Alanları ─────────────────────────────────────────────
    [Header("Stok & Fiyat")]
    [SerializeField] private int   stockCount    = 247;
    [SerializeField] private int   maxStock      = 500;
    [SerializeField] private float marketPrice   = 1240f;
    [SerializeField] private float changePercent = 3.2f;

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
    private int  _customerIndex   = 1;
    private int  _totalCallsToday = 0;
    private int  _selectedReply   = 0;

    // ── Pencere Sürükleme ─────────────────────────────────────────────
    private bool      _isDragging;
    private Vector2   _dragStartMouse;
    private Vector2   _dragStartWindowPos;

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
    private Label         _stockValue;
    private VisualElement _stockBarFill;
    private Label         _stockStatus;
    private Label         _marketPriceLabel;
    private Label         _priceDeltaLabel;
    private Label         _totalCallsLabel;
    private Label         _confirmedPriceLabel;
    private Label         _selectedReplyLabel;
    private TextField     _priceInput;
    private Label         _statusText;

    private VisualElement _reply1;
    private VisualElement _reply2;
    private VisualElement _reply3;

    public bool IsOpen { get; private set;}

    // ── Awake ──────────────────────────────────────────────────────────
    private void Awake()
    {
        var doc  = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        // Pencere
        _window     = root.Q<VisualElement>("window");
        _windowBody = root.Q<VisualElement>("window-body");

        // Başlangıç konumu
        _window.style.left = windowStartPos.x;
        _window.style.top  = windowStartPos.y;

        // UI referansları
        _customerTitle       = root.Q<Label>("customer-title");
        _avatarText          = root.Q<Label>("avatar-text");
        _customerStatus      = root.Q<Label>("customer-status");
        _dailyCount          = root.Q<Label>("daily-count");
        _customerMessageText = root.Q<Label>("customer-message-text");
        _stockValue          = root.Q<Label>("stock-value");
        _stockBarFill        = root.Q<VisualElement>("stock-bar-fill");
        _stockStatus         = root.Q<Label>("stock-status");
        _marketPriceLabel    = root.Q<Label>("market-price");
        _priceDeltaLabel     = root.Q<Label>("price-delta");
        _totalCallsLabel     = root.Q<Label>("total-calls");
        _confirmedPriceLabel = root.Q<Label>("confirmed-price");
        _selectedReplyLabel  = root.Q<Label>("selected-reply");
        _priceInput          = root.Q<TextField>("price-input");
        _statusText          = root.Q<Label>("status-text");

        _reply1 = root.Q<VisualElement>("reply-1");
        _reply2 = root.Q<VisualElement>("reply-2");
        _reply3 = root.Q<VisualElement>("reply-3");

        // ── Title Bar: Sürükleme ──────────────────────────────────────
        var titleBar = root.Q<VisualElement>("title-bar");
        titleBar.RegisterCallback<MouseDownEvent>(OnTitleBarMouseDown);
        titleBar.RegisterCallback<MouseMoveEvent>(OnTitleBarMouseMove);
        titleBar.RegisterCallback<MouseUpEvent>(OnTitleBarMouseUp);

        // ── Pencere Kontrol Butonları ─────────────────────────────────
        root.Q<Button>("btn-minimize").clicked += OnMinimizeClicked;
        root.Q<Button>("btn-close").clicked    += OnCloseClicked;

        // ── Cevap Seçimi ─────────────────────────────────────────────
        _reply1.RegisterCallback<ClickEvent>(_ => SelectReply(1));
        _reply2.RegisterCallback<ClickEvent>(_ => SelectReply(2));
        _reply3.RegisterCallback<ClickEvent>(_ => SelectReply(3));

        // ── Alt Butonlar ─────────────────────────────────────────────
        root.Q<Button>("btn-confirm").clicked += OnConfirmClicked;
        root.Q<Button>("btn-next").clicked    += OnNextCustomerClicked;

        // ── İlk Çizim ────────────────────────────────────────────────
        RefreshUI();
        UpdateStockUI();
        UpdateMarketPriceUI();
    }

    // ══════════════════════════════════════════════════════════════════
    //  PENCERE SÜRÜKLEME
    // ══════════════════════════════════════════════════════════════════

    private void Start()
    {
        Hide();       
    }
    private void OnTitleBarMouseDown(MouseDownEvent e)
    {
        if (e.button != 0) return;
        _isDragging         = true;
        _dragStartMouse     = new Vector2(e.mousePosition.x, e.mousePosition.y);
        _dragStartWindowPos = new Vector2(_window.resolvedStyle.left, _window.resolvedStyle.top);
        var titleBar = _window.Q<VisualElement>("title-bar");
        titleBar.CaptureMouse();
        e.StopPropagation();
    }

    private void OnTitleBarMouseMove(MouseMoveEvent e)
    {
        if (!_isDragging) return;

        Vector2 delta = new Vector2(e.mousePosition.x, e.mousePosition.y) - _dragStartMouse;
        float newLeft = _dragStartWindowPos.x + delta.x;
        float newTop  = _dragStartWindowPos.y + delta.y;

        // Sınır kontrolü (ekrandan çıkmasın)
        newLeft = Mathf.Max(0f, newLeft);
        newTop  = Mathf.Max(0f, newTop);

        _window.style.left = newLeft;
        _window.style.top  = newTop;
        e.StopPropagation();
    }

    private void OnTitleBarMouseUp(MouseUpEvent e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        var titleBar = _window.Q<VisualElement>("title-bar");
        if (titleBar.HasMouseCapture()) titleBar.ReleaseMouse();
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
        _window.style.display = DisplayStyle.None;
        Debug.Log("[DialogSystem] Pencere kapatıldı.");
        Hide();
    }

    /// <summary>Kapatılan pencereyi tekrar açar (harici çağrı).</summary>
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

        if (index == 1) _reply1.AddToClassList("selected");
        else if (index == 2) _reply2.AddToClassList("selected");
        else if (index == 3) _reply3.AddToClassList("selected");

        SetStatus($"Cevap #{index} seçildi.");
    }

    // ══════════════════════════════════════════════════════════════════
    //  BUTON OLAYLARI
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

        string priceStr          = $"₺{price:N0}";
        _confirmedPriceLabel.text = priceStr;
        _selectedReplyLabel.text  = $"Cevap #{_selectedReply}";

        SetStatus($"✓ Müşteri #{_customerIndex:D2} — Cevap #{_selectedReply}, {priceStr} onaylandı.");
    }

    private void OnNextCustomerClicked()
    {
        _customerIndex++;
        _totalCallsToday++;
        _selectedReply = 0;
        RefreshUI();
    }

    // ══════════════════════════════════════════════════════════════════
    //  UI GÜNCELLEME
    // ══════════════════════════════════════════════════════════════════

    private void RefreshUI()
    {
        _customerTitle.text       = $"MÜŞTERİ #{_customerIndex:D2}";
        _avatarText.text          = _customerIndex.ToString("D2");
        _dailyCount.text          = $"{_customerIndex} MÜŞTERİ";
        _totalCallsLabel.text     = _totalCallsToday.ToString();

        if (customerMessages.Count > 0)
        {
            int msgIdx = (_customerIndex - 1) % customerMessages.Count;
            _customerMessageText.text = customerMessages[msgIdx];
        }

        _reply1.RemoveFromClassList("selected");
        _reply2.RemoveFromClassList("selected");
        _reply3.RemoveFromClassList("selected");
        _priceInput.SetValueWithoutNotify("");
        _confirmedPriceLabel.text = "—";
        _selectedReplyLabel.text  = "—";

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

    public void SetCurrentCustomerMessage(string message)
        => _customerMessageText.text = message;

    /// <summary>Pencere konumunu kod ile ayarlar.</summary>
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
        UnityEngine.Cursor.visible = true;
    }
    public void Hide()
    {
        IsOpen = false;
        _window.style.display = DisplayStyle.None;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }
}
