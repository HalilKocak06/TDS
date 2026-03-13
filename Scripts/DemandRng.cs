using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DemandRng
{

    //YAZ %75 SUMMER , %25 FOURSEASON
    //KIŞ %80 WINTER, %20 FOURSEASON
    public static TireSeason PickWantedTireSeason(WorldSeason worldSeason, System.Random rng)
    {
        int roll = rng.Next(0, 100);
        
        if(worldSeason == WorldSeason.Summer)
            return (roll < 80) ? TireSeason.Summer : TireSeason.FourSeason;
        else
            return (roll < 80) ? TireSeason.Winter : TireSeason.FourSeason;    
    }

    // ✅ Marka: %75 Michelin, %25 Kodemax
    // ❗ Kodemax 4 mevsim YOK: season FourSeason ise Michelin zorunlu
    public static TireBrand PickWantedBrand(TireSeason wantedSeason, System.Random rng)
    {
        if (wantedSeason == TireSeason.FourSeason)
            return TireBrand.Micheal;

        int roll = rng.Next(0, 100);
        return (roll < 75) ? TireBrand.Micheal : TireBrand.Kodemax;
    }
    
}
