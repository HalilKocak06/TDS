using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum DialogueState
{
    Greeting,
    AskNeed,
    PriceNegotiation,
    Accepted,
    Rejected,
    Closed
}

public class DialogueSession : MonoBehaviour
{
    public DialogueState State {get; private set;} = DialogueState.Greeting;

    public readonly TireOrder Order;
    public readonly CustomerProfileSO Profile;

    public int OfferTurnIndex {get; private set;} = 0;
    public int MarketUnitPrice { get; private set;} 
    public int AgreedUnitPrice { get; private set;} = -1;

    readonly EconomyManager economy;
    readonly Func<int> getMonthIndex;

    public event Action<string> OnNpcText;
    public event Action<string[], Action<int> > OnChoices;
    public event Action<Action<int>> OnOfferInput;
    public event Action OnCloseUI;

    public event Action<int /*totalPrice*/> OnDealAccepted;

    public event Action OnDealRejected;

    public DialogueSession(TireOrder order, CustomerProfileSO profile, EconomyManager economy, Func<int> getMonthIndex)
    {
        Order = order;
        Profile = profile;
        this.economy = economy;
        this.getMonthIndex = getMonthIndex;
    }

    public void Start()
    {
        StepGreeting();
    }

    void StepGreeting()
    {
        State = DialogueState.Greeting;
        OnNpcText?.Invoke("Selam Usta, bir lastik bakacaktım.");
        OnChoices?.Invoke(new[] {"Devam"}, _ => StepAskNeed());
    }

    void StepAskNeed()
    {
        State = DialogueState.AskNeed;

        var key = EconomyManager.ToKey(Order);
        int stock = economy.GetStock(key);

        OnNpcText?.Invoke($"Ebat: {Order.size.width}/{Order.size.aspect} R{Order.size.rim}. Stok var mı? (Stok: {stock})");
        OnChoices?.Invoke(new[] { "Var", "Yok (stok yok de)" }, idx =>
        {
            if (idx == 0)
            {
                if (stock < Order.quantity)
                {
                    StepRejected("Usta o ebat yok gibi…");
                    return;
                }
                StepPriceNegotiation();
            }
            else
            {
                StepRejected("Tamamdır, başka yere bakayım.");
            }
        });
    }

    void StepPriceNegotiation()
    {
        State = DialogueState.PriceNegotiation;

        OnNpcText?.Invoke($"Piyasa {MarketUnitPrice}. Sen kaça verirsin? (Kalan hak: {Mathf.Max(0, Profile.maxOfferTurns - OfferTurnIndex)})");
        OnOfferInput?.Invoke(offer =>
        {
            var eval = economy.EvaluateOffer(Profile, offer, MarketUnitPrice, OfferTurnIndex);
            OfferTurnIndex++;

            if (eval.accepted)
            {
                AgreedUnitPrice = offer;
                StepAccepted();
                return;
            }

            if (eval.counter)
            {
                OnNpcText?.Invoke(eval.message);
                // tekrar teklif iste
                StepPriceNegotiation();
                return;
            }

            StepRejected(eval.message);
        });
    }

    void StepAccepted()
    {
        State = DialogueState.Accepted;

        var key = EconomyManager.ToKey(Order);
        if (!economy.TryReserveStock(key, Order.quantity))
        {
            StepRejected("Bir saniye… stok bitti, kusura bakma.");
            return;
        }

        int total = AgreedUnitPrice * Order.quantity; // işçilik ekleyeceksen buraya
        OnNpcText?.Invoke($"Anlaştık. Toplam: {total}. Arabayı içeri alalım.");
        OnChoices?.Invoke(new[] { "Tamam" }, _ =>
        {
            OnDealAccepted?.Invoke(total);
            OnCloseUI?.Invoke();
            State = DialogueState.Closed;
        });
    }

    void StepRejected(string reason)
    {
        State = DialogueState.Rejected;
        OnNpcText?.Invoke(reason);
        OnChoices?.Invoke(new[] { "Kapat" }, _ =>
        {
            OnDealRejected?.Invoke();
            OnCloseUI?.Invoke();
            State = DialogueState.Closed;
        });
    }


}
