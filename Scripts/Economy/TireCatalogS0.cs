using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bu kod çok hassas bir RNG işlemi sunar lastik ebat seçimleriyle . 


[Serializable]
public class TireSizeEntry
{
    public int width;
    public int aspect;
    public int rim;

    //demandWeight: yıldız sisteminden gelen final ağırlık

    [Min(1)] public int demandWeight = 1;
    
    public string Code => $"{width}{aspect}{rim}";
}

[CreateAssetMenu(menuName = "TDS/Economy/Tire Catalog", fileName ="TireCatalog")]

public class TireCatalogS0 : ScriptableObject
{
    public List<TireSizeEntry> sizes = new List<TireSizeEntry>();

    public TireSizeEntry PickWeighted(System.Random rng)
    {
        if(sizes == null || sizes.Count == 0) return null;

        int total = 0;
        for(int i=0; i< sizes.Count; i++)
            total += Mathf.Max(1, sizes[i].demandWeight);

        int roll = rng.Next(0, total);
        int acc = 0;

        for(int i=0; i < sizes.Count; i++)
        {
            acc += Mathf.Max(1, sizes[i].demandWeight);
            if(roll< acc) return sizes[i];
        }
        return sizes[sizes.Count - 1];    
    }
}
