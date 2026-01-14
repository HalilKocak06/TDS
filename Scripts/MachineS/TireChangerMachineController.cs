using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireChangerMachineController : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] Transform wheelSlotPoint; //mevcut slot
    [SerializeField] Transform rimStayPoint; // jantın kalacağı yer
    [SerializeField] Transform tirePickupPoint; //lastiğin çıkacağı yer

    [Header("Work")]
    [SerializeField] float workTime = 5f;

    public bool HasWheel => currentWheel != null;
    public bool IsWorking {get; private set;}

    WheelCarryable currentWheel;

    public Transform GetRimStayPoint() => rimStayPoint;

    //PlayerWheelCarrier burayı çağıracak
    public bool TryAcceptWheel(WheelCarryable wheel)
    {
        if (IsWorking) return false;
        if (wheel == null) return false;

        currentWheel = wheel;

        //Wheel slotta sabit dursun
        ParentAndSnapKeepWorld(currentWheel.transform, wheelSlotPoint);

        currentWheel.SetPlacedOnMachine(true);
        return true;
    }

    public void TryStart()
    {
        if(!HasWheel) return;
        if(IsWorking) return;

        StartCoroutine(WorkRoutine());
    }

    IEnumerator WorkRoutine()
    {
        IsWorking = true;
        //Ses animasyonu da ekleyebilirsin TODO:

        yield return new WaitForSeconds(workTime);

        SplitWheel();
        IsWorking = false;
    }

    void SplitWheel()
    {
        if (currentWheel == null)
        {
            Debug.LogWarning("SplitWheel called but currentWheel is null");
            return;
        }

        //Wheel içinde Tire ve rim bulalım
        var tire = currentWheel.transform.Find("Tyres");
        var rim = currentWheel.transform.Find("Rim");

        if(tire == null || rim == null)
        {
            Debug.LogWarning("Wheel içinde Tire/Rim child bulunamadı. İsimler tam 'Tire' ve 'Rim' olmalı");
            return;
        }

        int genericLayer = LayerMask.NameToLayer("Generic");
        if (genericLayer == -1)
        {
            Debug.LogError("Layer 'Generic' yok! Project Settings > Tags and Layers'tan 'Generic' layer'ı ekle.");
        }
        else
        {
            SetLayerRecursively(tire.gameObject, genericLayer);
            SetLayerRecursively(rim.gameObject, genericLayer);
        }



        var rimTarget = (rimStayPoint != null ? rimStayPoint : wheelSlotPoint);
        ParentAndSnapKeepWorld(rim, rimTarget);

        var rimPhys = rim.GetComponentInChildren<SplitPhysicsToggle>(true);
        if(rimPhys) rimPhys.SetOnMachine(true);
        else Debug.LogError("Rim tarafında SplitPhysicsToggle bulunamadı (child dahil arandı).");



        Transform pickPoint = tirePickupPoint != null ? tirePickupPoint : wheelSlotPoint;
        ParentAndSnapKeepWorld(tire,pickPoint);

        //Tire yerde serbest E ile al / G ile bırak
        var tirePhys = tire.GetComponentInChildren<SplitPhysicsToggle>(true);
        if(tirePhys) tirePhys.SetLoose(true);
        else Debug.LogError("Tyre tarafında SplitPhysicsToggle bulunamadı (child dahil arandı).");



        //Wheel root artık yok işlevsiz kalsın
        currentWheel.gameObject.SetActive(false);

        //Tire'ı player'a ver
        GiveTireToPlayer(tire.gameObject);

        currentWheel = null;

    }

    void GiveTireToPlayer(GameObject tireObj)
    {
        //PlayerWheelCarrier'a ya da genel bir 
        var playerCarrier = FindFirstObjectByType<PlayerWheelCarrier>();
        if(playerCarrier == null)
        {
            Debug.LogWarning("PlayerWheelCarrier bulunamadı. Tire yerde bırakılıyor.");
            tireObj.transform.SetParent(null);
            return;
        }

        playerCarrier.ForcePickUpExternalObject(tireObj);

    }

    //Bu method parenting / hizalama yapmaz, yani senin mevcut “otomatik yerleşme” sistemini bozmaz. Sadece “makinenin içinde şu an wheel var” state’ini kurar.
    public bool RegisterWheelAlreadyPlaced(WheelCarryable wheel)
    {
        if (IsWorking) return false;
        if (wheel == null) return false;

        currentWheel = wheel;
        return true;
    }

    static void ParentAndSnapKeepWorld(Transform child, Transform parent)
    {
        //World scale/rot/pos korunur.
        child.SetParent(parent, true);

        child.SetPositionAndRotation(parent.position, parent.rotation);

    }

    static void SetLayerRecursively(GameObject obj, int layer)
    {
    obj.layer = layer;
    foreach (Transform child in obj.transform)
        SetLayerRecursively(child.gameObject, layer);
    }

}

