using UnityEngine;

public class DialogueOrderFlowTester : MonoBehaviour
{
    [SerializeField] private DialogueFlowLibrary library;

    [Header("Test Order")]
    [SerializeField] private TireBrand brand = TireBrand.Micheal;
    [SerializeField] private TireSeason season = TireSeason.Summer;
    [SerializeField] private CustomerType customerType = CustomerType.Cheap;

    private void Start()
    {
        if (library == null)
        {
            Debug.LogWarning("[DialogueOrderFlowTester] library null");
            return;
        }

        var order = new TireOrder
        {
            size = new TireSize(215, 55, 17),
            brand = brand,
            season = season,
            condition = TireCondition.New,
            quantity = 4
        };

        var flow = library.PickRandomFlow(order, customerType);

        if (flow == null)
        {
            Debug.LogWarning("[DialogueOrderFlowTester] flow null");
            return;
        }

        Debug.Log($"[DialogueOrderFlowTester] Picked flow = {flow.flowId}");
        Debug.Log($"[DialogueOrderFlowTester] Start node = {flow.startNodeId}");
    }
}