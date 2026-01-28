using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    //Job -based cache supports multiple split wheels
    class CachedAssembly
    {
        //Buradaki amacımız her split'i hafıza atmak.

        public GameObject wheelRoot;
        public Transform rim;

        public Vector3 rimLocalPos;
        public Quaternion rimLocalRot;
        public Vector3 rimLocalScale;

        public Vector3 tireLocalPos;
        public Quaternion tireLocalRot;
        public Vector3 tireLocalScale;

        public int wheelLayer;
    }

    static int s_nextJobId = 1;
    readonly Dictionary<int, CachedAssembly> cacheByJob = new Dictionary <int,CachedAssembly>();


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

        int jobId = s_nextJobId++;
        var assembly = new CachedAssembly
        {
            wheelRoot = currentWheel.gameObject,
            rim = rim,

            rimLocalPos = rim.localPosition,
            rimLocalRot = rim.localRotation,
            rimLocalScale = rim.localScale,

            tireLocalPos = tire.localPosition,
            tireLocalRot = tire.localRotation,
            tireLocalScale = tire.localScale,

            wheelLayer = currentWheel.gameObject.layer
        };

        cacheByJob[jobId] =  assembly;

        //identity taglar :
        var wheelId = assembly.wheelRoot.GetComponent<WheelPartIdentity>();
        if (wheelId == null) wheelId = assembly.wheelRoot.AddComponent<WheelPartIdentity>();
        wheelId.kind = WheelPartKind.WheelRoot;
        wheelId.jobId = jobId;

        var rimId = rim.GetComponent<WheelPartIdentity>();
        if(rimId == null) rimId = rim.gameObject.AddComponent<WheelPartIdentity>();
        rimId.kind = WheelPartKind.Rim;
        rimId.jobId = jobId;

        var tireId = tire.GetComponent<WheelPartIdentity>();
        if(tireId == null) tireId = tire.gameObject.AddComponent<WheelPartIdentity>();
        tireId.kind = WheelPartKind.Tire;
        tireId.jobId = jobId;




        


        
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

        // B: rim must be in machine
        var rimInMachine = GetRimInMachine();
        if (rimInMachine == null)
        {
            Debug.LogWarning("Mount failed: Makinede rim yok (rimStayPoint altında Rim bulunamadı).");
            return false;
        }

        int jobId = rimInMachine.jobId;
        if (jobId <= 0 || !cacheByJob.TryGetValue(jobId, out var assembly))
        {
            Debug.LogWarning($"Mount failed: Rim jobId={jobId} için cache bulunamadı (yanlış rim / cache temizlenmiş olabilir).");
            return false;
        }

        // Tire identity check
        var tireId = newTireObj.GetComponent<WheelPartIdentity>();
        if (tireId == null)
        {
            // new tire olabilir, ekleyelim
            tireId = newTireObj.AddComponent<WheelPartIdentity>();
            tireId.kind = WheelPartKind.Tire;
            tireId.jobId = 0;
        }

        if (tireId.kind != WheelPartKind.Tire)
        {
            Debug.LogWarning("Mount failed: Elindeki parça tire değil.");
            return false;
        }

        // (opsiyonel) yanlışlıkla rim'i tire diye takma
        if (newTireObj.name.ToLower().Contains("rim"))
        {
            Debug.LogWarning("Mount failed: Elindeki parça rim görünüyor, tire bekleniyordu.");
            return false;
        }

        // A: assign tire to this job
        tireId.jobId = jobId;

        // Wheel root’u geri aç
        assembly.wheelRoot.SetActive(true);

        // Wheel root’u slot’a geri koy
        ParentAndSnapKeepWorld(assembly.wheelRoot.transform, wheelSlotPoint);

        // Rim'i wheel altına geri al + local transformları geri bas
        assembly.rim.SetParent(assembly.wheelRoot.transform, false);
        assembly.rim.localPosition = assembly.rimLocalPos;
        assembly.rim.localRotation = assembly.rimLocalRot;
        assembly.rim.localScale = assembly.rimLocalScale;

        Transform tireSocket = assembly.wheelRoot.transform.Find("TireSocket"); // opsiyonel
        Transform parent = tireSocket != null ? tireSocket : assembly.wheelRoot.transform;

        // Tire'ı wheel altına geri al + local transformları geri bas
        newTireObj.transform.SetParent(parent, false);
        newTireObj.transform.localPosition = assembly.tireLocalPos;
        newTireObj.transform.localRotation = assembly.tireLocalRot;
        newTireObj.transform.localScale = assembly.tireLocalScale;

        // Layer’ları wheel layer’a geri al
        if (assembly.wheelLayer != -1)
        {
            SetLayerRecursively(assembly.rim.gameObject, assembly.wheelLayer);
            SetLayerRecursively(newTireObj, assembly.wheelLayer);
        }

        // Rim/Tire physics’i tekrar kapat (wheel parçası oldular)
        var rimPhys = assembly.rim.GetComponentInChildren<SplitPhysicsToggle>(true);
        if (rimPhys) rimPhys.DisableAll(); // IMPORTANT

        var tirePhys = newTireObj.GetComponentInChildren<SplitPhysicsToggle>(true);
        if (tirePhys) tirePhys.DisableAll(); // IMPORTANT

        // WheelCarryable tekrar makinede “placed” olsun
        var wc = assembly.wheelRoot.GetComponent<WheelCarryable>();
        if (wc != null)
        {
            wc.SetPlacedOnMachine(true);
            currentWheel = wc;
        }

        // cache temizle (sadece bu job için)
        cacheByJob.Remove(jobId);

        Debug.Log($"Mount success: jobId={jobId} reassembled!");
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

    WheelPartIdentity GetRimInMachine()
    {
    if (rimStayPoint == null) return null;

    // rimStayPoint altında duran Rim parçasını bul
    foreach (Transform ch in rimStayPoint)
    {
        var id = ch.GetComponent<WheelPartIdentity>();
        if (id != null && id.kind == WheelPartKind.Rim)
            return id;
    }
    return null;
    }

}

