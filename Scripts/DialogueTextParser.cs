using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DialogueTextParser
{
    private static readonly Regex NodeHeaderRegex =
        new(@"^\[(.+?)\]$", RegexOptions.Compiled);

    private static readonly Regex ChoiceRegex =
        new(@"^([A-Z])\s*=\s*(.+?)\s*->\s*(.+)$", RegexOptions.Compiled);

    private static readonly Regex FlowMetaRegex =
        new(@"^(?<brand>[A-Z]+)_(?<ctype>[A-Z]+)_(?<season>[A-Z]+)_FLOW(?<num>\d+)$", RegexOptions.Compiled);

    public static List<ParsedDialogueFlow> ParseAllFlows(string rawText)
    {
        var flows = new List<ParsedDialogueFlow>();

        if (string.IsNullOrWhiteSpace(rawText))
            return flows;

        string normalized = rawText.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] lines = normalized.Split('\n');

        ParsedDialogueFlow currentFlow = null;
        ParsedDialogueNode currentNode = null;

        bool inChoices = false;
        bool inSystem = false;
        bool inCondition = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string raw = lines[i];
            string line = raw.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            bool looksLikeFlowHeader =
                !line.StartsWith("[") &&
                !line.StartsWith("Müşteri:") &&
                !line.StartsWith("Sistem:") &&
                !line.StartsWith("Koşul:") &&
                !line.StartsWith("Seçenekler:") &&
                !line.Contains(" = ") &&
                line == line.ToUpperInvariant();

            if (looksLikeFlowHeader)
            {
                currentFlow = new ParsedDialogueFlow
                {
                    flowId = line
                };

                FillFlowMetadata(currentFlow);

                flows.Add(currentFlow);
                currentNode = null;
                inChoices = false;
                inSystem = false;
                inCondition = false;
                continue;
            }

            Match nodeMatch = NodeHeaderRegex.Match(line);
            if (nodeMatch.Success)
            {
                if (currentFlow == null)
                {
                    Debug.LogWarning($"[Parser] Node bulundu ama flow yok: {line}");
                    continue;
                }

                string nodeId = nodeMatch.Groups[1].Value.Trim();

                if (currentFlow.FindNode(nodeId) != null)
                {
                    Debug.LogWarning($"[Parser] Duplicate node id in flow '{currentFlow.flowId}': {nodeId}");
                }

                currentNode = new ParsedDialogueNode
                {
                    id = nodeId
                };

                if (nodeId.EndsWith("_START", StringComparison.OrdinalIgnoreCase))
                    currentFlow.startNodeId = nodeId;

                if (nodeId.Contains("PRICE_", StringComparison.OrdinalIgnoreCase))
                    currentNode.kind = ParsedNodeKind.PriceInput;

                if (nodeId.Contains("_NODE_SERVICE", StringComparison.OrdinalIgnoreCase))
                    currentNode.kind = ParsedNodeKind.Service;

                if (nodeId.Contains("_END_SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    currentNode.kind = ParsedNodeKind.Terminal;
                    currentNode.isTerminal = true;
                    currentNode.isSuccessEnd = true;
                }

                if (nodeId.Contains("_END_FAIL", StringComparison.OrdinalIgnoreCase))
                {
                    currentNode.kind = ParsedNodeKind.Terminal;
                    currentNode.isTerminal = true;
                    currentNode.isFailEnd = true;
                }

                currentFlow.nodes.Add(currentNode);

                inChoices = false;
                inSystem = false;
                inCondition = false;
                continue;
            }

            if (currentNode == null)
                continue;

            if (line.Equals("Seçenekler:", StringComparison.OrdinalIgnoreCase))
            {
                inChoices = true;
                inSystem = false;
                inCondition = false;
                continue;
            }

            if (line.Equals("Sistem:", StringComparison.OrdinalIgnoreCase))
            {
                inChoices = false;
                inSystem = true;
                inCondition = false;
                continue;
            }

            if (line.Equals("Koşul:", StringComparison.OrdinalIgnoreCase))
            {
                inChoices = false;
                inSystem = false;
                inCondition = true;
                continue;
            }

            if (line.StartsWith("Müşteri:", StringComparison.OrdinalIgnoreCase))
            {
                inChoices = false;
                inSystem = false;
                inCondition = false;

                string text = line.Substring("Müşteri:".Length).Trim();
                bool hasRuntimeOrder = text.Contains("--RUNTIME ORDER", StringComparison.OrdinalIgnoreCase);
                text = text.Replace("--RUNTIME ORDER", "").Trim();

                currentNode.lines.Add(new DialogueLineData
                {
                    speaker = DialogueSpeaker.Customer,
                    text = text,
                    hasRuntimeOrderTag = hasRuntimeOrder
                });

                continue;
            }

            if (inChoices)
            {
                Match choiceMatch = ChoiceRegex.Match(line);
                if (choiceMatch.Success)
                {
                    currentNode.choices.Add(new DialogueChoiceDataEx
                    {
                        key = choiceMatch.Groups[1].Value.Trim(),
                        text = choiceMatch.Groups[2].Value.Trim(),
                        nextNodeId = choiceMatch.Groups[3].Value.Trim()
                    });
                }
                else
                {
                    Debug.LogWarning($"[Parser] Choice parse edilemedi: {line}");
                }

                continue;
            }

            if (inSystem)
            {
                string sys = line.StartsWith("-") ? line.Substring(1).Trim() : line;
                currentNode.systemNotes.Add(sys);

                if (ContainsPriceInputHint(sys))
                    currentNode.kind = ParsedNodeKind.PriceInput;

                continue;
            }

            if (inCondition)
            {
                string cond = line.StartsWith("-") ? line.Substring(1).Trim() : line;
                currentNode.conditions.Add(cond);
                continue;
            }
        }

        return flows;
    }

    private static void FillFlowMetadata(ParsedDialogueFlow flow)
    {
        if (flow == null || string.IsNullOrWhiteSpace(flow.flowId))
            return;

        Match m = FlowMetaRegex.Match(flow.flowId);
        if (!m.Success)
        {
            Debug.LogWarning($"[Parser] Flow metadata parse edilemedi: {flow.flowId}");
            return;
        }

        flow.brandKey = m.Groups["brand"].Value.Trim();
        flow.customerTypeKey = m.Groups["ctype"].Value.Trim();
        flow.seasonKey = m.Groups["season"].Value.Trim();

        if (int.TryParse(m.Groups["num"].Value, out int num))
            flow.flowNumber = num;
    }

    private static bool ContainsPriceInputHint(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string t = text.ToLowerInvariant();

        return t.Contains("oyuncu 1. fiyatı girer") ||
               t.Contains("oyuncu 2. fiyatı girer") ||
               t.Contains("oyuncu 3. ve son fiyatı girer") ||
               t.Contains("oyuncu fiyatı girer");
    }
}