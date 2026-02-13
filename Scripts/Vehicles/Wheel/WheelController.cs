using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    [SerializeField] LugNut[] lugNuts; //Bijonları buraya sürükle.
    public bool IsUnlocked { get; private set;} 

    public bool AreLugNutsTight => !IsUnlocked;

    // Update is called once per frame
    void Update()
    {
        int removed = 0 ;
        foreach( var n in lugNuts)
            if (n != null && n.IsRemoved) removed ++;

        bool shouldUnlock = (removed == lugNuts.Length);

        if(IsUnlocked != shouldUnlock)
        {
            IsUnlocked = shouldUnlock;
            Debug.Log(IsUnlocked ? "Wheel Unlocked" : "Wheel Locked");
        }   
    }
}