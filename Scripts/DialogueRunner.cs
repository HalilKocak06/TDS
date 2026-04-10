using System.Linq;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DialogSystemController dialogUI;
    [SerializeField] private DialogueGraphSO graph;

    [Header("Test Pricing")]
    [SerializeField] private int marketPrice = 150;
    [SerializeField] private int maxMarkupPercent = 10;

    private DialogueNodeData currentNode;
    private int rejectCount = 0;
    private const int maxRejectCount = 2;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("F6 Basıldı");
            StartDialogue();
        }
    }

    private void OnEnable()
    {
        if (dialogUI != null)
        {
            dialogUI.OnOfferSubmitted += HandleOfferSubmitted;
            dialogUI.OnCloseClicked += HandleCloseClicked;
        }
    }

    private void OnDisable()
    {
        if (dialogUI != null)
        {
            dialogUI.OnOfferSubmitted -= HandleOfferSubmitted;
            dialogUI.OnCloseClicked -= HandleCloseClicked;
        }
    }

    public void StartDialogue()
    {
        if (graph == null || dialogUI == null)
        {
            Debug.LogWarning("[DialogueRunner] Graph veya UI eksik.");
            return;
        }

        rejectCount = 0;
        currentNode = FindNode(graph.startNodeId);

        if (currentNode == null)
        {
            Debug.LogError("[DialogueRunner] Start node bulunamadı: " + graph.startNodeId);
            return;
        }

        dialogUI.Show();
        ShowCurrentNode();
    }

    private DialogueNodeData FindNode(string id)
    {
        return graph.nodes.FirstOrDefault(n => n.id == id);
    }

    private void ShowCurrentNode()
    {
        if (currentNode == null)
            return;

        dialogUI.ClearChoices();
        dialogUI.SetOfferUIVisible(false);
        dialogUI.SetNpcLine(currentNode.npcText);

        if (currentNode.isPriceNode)
        {
            dialogUI.SetOfferUIVisible(true);
            dialogUI.SetOfferText("");
            dialogUI.SetOfferPlaceholder("Fiyat gir...");
            return;
        }

        if (currentNode.choices != null)
        {
            foreach (var choice in currentNode.choices)
            {
                string nextId = choice.nextNodeId;
                dialogUI.AddChoice(choice.text, () => GoToNode(nextId));
            }
        }
    }

    private void GoToNode(string nodeId)
    {
        DialogueNodeData nextNode = FindNode(nodeId);

        if (nextNode == null)
        {
            Debug.LogError("[DialogueRunner] Node bulunamadı: " + nodeId);
            return;
        }

        currentNode = nextNode;
        ShowCurrentNode();
    }

    private void HandleOfferSubmitted(int offeredPrice)
    {
        int acceptedMax = Mathf.RoundToInt(marketPrice * (1f + maxMarkupPercent / 100f));

        if (offeredPrice <= acceptedMax)
        {
            rejectCount = 0;
            GoToNode("MP_F07_ACCEPTED");
            return;
        }

        rejectCount++;

        if (rejectCount >= maxRejectCount)
            GoToNode("MP_F07_FAIL");
        else
            GoToNode("MP_F07_REJECT");
    }

    private void HandleCloseClicked()
    {
        Debug.Log("[DialogueRunner] Dialog kapatıldı.");
    }
}