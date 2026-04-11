using UnityEngine;

public class DialoguePlayTester : MonoBehaviour
{
    [SerializeField] private DialogueFlowLibrary library;
    [SerializeField] private ParsedDialoguePlayer player;

    private void Start()
    {
        var flow = library.PickRandomFlow("MICHEAL", "REFERENCED", "SUMMER");

        player.StartDialogue(flow);
    }
}