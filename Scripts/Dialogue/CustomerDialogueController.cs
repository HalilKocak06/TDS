using UnityEngine;

public class CustomerDialogueController : MonoBehaviour
{
    [SerializeField] DialogueUIBridge ui;
    [SerializeField] CustomerProfileSO profile;

    DialogueSession session;

    // dışarıdan müşteri order’ı geliyor varsayalım
    public void BeginDialogue(TireOrder order)
    {
        session = new DialogueSession(
            order,
            profile,
            EconomyManager.I,
            getMonthIndex: () => 0 // şimdilik 0, sonra TimeSystem’den ay al
        );

        // bind
        session.OnNpcText += (t) => ui.SetNPCText(t);
        session.OnChoices += (choices, cb) => ui.SetChoices(choices, cb);
        session.OnOfferInput += (cb) => ui.RequestOffer(cb);
        session.OnCloseUI += () => ui.Hide();

        session.OnDealAccepted += (total) =>
        {
            // burada: aracı lifte yolla, TireJobManager’a iş aç
            // ödeme: iş bitince EconomyManager.I.PayForCompletedJobByDeal(total, "deal");
            Debug.Log($"[Deal] Accepted total={total}");
        };

        session.OnDealRejected += () =>
        {
            Debug.Log("[Deal] Rejected");
        };

        ui.Show();
        session.Start();
    }
}