using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager I {get; private set;}
    float blockInputUntil;

    [Header("Refs")]
    [SerializeField] DialogueUIController ui;

    [Header("Player Lock")]
    [SerializeField] MonoBehaviour playerMovementScriptToDisable;

    CustomerController currentCustomer;

    public bool IsOpen{ get; private set;}

    enum TalkState
    {
        None,
        Greeting,
        PlayerResponded,
        CustomerWantsTire,
        Negotiation,
        Accepted,
        Rejected
    }

    TalkState state = TalkState.None;

    string wantedFake = "215/40R17";
    int stockFake = 8;
    int marketFake = 150;

    void Awake()
    {
        if( I != null && I != this ) { Destroy(gameObject); return ;}
        I = this;

        if(ui)
        {
            ui.OnCloseClicked += End;
            ui.OnOfferSubmitted += HandleOffer;
        }
    }
    public bool CanUIInteract => Time.unscaledTime >= blockInputUntil;

    public void BeginWithCustomer(CustomerController customer)
    {
        IsOpen = true;
        blockInputUntil = Time.unscaledTime + 0.10f;
        if(ui == null)
        {
            Debug.LogWarning("[DialogueManager] UI ref missing!");
            return;
        }

        currentCustomer = customer;
        state = TalkState.Greeting;

        LockPlayer(true);
        ui.Show();

        ui.SetOfferUIVisible(false);
        ui.SetInfo("-","-","-");

        ui.SetNpcLine("Selamlar kolay gelsin ustam.");
        ui.ClearChoices();
        ui.AddChoice("Sağ ol abi, nasil yardimci olalim?", OnPlayerPolite);
        ui.AddChoice("Eyvallah, söyle bakalim", OnPlayerCasual);

    }

    void OnPlayerPolite()
    {
        state = TalkState.PlayerResponded;
        ui.SetOfferUIVisible(false);

        ui.SetNpcLine("Bir lastik işim var ustam");
        ui.ClearChoices();

        ui.AddChoice("Tamam abi , hangi ebat?", GoToCustomerWantsTire);
        ui.AddChoice("Bugünlük kapaliyiz", RejectEarly);        
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

        ui.SetNpcLine($"{wantedFake} istiyorum . 4 adet");
        ui.SetInfo(wantedFake, stockFake.ToString(), marketFake.ToString());

        ui.ClearChoices();
        ui.AddChoice("Pazarlığa girelim.", GoToNegotiation);
        ui.AddChoice("Piyasa olur, yapalım.", AutoAcceptAtMarket);
        ui.AddChoice("Yok abi elimizde bu ebat", RejectNoStockFake);
    }

    void GoToNegotiation()
    {
        state = TalkState.Negotiation;

        ui.SetNpcLine($"Piyasa {marketFake}. Sen kaç diyorsun?");
        ui.ClearChoices();

        ui.SetOfferUIVisible(true);
        ui.SetOfferPlaceholder("Teklif (örn: 130)");
        ui.SetOfferText(""); // input temizlensin

        ui.AddChoice("Vazgeçtim.", End);
    }

    void HandleOffer(int offer)
    {
        if(state != TalkState.Negotiation) return;

        //market =150 , kabul min = 135
        int acceptMin = Mathf.RoundToInt(marketFake * 0.90f);

        if(offer >= acceptMin)
        {
            state = TalkState.Accepted;
            ui.SetOfferUIVisible(false);

            ui.SetNpcLine($"Tamam ustam , {offer} olsun . Anlaştık");
            ui.ClearChoices();
            ui.AddChoice("Başlayalım", End);
            return;
        }

        state = TalkState.Rejected;
        ui.SetOfferUIVisible(true);

        int counter = Mathf.Clamp(marketFake - 5 , 1, 999999);
        ui.SetNpcLine($"{offer} olmaz ustam, çok düşük. {counter} yapalım.");

        ui.ClearChoices();
        ui.AddChoice($"{counter} tamam.", () =>
        {
            // müşteri teklifini kabul ediyormuş gibi davran
            state = TalkState.Accepted;
            ui.SetOfferUIVisible(false);

            ui.SetNpcLine($"Anlaştık ustam, {counter}.");
            ui.ClearChoices();
            ui.AddChoice("Başlayalım.", End);
        });

        ui.AddChoice("Tekrar teklif vereyim.", () =>
        {
            state = TalkState.Negotiation;
            ui.SetNpcLine($"Piyasa {marketFake}. Sen kaç diyorsun?");
            // input açık kalsın, kullanıcı yeni sayı girsin
        });

        ui.AddChoice("O zaman olmaz, güle güle.", End);
    }

    void AutoAcceptAtMarket()
    {
        state = TalkState.Accepted;
        ui.SetOfferUIVisible(false);

        ui.SetNpcLine("Tamam ustam, piyasa fiyatından verelim. Anlaştık.");
        ui.ClearChoices();
        ui.AddChoice("Başlayalım.", End);
    }

    void RejectNoStockFake()
    {
        state = TalkState.Rejected;
        ui.SetOfferUIVisible(false);

        ui.SetNpcLine("Hadi ya… o zaman başka yere gideyim.");
        ui.ClearChoices();
        ui.AddChoice("Kusura bakma abi.", End);
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
        if (ui) ui.Hide();
        LockPlayer(false);

        currentCustomer = null;
        state = TalkState.None;

        IsOpen = false;
        ui.Hide();
    }

    void LockPlayer(bool locked)
    {
        Cursor.visible = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;

        if (playerMovementScriptToDisable != null)
            playerMovementScriptToDisable.enabled = !locked;
    }
}