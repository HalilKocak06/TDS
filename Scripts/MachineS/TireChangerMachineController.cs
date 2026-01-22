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

    // Split sonrası tekrar birleştirmek için cache
    GameObject cachedWheelRoot; // currentWheel gameObject
    Transform cachedRim;
    // Transform cachedTire;

    Vector3 rimLocalPos;
    Quaternion rimLocalRot;
    Vector3 rimLocalScale;

    Vector3 tireLocalPos;
    Quaternion tireLocalRot;
    Vector3 tireLocalScale;
    
    int cachedWheelLayer = -1;

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
        var tire = currentWheel.transform.Find("Tyres"); //Tyre objesini bulur
        var rim = currentWheel.transform.Find("Rim"); // Rim objesini bulur

        if (tire == null || rim == null)
        {
            Debug.LogWarning("Wheel içinde 'Tyres' veya 'Rim' child bulunamadı. İsimler birebir aynı olmalı.");
            return;
        }


        //Orjinal local transformları saklıyoruz ki birleştirirken kullanacağız
        //Bulduğumuz objelerden alıyoruz konumlarını 
        rimLocalPos = rim.localPosition;
        rimLocalRot = rim.localRotation;
        rimLocalScale = rim.localScale;

        tireLocalPos = tire.localPosition;
        tireLocalRot = tire.localRotation;
        tireLocalScale = tire.localScale;

        //  Cache: tekrar assemble edeceğiz
        cachedWheelRoot = currentWheel.gameObject;
        cachedRim = rim;
        // cachedTire = tire;
        cachedWheelLayer = currentWheel.gameObject.layer;



        
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

    //  lastik eldeyken E'ye basınca çağrılacak
    public bool TryMountTire(GameObject newTireObj)
    {
        if (IsWorking) return false;

        if (newTireObj == null) return false;
        if (cachedWheelRoot == null) return false;
        if (cachedRim == null) return false;

        // Makinede rim var mı? (rimStayPoint altında olmalı)
        if (rimStayPoint == null || cachedRim.parent != rimStayPoint)
        {
            Debug.LogWarning("Mount failed: Rim makinede değil (rimStayPoint altında bulunamadı).");
            return false;
        }

         // Basit koruma: yanlışlıkla rim'i "tire" diye takma
        if (newTireObj.name.ToLower().Contains("rim"))
        {
            Debug.LogWarning("Mount failed: Elindeki parça rim görünüyor, tire bekleniyordu.");
            return false;
        }


        // Wheel root’u geri aç
        cachedWheelRoot.SetActive(true);

        // Wheel root’u slot’a geri koy
        ParentAndSnapKeepWorld(cachedWheelRoot.transform, wheelSlotPoint);

        // Rim'i wheel altına geri al + local transformları geri bas
        cachedRim.SetParent(cachedWheelRoot.transform, false);
        cachedRim.localPosition = rimLocalPos;
        cachedRim.localRotation = rimLocalRot;
        cachedRim.localScale = rimLocalScale;

        Transform tireSocket = cachedWheelRoot.transform.Find("TireSocket"); // opsiyonel
        Transform parent = tireSocket != null ? tireSocket : cachedWheelRoot.transform;

        // Tire'ı wheel altına geri al + local transformları geri bas
        // newTireObj.SetParent(cachedWheelRoot.transform, false);
        newTireObj.transform.SetParent(parent,false);
        newTireObj.transform.localPosition = tireLocalPos;
        newTireObj.transform.localRotation = tireLocalRot;
        newTireObj.transform.localScale = tireLocalScale;

        // Layer’ları wheel layer’a geri al
        if (cachedWheelLayer != -1)
        {
            SetLayerRecursively(cachedRim.gameObject, cachedWheelLayer);
            SetLayerRecursively(newTireObj, cachedWheelLayer);
        }

        // Rim/Tire physics’i tekrar kapat (wheel parçası oldular)
        var rimPhys = cachedRim.GetComponentInChildren<SplitPhysicsToggle>(true);
        if (rimPhys) rimPhys.SetOnMachine(true);

        var tirePhys = newTireObj.GetComponentInChildren<SplitPhysicsToggle>(true);
        if (tirePhys) tirePhys.SetOnMachine(true);

        // WheelCarryable tekrar makinede “placed” olsun
        var wc = cachedWheelRoot.GetComponent<WheelCarryable>();
        if (wc != null)
        {
            wc.SetPlacedOnMachine(true);
            // İstersen burada currentWheel = wc yapabilirsin (makine tekrar wheel görüyor olsun)
            currentWheel = wc;
        }

        // Cache temizle (istersen sonraki split’e kadar saklayabilirsin; ben temiz bırakıyorum)
        cachedWheelRoot = null;
        cachedRim = null;

        Debug.Log("Mount success: Tire + Rim => Wheel reassembled!");
        return true;
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

