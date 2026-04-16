using UnityEngine;

public class DialogueRuntimeService : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DialogueFlowLibrary flowLibrary;
    [SerializeField] private CustomerProfileLibrary profileLibrary;
    [SerializeField] private ParsedDialoguePlayer dialoguePlayer;

    public CustomerDialogueContext BuildContext(TireOrder order)
    {
        if(order == null)
        {
            Debug.LogWarning("[DialogueRuntimeService] order null");
            return null;
        }

        CustomerType pickedType = CustomerTypePicker.PickRandom();
        CustomerProfileSO profile = profileLibrary.GetProfile(pickedType);
        ParsedDialogueFlow flow = flowLibrary.PickRandomFlow(order, pickedType);

        if(flow == null)
        {
            Debug.LogWarning("[DialogueRuntimeService] flow null");
            return null;
        }

        var context = new CustomerDialogueContext
        {
            order = order,
            customerType = pickedType,
            profile = profile,
            flow = flow
        };

        Debug.Log($"[DialogueRuntimeService] Context built -> type={pickedType}, profile={(profile != null ? profile.name : "null")}, flow={flow.flowId}");
        return context;
    }

    public void StartDialogue(CustomerDialogueContext context)
    {
        if(context == null)
        {
            Debug.LogWarning("[DialogueRuntimeService] context null");
            return;
        }

        dialoguePlayer.StartDialogue(context.flow, context.order, context.profile);
    }
    
}
