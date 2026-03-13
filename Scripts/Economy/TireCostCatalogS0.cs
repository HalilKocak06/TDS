using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


[Serializable]
public class TireCostEntry
{
    public int width;
    public int aspect;
    public int rim;

    public TireSeason season;
    public TireCondition condition;
    public TireBrand brand;

    [Min(0)]
    public int unitCost;
    
    public TireKey ToKey()
    {
        return new TireKey(
            width,
            aspect,
            rim,
            season,
            condition,
            brand
        );
    }
}

[CreateAssetMenu(menuName ="TDS/Economy/Tire Cost Catalog" , fileName ="TİreCostCatalog")]
public class TireCostCatalogS0 : ScriptableObject
{
    public List<TireCostEntry> entries = new List<TireCostEntry>();

    Dictionary<TireKey, int> lookup;

    public void RebuildLookup()
    {
        lookup = new Dictionary<TireKey, int>();

        if(entries == null)
            return;
        for(int i = 0; i< entries.Count; i++)
        {
            TireCostEntry e = entries[i];
            if (e == null)
                continue;
            TireKey key = e.ToKey();

            if(lookup.ContainsKey(key))
            {
                Debug.LogWarning($"[TireCostCatalog] Duplicate key found, overwriting: {key}");
            }    
            lookup[key] = e.unitCost;
        }    
    }

    public bool TryGetCost(TireKey key , out int cost)
    {
        if(lookup == null)
            RebuildLookup();

        return lookup.TryGetValue(key, out cost);    
    }

    public int GetCostOrDefault(TireKey key, int fallbackCost =0)
    {
        if(TryGetCost(key, out int cost))
            return cost;

        return fallbackCost;    
    }

    private void OnEnable()
    {
        RebuildLookup();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildLookup();
    }
#endif


}
