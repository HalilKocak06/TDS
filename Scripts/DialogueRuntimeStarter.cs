using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueRuntimeStarter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DialogueFlowLibrary library;
    [SerializeField] private ParsedDialoguePlayer player;

    [Header("Debug Start")]
    [SerializeField] private bool startOnPlay = true;

    [Header("Debug Order")]
    [SerializeField] private int width = 215;
    [SerializeField] private int aspect = 55;
    [SerializeField] private int rim = 17;
    [SerializeField] private TireBrand brand = TireBrand.Micheal;
    [SerializeField] private TireSeason season = TireSeason.Summer;
    [SerializeField] private TireCondition condition = TireCondition.New;
    [SerializeField] private int quantity = 4;


    private void Start()
    {
        if(!startOnPlay)
            return;

        StartRuntimeDialogue();    
    }

    [ContextMenu("Start RuntimeDialogue")]
    public void StartRuntimeDialogue()
    {
        if(library == null)
        {
            Debug.LogWarning("[DialogueRuntimeStarter] library null");
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("[DialogueRuntimeStarter] player null");
            return;
        }

        var order = new TireOrder
        {
            size = new TireSize(width, aspect, rim),
            brand = brand,
            season = season,
            condition = condition,
            quantity = quantity
        };

        CustomerType pickedType = CustomerTypePicker.PickRandom();

        Debug.Log($"[DialogueRuntimeStarter] Order = {order.brand} {order.size.width}/{order.size.aspect}R{order.size.rim} {order.season} x{order.quantity}");
        Debug.Log($"[DialogueRuntimeStarter] Picked CustomerType = {pickedType}");

        ParsedDialogueFlow flow = library.PickRandomFlow(order, pickedType);

        if (flow == null)
        {
            Debug.LogWarning("[DialogueRuntimeStarter] No flow found for this order + customerType");
            return;
        }

        Debug.Log($"[DialogueRuntimeStarter] Picked Flow = {flow.flowId}");

        player.StartDialogue(flow, order);
    }
}
