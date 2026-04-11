using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueFlowLibrary : MonoBehaviour
{
    [Header("Dialogue Groups")]
    [SerializeField] private List<DialogueFileGroup> dialogueGroups = new();

    [Header("Debug")]
    [SerializeField] private bool parseOnAwake = true;

    private readonly List<ParsedDialogueFlow> allFlows = new();

    public IReadOnlyList<ParsedDialogueFlow> AllFlows => allFlows;

    private void Awake()
    {
        if (parseOnAwake)
            RebuildLibrary();
    }

    [ContextMenu("Rebuild Library")]
    public void RebuildLibrary()
    {
        allFlows.Clear();

        Debug.Log($"[DialogueFlowLibrary] Group count = {dialogueGroups.Count}");

        foreach (var group in dialogueGroups)
        {
            if (group == null)
                continue;

            Debug.Log($"[DialogueFlowLibrary] Group = {group.groupName}, FileCount = {group.files.Count}");

            foreach (var file in group.files)
            {
                if (file == null)
                {
                    Debug.LogWarning($"[DialogueFlowLibrary] Null file in group {group.groupName}");
                    continue;
                }

                Debug.Log($"[DialogueFlowLibrary] Parsing file = {file.name}");

                var flows = DialogueTextParser.ParseAllFlows(file.text);

                Debug.Log($"[DialogueFlowLibrary] Parsed {flows.Count} flow(s) from {file.name}");

                allFlows.AddRange(flows);
            }
        }

        Debug.Log($"[DialogueFlowLibrary] Total flow count = {allFlows.Count}");
    }

    public ParsedDialogueFlow PickRandomFlow(string brandKey, string customerTypeKey, string seasonKey)
    {
        var matches = allFlows.Where(f =>
            string.Equals(f.brandKey, brandKey, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(f.customerTypeKey, customerTypeKey, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(f.seasonKey, seasonKey, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (matches.Count == 0)
        {
            Debug.LogWarning($"[DialogueFlowLibrary] No flow match for brand={brandKey}, customerType={customerTypeKey}, season={seasonKey}");
            return null;
        }

        int index = UnityEngine.Random.Range(0, matches.Count);
        return matches[index];
    }

    public ParsedDialogueFlow PickRandomFlow(TireOrder order, CustomerType customerType)
    {
        if (order == null)
        {
            Debug.LogWarning("[DialogueFlowLibrary] PickRandomFlow(order, type) -> order null");
            return null;
        }

        string brandKey = BrandToKey(order.brand);
        string customerTypeKey = CustomerTypeToKey(customerType);
        string seasonKey = SeasonToKey(order.season);

        Debug.Log($"[DialogueFlowLibrary] Order-based pick -> brand={brandKey}, customerType={customerTypeKey}, season={seasonKey}");

        return PickRandomFlow(brandKey, customerTypeKey, seasonKey);
    }

    private string BrandToKey(TireBrand brand)
    {
        return brand.ToString().ToUpperInvariant();
    }

    private string SeasonToKey(TireSeason season)
    {
        switch (season)
        {
            case TireSeason.Summer: return "SUMMER";
            case TireSeason.Winter: return "WINTER";
            case TireSeason.FourSeason: return "ALLSEASON";
            default: return season.ToString().ToUpperInvariant();
        }
    }

    private string CustomerTypeToKey(CustomerType type)
    {
        switch (type)
        {
            case CustomerType.Premium: return "PREMIUM";
            case CustomerType.Standard: return "STANDARD";
            case CustomerType.Cheap: return "CHEAP";
            case CustomerType.Referral: return "REFERENCED";
            case CustomerType.PriceOnly: return "PRICEONLY";
            default: return type.ToString().ToUpperInvariant();
        }
    }
}