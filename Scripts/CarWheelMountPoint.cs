using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWheelMountPoint : MonoBehaviour
{
    [Header("WheelMounttoCar")]
    [SerializeField] Transform wheelMountPointCar;
    [SerializeField] bool keepWorldScale = true;

    public WheelCarryable CurrentWheel {get; private set;}
    public bool HashWheel => CurrentWheel != null;

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
        return true;        
    }

    public WheelCarryable TryReleaseFromCar()
    {
        if (CurrentWheel == null) return null;

        var wheel = CurrentWheel;
        CurrentWheel = null;

        // Wheel no longer fixed on car
        wheel.SetPlacedOnMachine(false);

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
