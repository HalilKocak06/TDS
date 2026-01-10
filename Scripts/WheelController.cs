using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    [SerializeField] LugNut[] lugNuts; //Bijonları buraya sürükle.
    public bool IsUnlocked { get; private set;} 

    // Update is called once per frame
    void Update()
    {
        if(IsUnlocked) return;

        int removed = 0 ;
        foreach( var n in lugNuts)
            if (n != null && n.IsRemoved) removed ++;

        if(removed == lugNuts.Length)
        {
            IsUnlocked = true;
            Debug.Log("Wheel Unlocked"); 
        }    
    }
}
