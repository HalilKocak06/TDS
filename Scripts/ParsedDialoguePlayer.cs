using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParsedDialoguePlayer : MonoBehaviour
{
    [SerializeField] private DialogSystemController ui;

    [Header("Debug Pricing")]
    [SerializeField] private int debugOffer = 1300;
    [SerializeField] private int debugStep = 25;
    [SerializeField] private int fallbackMarketPrice = 1250;

    private ParsedDialogueFlow currentFlow;
    private ParsedDialogueNode currentNode;
    private TireOrder runtimeOrder;
    private CustomerProfileSO runtimeProfile;
    private int offerTurnIndex = 0;

    private readonly List<DialogueChoiceDataEx> visibleChoices = new();

    private void Update()
    {
        if (currentFlow == null || currentNode == null)
            return;

        // Price node ise choice değil fiyat kontrolü yap
        if (currentNode.kind == ParsedNodeKind.PriceInput)
        {
            HandleDebugPriceInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            TrySelectChoiceByIndex(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            TrySelectChoiceByIndex(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            TrySelectChoiceByIndex(2);
    }

    public void StartDialogue(ParsedDialogueFlow flow , TireOrder order = null , CustomerProfileSO profile = null)
    {
        Debug.Log("[DialoguePlayer] A StartDialogue ENTER");

        if (flow == null)
        {
            Debug.LogError("[DialoguePlayer] flow null");
            return;
        }

        if (ui == null)
        {
            Debug.LogError("[DialoguePlayer] ui null");
            return;
        }

        currentFlow = flow;
        runtimeOrder = order;
        runtimeProfile = profile;
        offerTurnIndex = 0;


        currentNode = flow.FindNode(flow.startNodeId);

        if (currentNode == null)
        {
            Debug.LogError($"[DialoguePlayer] Start node not found: {flow.startNodeId}");
            return;
        }

        Debug.Log("[DialoguePlayer] B before ui.Show()");
        ui.Show();
        Debug.Log("[DialoguePlayer] C after ui.Show()");
        ui.ClearChoices();
        ui.SetOfferUIVisible(false);

        Debug.Log($"[DialoguePlayer] StartDialogue -> {flow.flowId}");

        if (runtimeOrder != null)
        ui.RefreshEconomyPanel(runtimeOrder);
        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        if (currentNode == null)
        {
            Debug.LogError("[DialoguePlayer] currentNode null");
            return;
        }

        Debug.Log($"[DialoguePlayer] Showing node: {currentNode.id}");

        visibleChoices.Clear();
        ui.ClearChoices();

        string combined = string.Join("\n", currentNode.lines.Select(l => ResolveRuntimeText(l.text))); //Ebatı çözmeye çalışır.
        ui.SetNpcLine(combined);

        if (currentNode.kind == ParsedNodeKind.PriceInput)
        {
            ui.SetOfferUIVisible(true);
            ui.SetOfferPlaceholder("Fiyat gir...");
            ui.SetOfferText(debugOffer.ToString());

            int market = GetMarketUnitPrice();

            Debug.Log($"[DialoguePlayer] Price node. Q/E ile değiştir, Enter ile gönder. Current={debugOffer}");
        }
        else
        {
            ui.SetOfferUIVisible(false);
        }

        if (currentNode.isTerminal)
        {
            Debug.Log("[DialoguePlayer] Dialogue End");
            return;
        }

        foreach (var choice in currentNode.choices)
        {
            var captured = choice;
            visibleChoices.Add(captured);

            ui.AddChoice(choice.text, () =>
            {
                OnChoiceSelected(captured);
            });
        }

        if (visibleChoices.Count > 0)
        {
            Debug.Log($"[DialoguePlayer] Visible choices: {visibleChoices.Count} | 1/2/3 ile seçebilirsin.");
        }
    }

    private void TrySelectChoiceByIndex(int index)
    {
        if (visibleChoices.Count == 0)
        {
            Debug.Log("[DialoguePlayer] No visible choices.");
            return;
        }

        if (index < 0 || index >= visibleChoices.Count)
        {
            Debug.Log($"[DialoguePlayer] Choice index out of range: {index}");
            return;
        }

        var choice = visibleChoices[index];
        Debug.Log($"[DialoguePlayer] Keyboard selected choice {index + 1}: {choice.text}");

        OnChoiceSelected(choice);
    }

    private void OnChoiceSelected(DialogueChoiceDataEx choice)
    {
        if (choice == null)
        {
            Debug.LogError("[DialoguePlayer] choice null");
            return;
        }

        Debug.Log($"[DialoguePlayer] Choice selected -> {choice.text} | next={choice.nextNodeId}");

        var next = currentFlow.FindNode(choice.nextNodeId);

        if (next == null)
        {
            Debug.LogError($"[DialoguePlayer] Node not found: {choice.nextNodeId}");
            return;
        }

        currentNode = next;
        ShowCurrentNode();
    }

    private void HandleDebugPriceInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            debugOffer = Mathf.Max(0, debugOffer - debugStep);
            ui.SetOfferText(debugOffer.ToString());
            Debug.Log($"[DialoguePlayer] Debug offer decreased -> {debugOffer}");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            debugOffer += debugStep;
            ui.SetOfferText(debugOffer.ToString());
            Debug.Log($"[DialoguePlayer] Debug offer increased -> {debugOffer}");
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitDebugOffer();
        }
    }

    private void SubmitDebugOffer()
    {
        int marketUnitPrice = GetMarketUnitPrice();

        bool accepted;
        string evalMessage = "";

        if(EconomyManager.I != null && runtimeProfile != null)
        {
            var eval = EconomyManager.I.EvaluateOffer(runtimeProfile, debugOffer, marketUnitPrice, offerTurnIndex);
            offerTurnIndex++;
            accepted = eval.accepted;
            evalMessage = eval.message;

            Debug.Log($"[DialoguePlayer] Eval -> accepted={eval.accepted}, rejected={eval.rejected}, counter={eval.counter}, msg={eval.message}");
        }
        else
        {
            accepted = debugOffer <= marketUnitPrice;
            offerTurnIndex++;
            evalMessage = accepted ? "Tamam, anlaştik." : "Olmadi.";
        }

        string nextNodeId = accepted
            ? currentNode.id + "_ACCEPT"
            : currentNode.id + "_REJECT";

        var next = currentFlow.FindNode(nextNodeId);

        if (next == null)
        {
            Debug.LogError($"[DialoguePlayer] Price result node not found: {nextNodeId}");
            return;
        }

        Debug.Log($"[DialoguePlayer] Submit offer={debugOffer}, market={marketUnitPrice}, next={nextNodeId}, msg={evalMessage}");

        currentNode = next;
        ShowCurrentNode();
    }

    private int GetMarketUnitPrice()
    {
        if(runtimeOrder != null && EconomyManager.I != null)
            return EconomyManager.I.GetMarketUnitPrice(runtimeOrder);

        return fallbackMarketPrice;    
    }

    private string ResolveRuntimeText(string raw) //RUntime'da ebatı yazdırmaya sağlar.
    {
        if(string.IsNullOrEmpty(raw))
            return raw;

        if(runtimeOrder == null)
        {
            return raw;
        }

        string result = raw;
        result = result.Replace("{ORDER_SIZE}", $"{runtimeOrder.size.width}/{runtimeOrder.size.aspect} R{runtimeOrder.size.rim}");
        result = result.Replace("{ORDER_BRAND}", runtimeOrder.brand.ToString());
        result = result.Replace("{ORDER_SEASON_TEXT}", SeasonToText(runtimeOrder.season));
        result = result.Replace("{ORDER_QUANTITY}", runtimeOrder.quantity.ToString());
        result = result.Replace("{ORDER_QUANTITY_TEXT}", QuantityToText(runtimeOrder.quantity));

        return result;

    }



    private string SeasonToText(TireSeason season)
    {
        switch (season)
        {
            case TireSeason.Summer: return "yazlık";
            case TireSeason.Winter: return "kışlık";
            case TireSeason.FourSeason: return "4 mevsim";
            default: return season.ToString();
        }
    }

    private string QuantityToText(int qty)
    {
        switch (qty)
        {
            case 1: return "bir";
            case 2: return "iki";
            case 3: return "üç";
            case 4: return "dört";
            default: return qty.ToString();
        }
    }
}