using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public partial class EconomyManager : MonoBehaviour
{
    public static EconomyManager I { get; private set;} //constructor

    [Header("State")]
    [SerializeField] int day = 1;
    [SerializeField] int money = 5000;

    [Header("Pricing v0")]
    [SerializeField] int unitSell = 900;
    [SerializeField] int laborFee = 300;

    //stok: SKU -> adet
    readonly Dictionary<TireKey,int> stock = new Dictionary<TireKey, int>();

    //UI / save vs dinleyebilir
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnDayChanged;
    public event Action OnInventoryChanged;

    [Header("Catalog")]
    [SerializeField] TireCatalogS0 catalog;

    [Header("Stock Seeding")]
    [SerializeField] int baseStockPerSku = 1;
    [SerializeField] int extraStockScale = 1;

    [SerializeField] int seedRandom = 12345;
    System.Random rng;

    [Header("Cost Data")]
    [SerializeField] TireCostCatalogS0 tireCostCatalog;
    [SerializeField] int fallbackUnitCost = 50;



    void Awake()
    {
        rng = new System.Random();
        if(catalog != null && catalog.sizes != null && catalog.sizes.Count > 0)
        {
            SeedInitialStockFromCatalog();
        }
        else
        {
            Debug.LogWarning("[Economy] Catalog yok! Import edip EconomyManager’a bağla.");

        }

        if( I != null && I != this) {Destroy(gameObject); return;}
        I = this;
        DontDestroyOnLoad(gameObject);


        OnMoneyChanged?.Invoke(money);
        OnDayChanged?.Invoke(day);
        OnInventoryChanged?.Invoke();
    }

    public int Money => money;
    public int Day => day;

    public static TireKey ToKey(TireOrder order)
        => new TireKey(order.size.width, order.size.aspect, order.size.rim, order.season, order.condition, order.brand);


   //MONEY
   public void AddMoney(int amount, string reason = "")
    {
        money += amount;
        Debug.Log($"[Economy] Money {(amount >= 0 ? "+" : "")}{amount} => {money}. {reason}");
        OnMoneyChanged?.Invoke(money);
    }     

    //DAY
    public void NextDay()
    {
        day++;
        Debug.Log($"[Economy] Day -> {day}");
        OnDayChanged?.Invoke(day);

        // v0: ileride kira/maaş, fabrika ETA burada
    }

    //INventory
    public int GetStock(TireKey key) => stock.TryGetValue(key, out var v) ? v : 0;

    public void AddStock(TireKey key , int qty)
    {
        if(qty <= 0 ) return;
        stock[key] = GetStock(key) + qty;
        OnInventoryChanged?.Invoke();
    }

    bool TryRemoveStock(TireKey key, int qty)
    {
        if(qty <= 0) return true;
        int have = GetStock(key);
        if(have < qty) return false;
        stock[key] = have - qty;
        OnInventoryChanged?.Invoke();
        return true;
    }

    public Dictionary<TireKey, int> StockSnapShot()
        => new Dictionary<TireKey, int>(stock);

    public string BuildStockSummary(int maxLines = 6)
    {
        var sb = new StringBuilder();
        int line = 0;

        foreach(var kv in stock)
        {
            if(kv.Value <= 0 ) continue;
            sb.AppendLine($"{kv.Key}: {kv.Value}");
            line++;
            if(line >= maxLines) break;
        }

        if(line == 0) sb.AppendLine("(stock empty)");
        return sb.ToString();
    }    
    //Pricing
    public int QuoteCustomerTotal(TireOrder order)
        => CalculateTotalPrice(GetMarketUnitPrice(order), order.quantity);

    //Integration
    //DealACcepteed'ta çağıracağız : stok varsa düş yoksa reject
    public bool TryAcceptCustomerOrder(TireOrder order , out int quoteTotal)
    {
        quoteTotal = QuoteCustomerTotal(order);
        var key = ToKey(order);

        if(!TryRemoveStock(key, order.quantity))
        {
            Debug.Log($"[Economy] Reject: {key} need={order.quantity} have={GetStock(key)}");
            return false;
        }
        Debug.Log($"[Economy] Accept: {key} x{order.quantity} quote={quoteTotal}");
        return true;
    }

    // Validate true olduğunda çağıracağız
    public void PayForCompletedJob(TireOrder order, int quoteTotal)
    {
        AddMoney(quoteTotal, $"JobComplete {ToKey(order)} x{order.quantity}");
    }

    void SeedInitialStockFromCatalog()
    {
        //Şimdilik sadece Summer + new stokluyoruz
        //sonra season ve condition yapacağım
        foreach(var e in catalog.sizes)
        {
            if(e == null) continue;

            int qty = 4;

            var kodemaxKey = new TireKey(e.width, e.aspect, e.rim, TireSeason.Summer, TireCondition.New, TireBrand.Kodemax);
            var michealKey = new TireKey(e.width, e.aspect,  e.rim, TireSeason.Summer,TireCondition.New, TireBrand.Micheal);

            AddStock(kodemaxKey, qty);
            AddStock(michealKey,qty);
        }

        Debug.Log($"[Economy] Seeded initial stock from catalog. Count={catalog.sizes.Count}");
    }

    public WorldSeason currentWorldSeason = WorldSeason.Summer;
    public bool TryCreateRandomOrder(out TireOrder order)
    {
        order = null;
        if(catalog == null) return false;

        var pick = catalog.PickWeighted(rng);
        if(pick == null) return false;

        var wantedSeason = DemandRng.PickWantedTireSeason(currentWorldSeason, rng);

        var wantedBrand = DemandRng.PickWantedBrand(wantedSeason, rng);

        order = new TireOrder
        {
            size = new TireSize(pick.width, pick.aspect, pick.rim),
            season = wantedSeason,
            condition = TireCondition.New,
            brand = wantedBrand,
            quantity = 4
        };

                var key = ToKey(order);

        int baseCost = GetUnitCost(order);
        int marketUnitPrice = GetMarketUnitPrice(order);
        int totalAtMarket = CalculateTotalPrice(marketUnitPrice, order.quantity);

        Debug.Log(
            $"[ECON TEST] ORDER -> {key} | qty={order.quantity} | baseCost={baseCost} | marketPrice={marketUnitPrice} | totalAtMarket={totalAtMarket}"
        );

        return true;
    }

    public string BuildWantedLabel(TireOrder order)
    {
        return $"{order.brand} {order.size.width}/{order.size.aspect}R{order.size.rim} {order.season}";
    }

    public int GetMarketMarkupPercent(TireOrder order)
    {
        int cost = GetUnitCost(order);
        int market = GetMarketUnitPrice(order);

        if (cost <= 0) return 0;

        return Mathf.RoundToInt(((market - cost) / (float)cost) * 100f);
    }




}

public partial class EconomyManager
{
    [Header("Market (V0)")]
    [SerializeField] private int baseMarketPrice = 1250;
    [SerializeField] private float monthlySwing = 0.03f; //+-%3

    private int cachedMarketMonth = -1;
    private float cachedMarketMultiplier = 1f;


    // 2) Market price (şimdilik SKU’ya göre basit: base * sizeFactor * monthMultiplier)
    public int GetMarketPrice(TireKey key, int monthIndex)
    {
        EnsureMarketMultiplier(monthIndex);

        float sizeFactor = 1f + Mathf.Clamp((key.width - 195) / 100f, -0.2f, 0.4f)
                             + Mathf.Clamp((key.rim - 16) / 10f, -0.1f, 0.3f);

        float p = baseMarketPrice * sizeFactor * cachedMarketMultiplier;
        return Mathf.Max(100, Mathf.RoundToInt(p));
    }

    private void EnsureMarketMultiplier(int monthIndex)
    {
        if (cachedMarketMonth == monthIndex) return;

        // deterministic olsun diye day/seed vs ile besleyebilirsin.
        // şimdilik random swing: 1 +- monthlySwing
        float swing = UnityEngine.Random.Range(-monthlySwing, monthlySwing);
        cachedMarketMultiplier = 1f + swing;
        cachedMarketMonth = monthIndex;
    }

    // 3) Offer evaluation
    public OfferEval EvaluateOffer(CustomerProfileSO profile, int offerUnitPrice, int marketUnitPrice, int offerTurnIndex)
    {
        if (profile == null) return OfferEval.Reject("profile missing");

        // turn limiti
        if (offerTurnIndex >= profile.maxOfferTurns)
            return OfferEval.Reject("Ben bi etrafa bakayim, ben çıkıyorum.");

        if (profile.willNeverBuy || profile.type == CustomerType.PriceOnly)
            return OfferEval.Reject("Sadece fiyat bakıyorum, almayacağım.");

        int maxAcceptPrice = marketUnitPrice;

        switch(profile.type)
        {
            case CustomerType.Premium:
                    maxAcceptPrice = Mathf.RoundToInt(marketUnitPrice * 1.10f);
                    break;

            case CustomerType.Standard:
                    maxAcceptPrice = marketUnitPrice;
                    break;

            case CustomerType.Cheap:
                    maxAcceptPrice = Mathf.RoundToInt(marketUnitPrice * 0.95f);
                    break;

            case CustomerType.Referral:
                    maxAcceptPrice = Mathf.RoundToInt(marketUnitPrice * 1.05f);
                    break;

            case CustomerType.PriceOnly:
                    return OfferEval.Reject("Ben bi etrafa bakayım");

            default:
                    maxAcceptPrice = marketUnitPrice;
                    break;        

        }

        if(offerUnitPrice <= maxAcceptPrice)
                return OfferEval.Accept("Tamamdır halledelim");

        return OfferEval.Counter(
            counterUnitPrice: maxAcceptPrice,
            reason: $"Bu fiyat fazla . En fazla .."
        );        
    }

    // Pazarlık sonucu STOĞU REZERVE ET (para job sonrası alınır)
    public bool TryReserveStock(TireKey key, int qty)
        => TryRemoveStock(key, qty);

    // Pazarlık sonucu ödeme (job bitince)
    public void PayForCompletedJobByDeal(int totalPrice, string reason)
        => AddMoney(totalPrice, reason);

    public int GetUnitCost(TireKey key)
    {
        if(tireCostCatalog!= null && tireCostCatalog.TryGetCost(key, out int cost))
        {
            return cost;
        }

        Debug.LogWarning($"[Economy] Cost not found for {key}, using fallback.");

        return fallbackUnitCost;
    }

    public int GetUnitCost(TireOrder order)
    {
        return GetUnitCost(ToKey(order));
    }

    public int GetOrderProductCost(TireOrder order)
    {
        int unitCost = GetUnitCost(order);

        return unitCost * order.quantity;
    }

    public int GetOrderGrossProfit(TireOrder order, int unitSellPrice)
    {

        int revenue = (unitSellPrice * order.quantity) ;

        int productCost = GetOrderProductCost(order);

        return revenue - productCost;

    }    

    public int GetStockForOrder(TireOrder order)
    {
        return GetStock(ToKey(order));
    }

    public int GetMarketUnitPrice(TireOrder order)
    {
        int cost = GetUnitCost(order);
        return Mathf.Max(cost, Mathf.RoundToInt(cost * 1.20f));
    }

    public int CalculateTotalPrice(int unitPrice, int quantity)
    {
        return (unitPrice * quantity) ;
    }

    public int GetMarketTotalPrice(TireOrder order)
    {
        return CalculateTotalPrice(GetMarketUnitPrice(order), order.quantity);
    }

}

public readonly struct OfferEval
{
    public readonly bool accepted;
    public readonly bool rejected;
    public readonly bool counter;
    public readonly int counterUnitPrice;
    public readonly string message;

    private OfferEval(bool a, bool r, bool c, int cu, string m)
    { accepted = a; rejected = r; counter = c; counterUnitPrice = cu; message = m; }

    public static OfferEval Accept(string msg) => new OfferEval(true, false, false, 0, msg);
    public static OfferEval Reject(string msg) => new OfferEval(false, true, false, 0, msg);
    public static OfferEval Counter(int counterUnitPrice, string reason) => new OfferEval(false, false, true, counterUnitPrice, reason);

    

}
