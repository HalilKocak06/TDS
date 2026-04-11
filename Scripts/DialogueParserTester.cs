using System.Collections.Generic;
using UnityEngine;

public class DialogueParserTester : MonoBehaviour
{
    [SerializeField] private TextAsset sourceText;

    private void Start()
    {
        ParseNow();
    }

    public void ParseNow()
    {
        if (sourceText == null)
        {
            Debug.LogWarning("[ParserTester] sourceText null");
            return;
        }

        List<ParsedDialogueFlow> flows = DialogueTextParser.ParseAllFlows(sourceText.text);

        Debug.Log($"[ParserTester] Flow count = {flows.Count}");

        foreach (var flow in flows)
        {
            Debug.Log($"[ParserTester] Flow={flow.flowId}, Start={flow.startNodeId}, NodeCount={flow.nodes.Count}");

            foreach (var node in flow.nodes)
            {
                Debug.Log(
                    $"  Node={node.id} | lines={node.lines.Count} | choices={node.choices.Count} | " +
                    $"system={node.systemNotes.Count} | cond={node.conditions.Count} | kind={node.kind}"
                );
            }
        }
    }
}