using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireIdentity : MonoBehaviour
{
    [Header("Identity")]
    public TireSize size;
    public TireSeason season;

    public TireCondition condition;

    public TireBrand brand;
    
    [Header("RunTime")]
    
    public int uniqueId;

    public string stringDisplayName => $"{brand} {size} * {season} * {condition}";

    public void Init(TireSize s, TireSeason se, TireCondition co, TireBrand br, int id=0)
    {
        size = s;
        season = se;
        condition  = co;
        brand = br;
        uniqueId = id;
    }

}
