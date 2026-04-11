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

    public void StartDialogue(ParsedDialogueFlow flow , TireOrder order = null)
    {
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
        currentNode = flow.FindNode(flow.startNodeId);

        if (currentNode == null)
        {
            Debug.LogError($"[DialoguePlayer] Start node not found: {flow.startNodeId}");
            return;
        }

        ui.Show();
        ui.ClearChoices();
        ui.SetOfferUIVisible(false);

        Debug.Log($"[DialoguePlayer] StartDialogue -> {flow.flowId}");
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

        string combined = string.Join("\n", currentNode.lines.Select(l => l.text));
        ui.SetNpcLine(combined);

        if (currentNode.kind == ParsedNodeKind.PriceInput)
        {
            ui.SetOfferUIVisible(true);
            ui.SetOfferPlaceholder("Fiyat gir...");
            ui.SetOfferText(debugOffer.ToString());

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
        int targetPrice = CalculateTargetPrice();
        bool accepted = debugOffer <= targetPrice;

        Debug.Log($"[DialoguePlayer] Submit offer={debugOffer}, target={targetPrice}, accepted={accepted}");

        string nextNodeId = accepted
            ? currentNode.id + "_ACCEPT"
            : currentNode.id + "_REJECT";

        var next = currentFlow.FindNode(nextNodeId);

        if (next == null)
        {
            Debug.LogError($"[DialoguePlayer] Price result node not found: {nextNodeId}");
            return;
        }

        currentNode = next;
        ShowCurrentNode();
    }

    private int CalculateTargetPrice()
    {
        int market = fallbackMarketPrice;

        if (currentFlow == null)
            return market;

        string type = currentFlow.customerTypeKey?.ToUpperInvariant();

        switch (type)
        {
            case "CHEAP":
                return Mathf.RoundToInt(market * 0.95f);

            case "REFERENCED":
                return Mathf.RoundToInt(market * 1.05f);

            case "PREMIUM":
                return Mathf.RoundToInt(market * 1.10f);

            case "STANDARD":
                return market;

            default:
                return market;
        }
    }
}