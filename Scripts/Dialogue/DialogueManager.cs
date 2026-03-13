using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager I { get; private set; }

    float blockInputUntil;

    [Header("Refs")]
    [SerializeField] DialogSystemController ui;

    [Header("Player Lock")]
    [SerializeField] MonoBehaviour playerMovementScriptToDisable;

    CustomerController currentCustomer;

    public bool IsOpen { get; private set; }

    int agreedTotalPrice;
    bool dealAccepted;

    enum TalkState
    {
        None,
        Greeting,
        PlayerResponded,
        CustomerWantsTire,
        Negotiation,
        Accepted,
        Rejected,
        JobStatus
    }

    TalkState state = TalkState.None;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        if (ui != null)
        {
            ui.OnCloseClicked += End;
            ui.OnOfferSubmitted += HandleOffer;
        }
        else
        {
            Debug.LogWarning("[DialogueManager] UI reference missing.");
        }
    }

    public bool CanUIInteract => Time.unscaledTime >= blockInputUntil;

    public void BeginWithCustomer(CustomerController customer)
    {
        if (customer == null)
        {
            Debug.LogWarning("[DialogueManager] BeginWithCustomer -> customer null");
            return;
        }

        if (ui == null)
        {
            Debug.LogWarning("[DialogueManager] BeginWithCustomer -> ui null");
            return;
        }

        IsOpen = true;
        blockInputUntil = Time.unscaledTime + 0.10f;
        currentCustomer = customer;

        // Sipariş yoksa oluştur
        if (currentCustomer.GetPendingOrder() == null)
        {
            if (EconomyManager.I != null && EconomyManager.I.TryCreateRandomOrder(out TireOrder newOrder))
            {
                currentCustomer.SetPendingOrder(newOrder);
            }
            else
            {
                Debug.LogWarning("[DialogueManager] Pending order oluşturulamadı.");
            }
        }

        LockPlayer(true);
        ui.Show();

        // Başlangıçta ekonomi panelini doldur
        TireOrder order = GetCurrentOrder();
        if (order != null)
            ui.RefreshEconomyPanel(order);

        ui.SetOfferUIVisible(false);
        ui.ClearChoices();

        if(currentCustomer.IsInServiceProcess())
        {
            OpenJobStatusDialogue();
            return;
        }

        //sipariş yoksa oluşturuyoruz
        if(currentCustomer.GetPendingOrder() == null)
        {
            if (EconomyManager.I != null && EconomyManager.I.TryCreateRandomOrder(out TireOrder newOrder))
            {
                currentCustomer.SetPendingOrder(newOrder);
            }
            else
            {
                Debug.LogWarning("[DialogueManager] Pending order oluşturulamadı.");
            }
        }

        state = TalkState.Greeting;

        ui.SetNpcLine("Selamlar kolay gelsin ustam.");
        ui.AddChoice("Sağ ol abi, nasıl yardımcı olalım?", OnPlayerPolite);
        ui.AddChoice("Eyvallah, söyle bakalım.", OnPlayerCasual);
    }

    void OnPlayerPolite()
    {
        state = TalkState.PlayerResponded;
        ui.SetOfferUIVisible(false);

        ui.SetNpcLine("Bir lastik işim var ustam.");
        ui.ClearChoices();
        ui.AddChoice("Tamam abi, hangi ebat?", GoToCustomerWantsTire);
        ui.AddChoice("Bugünlük kapalıyız.", RejectEarly);
    }

    void OnPlayerCasual()
    {
        state = TalkState.PlayerResponded;
        ui.SetOfferUIVisible(false);

        ui.SetNpcLine("Ustam 4 lastik değiştireceğiz.");
        ui.ClearChoices();
        ui.AddChoice("Tamam abi, hangi ebat?", GoToCustomerWantsTire);
        ui.AddChoice("Bugünlük kapalıyız.", RejectEarly);
    }

    void GoToCustomerWantsTire()
    {
        state = TalkState.CustomerWantsTire;
        ui.SetOfferUIVisible(false);

        TireOrder order = GetCurrentOrder();
        if (order == null)
        {
            ui.SetNpcLine("Ustam bir gariplik oldu, siparişi çıkaramadım.");
            ui.ClearChoices();
            ui.AddChoice("Kapat", End);
            return;
        }

        // Burada ekonomi paneli ve müşteri mesajı gerçek order'dan dolacak
        ui.RefreshEconomyPanel(order);

        ui.ClearChoices();
        ui.AddChoice("Pazarlığa girelim.", GoToNegotiation);
        ui.AddChoice("Piyasa olur, yapalım.", AcceptAtMarket);
    }

    void GoToNegotiation()
    {
        state = TalkState.Negotiation;

        TireOrder order = GetCurrentOrder();
        if (order == null)
        {
            ui.SetNpcLine("Sipariş bilgisi yok.");
            return;
        }

        int marketUnitPrice = EconomyManager.I.GetMarketUnitPrice(order);

        ui.SetNpcLine($"Piyasa {marketUnitPrice}. Sen kaç diyorsun?");
        ui.ClearChoices();

        ui.SetOfferUIVisible(true);
        ui.SetOfferPlaceholder("Teklif (örn: 130)");
        ui.SetOfferText("");

        ui.AddChoice("Vazgeçtim.", End);
    }

    void HandleOffer(int offer)
    {
        if (state != TalkState.Negotiation)
            return;

        TireOrder order = GetCurrentOrder();
        if (order == null)
        {
            ui.SetNpcLine("Sipariş bilgisi yok.");
            return;
        }

        int marketUnitPrice = EconomyManager.I.GetMarketUnitPrice(order);
        int acceptMin = Mathf.RoundToInt(marketUnitPrice * 0.90f);

        if (offer >= acceptMin)
        {
            state = TalkState.Accepted;
            ui.SetOfferUIVisible(false);

            ui.SetNpcLine($"Tamam ustam, {offer} olsun. Anlaştık.");
            ui.ClearChoices();

            agreedTotalPrice = (offer * order.quantity);
            dealAccepted = true;

            ui.AddChoice("Başlayalım.", ConfirmAndEnd);
            return;
        }

        state = TalkState.Rejected;
        ui.SetOfferUIVisible(true);

        int counter = Mathf.Clamp(marketUnitPrice - 5, 1, 999999);

        ui.SetNpcLine($"{offer} olmaz ustam, çok düşük. {counter} yapalım.");
        ui.ClearChoices();

        ui.AddChoice($"{counter} tamam.", () =>
        {
            state = TalkState.Accepted;
            ui.SetOfferUIVisible(false);

            agreedTotalPrice = (counter * order.quantity);
            dealAccepted = true;

            ui.SetNpcLine($"Anlaştık ustam, {counter}.");
            ui.ClearChoices();
            ui.AddChoice("Başlayalım.", ConfirmAndEnd);
        });

        ui.AddChoice("Tekrar teklif vereyim.", () =>
        {
            state = TalkState.Negotiation;
            ui.SetNpcLine($"Piyasa {marketUnitPrice}. Sen kaç diyorsun?");
        });

        ui.AddChoice("O zaman olmaz, güle güle.", ConfirmAndEnd);
    }

    void AcceptAtMarket()
    {
        TireOrder order = GetCurrentOrder();
        if (order == null)
        {
            ui.SetNpcLine("Sipariş bilgisi yok.");
            return;
        }

        int marketUnitPrice = EconomyManager.I.GetMarketUnitPrice(order);

        state = TalkState.Accepted;
        ui.SetOfferUIVisible(false);

        ui.SetNpcLine($"Tamam ustam, tanesi {marketUnitPrice}. Anlaştık.");
        ui.ClearChoices();
        ui.AddChoice("Başlayalım.", End);
    }

    void RejectEarly()
    {
        state = TalkState.Rejected;
        ui.SetOfferUIVisible(false);

        ui.SetNpcLine("Tamam ustam, sonra uğrarım.");
        ui.ClearChoices();
        ui.AddChoice("Görüşürüz.", End);
    }

    public void End()
    {
        //YANİ herşeye reset atıyoruz.
        IsOpen = false;

        if (ui != null)
            ui.Hide();

        LockPlayer(false);

        currentCustomer = null;
        state = TalkState.None;
        agreedTotalPrice = 0; // sıfırlıyoruz res
        dealAccepted = false;
    }

    void LockPlayer(bool locked)
    {
        Cursor.visible = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;

        if (playerMovementScriptToDisable != null)
            playerMovementScriptToDisable.enabled = !locked;
    }

    TireOrder GetCurrentOrder()
    {
        if (currentCustomer == null)
            return null;

        return currentCustomer.GetPendingOrder();
    }

    void ConfirmAndEnd()
    {
        if(dealAccepted && currentCustomer != null)
        {
            bool started = currentCustomer.ConfirmDealAndStartJob(agreedTotalPrice);

            if(!started)
            {
                Debug.LogWarning("[DialogueManager] deal kabul edildi ama job başlatılamadı.");
            }
        }

        End();
    }

    void OpenJobStatusDialogue()
    {
        state = TalkState.JobStatus;
        ui.SetOfferUIVisible(false);
        ui.ClearChoices();
        //Ortada müşteri yoksa bu çıkar.
        if(currentCustomer == null)
        {
            ui.SetNpcLine("Ortada müşteri yok ustam.");
            ui.AddChoice("Kapat", End);
            return;
        }
        //Suradaki adamla böyle konuşuyoz.
        if(currentCustomer.isInWaitinForBay())
        {
            ui.SetNpcLine("Ustam sıra bekliyorum , lift boşalınca alacağız değil mi ?");
            ui.AddChoice("Aynen abi, sıradasın", End);
            ui.AddChoice("İptal edelim istersen" , RejectServiceInProgress);
            return;
        }

        ui.SetNpcLine("Ne durumda araba ustam ?");
        ui.AddChoice("Abi bitti", TryFinishJob);
        ui.AddChoice("Daha bitmedi abi az kaldı", End);
    }

    void TryFinishJob()
    {
        if(currentCustomer == null)
        {
            ui.SetNpcLine("Müşteri bulunamadı");
            ui.ClearChoices();
            ui.AddChoice("Kapat", End);
            return;
        }

        TireOrder order = currentCustomer.GetPendingOrder();
        if (order == null)
        {

        ui.SetNpcLine("Sipariş bilgisi yok.");
        ui.ClearChoices();
        ui.AddChoice("Kapat", End);
        return;

        }

        bool done  = false;
        done = currentCustomer.TryCompleteJobFromDialogue();

        if (done)
        {

        ui.SetNpcLine("Eyvallah ustam, eline sağlık.");
        ui.ClearChoices();
        ui.AddChoice("Güle güle kullan abi.", End);

        }
        else
        {

        ui.SetNpcLine("Ustam iş daha tam bitmemiş gibi duruyor.");
        ui.ClearChoices();
        ui.AddChoice("Tamam abi, kontrol edeyim.", End);

        }



    }

    void RejectServiceInProgress()
    {

    ui.SetNpcLine("Tamam ustam, ben sonra geleyim.");
    ui.ClearChoices();
    ui.AddChoice("Görüşürüz.", () =>
    {
        if (currentCustomer != null)
            currentCustomer.LeaveShop();

        End();
    });
    
    }


}