using UnityEngine;

public class DialogueLibraryTester : MonoBehaviour
{
    [SerializeField] private DialogueFlowLibrary library;

    [Header("Test Filter")]
    [SerializeField] private string brandKey = "MICHEAL";
    [SerializeField] private string customerTypeKey = "REFERENCED";
    [SerializeField] private string seasonKey = "SUMMER";

    private void Start()
    {
        if (library == null)
        {
            Debug.LogWarning("[DialogueLibraryTester] library null");
            return;
        }

        var flow = library.PickRandomFlow(brandKey, customerTypeKey, seasonKey);

        if (flow == null)
        {
            Debug.LogWarning("[DialogueLibraryTester] flow not found");
            return;
        }

        Debug.Log($"[DialogueLibraryTester] Picked flow = {flow.flowId}");
        Debug.Log($"[DialogueLibraryTester] Start node = {flow.startNodeId}");
    }
}