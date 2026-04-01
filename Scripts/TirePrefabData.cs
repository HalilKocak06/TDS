using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TirePrefabData
{
    [Header("Kimlik/debug")]
    public string prefabId;

    [Header("Sipariş eşleşmesi")]
    public TireBrand brand;
    public TireSeason season;

    [Header("Spawn edilecek prefab")]
    public GameObject prefab;

    //Verilen brand + season bu entry ile eşleşiyor mu ? 
    public bool Matches(TireBrand wantedBrand, TireSeason wantedSeason)
    {
        return brand == wantedBrand && season == wantedSeason;
    }
    
}
