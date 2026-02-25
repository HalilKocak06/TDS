using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class EconomyManager : MonoBehaviour
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

        //DEmo başlangıç stoğu 
        AddStock(new TireKey(195,55,16,TireSeason.Summer,TireCondition.New), 12);
        AddStock(new TireKey(205,55,16, TireSeason.Summer, TireCondition.New),8);

        OnMoneyChanged?.Invoke(money);
        OnDayChanged?.Invoke(day);
        OnInventoryChanged?.Invoke();
    }

    public int Money => money;
    public int Day => day;

    public static TireKey ToKey(TireOrder order)
        => new TireKey(order.size.width, order.size.aspect, order.size.rim, order.season, order.condition);


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
        => (order.quantity * unitSell) + laborFee;

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

            int qty = baseStockPerSku + Mathf.FloorToInt(e.demandWeight * extraStockScale / 10f);

            if(qty < 1) qty = 1;

            var key = new TireKey(e.width, e.aspect, e.rim, TireSeason.Summer, TireCondition.New);
            AddStock(key, qty);
        }

        Debug.Log($"[Economy] Seeded initial stock from catalog. Count={catalog.sizes.Count}");
    }

    public bool TryCreateRandomOrder(out TireOrder order)
    {
        order = null;
        if(catalog == null) return false;

        var pick = catalog.PickWeighted(rng);
        if(pick == null) return false;

        order = new TireOrder
        {
            size = new TireSize(pick.width, pick.aspect, pick.rim),
            season = TireSeason.Summer,
            condition = TireCondition.New,
            quantity = 4
        };

        return true;
    }


}
