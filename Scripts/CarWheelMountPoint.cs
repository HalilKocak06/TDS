using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class CarWheelMountPoint : MonoBehaviour
{
    [Header("WheelMounttoCar")]
    [SerializeField] Transform wheelMountPointCar;
    [SerializeField] bool keepWorldScale = true;

    [UnitHeaderInspectable("Slot Info")]
    [SerializeField] string slotName = "FL";
    public string SlotName => slotName;

    public event Action<CarWheelMountPoint> OnChanged; //Job manager dinleyecek.

    public WheelCarryable CurrentWheel {get; private set;}
    public bool HashWheel => CurrentWheel != null;

    public TireIdentity GetMountedTireIdentity()
    {
        if(CurrentWheel == null) return null;
        
        return CurrentWheel.GetComponentInChildren<TireIdentity>(true);
    }

    public bool AreLugNutsTight()
    {
        if(CurrentWheel == null) return false;

        var wc = CurrentWheel.GetComponent<WheelController>();
        if(wc == null) return false;

        return wc.AreLugNutsTight;
    }


    public bool TryMountToCar(WheelCarryable wheel)
    {
        if(wheel == null) return false;
        if(HashWheel) return false;
        if(wheelMountPointCar == null)
        {
            Debug.LogWarning("CarWheelMountPoint: mountPoint atanmadi!");
            return false;
        }

        CurrentWheel = wheel;

        //Araca takılınca fizik kapanmalı 
        wheel.SetPlacedOnMachine(true);

        if(keepWorldScale)
            ParentAndSnapKeepWorldScale(wheel.transform, wheelMountPointCar);
        else
            ParentAndSnapLocal(wheel.transform, wheelMountPointCar);

        Debug.Log("CarWheelMountPoint: Wheel mounted!");
        
        var tid = GetMountedTireIdentity();
        Debug.Log($"[Mount] {slotName} -> {(tid != null ? tid.stringDisplayName : "NO TireIdentity")}");
        
        // JobManager'a haber ver
        OnChanged?.Invoke(this);
        
        return true;        
    }

    public WheelCarryable TryReleaseFromCar()
    {
        if (CurrentWheel == null) return null;

        var wheel = CurrentWheel;
        CurrentWheel = null;

        // Wheel no longer fixed on car
        wheel.SetPlacedOnMachine(false);

        // release log + event
        Debug.Log($"[Mount] {slotName} -> RELEASED");
        OnChanged?.Invoke(this);

        return wheel;
    }

    static void ParentAndSnapLocal(Transform child, Transform parent)
    {
        child.SetParent(parent, false);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
    }

    static void ParentAndSnapKeepWorldScale(Transform child, Transform parent)
    {
        Vector3 worldScale = child.lossyScale;

        child.SetParent(parent, true);
        child.SetPositionAndRotation(parent.position, parent.rotation);

        Vector3 parentLossy = parent.lossyScale;
        child.localScale = new Vector3(
            worldScale.x / parentLossy.x,
            worldScale.y / parentLossy.y,
            worldScale.z / parentLossy.z
        );
    }


    
}
