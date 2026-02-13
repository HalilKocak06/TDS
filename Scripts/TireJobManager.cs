using UnityEngine;

public class TireJobManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] StorageTireSpawner tireSpawner;
    [SerializeField] CarJobController carJob;

    [Header("Debug Start")]
    [SerializeField] bool autoStart = false;

    TireOrder activeOrder;
    CarWheelMountPoint[] mountPoints;
    bool completed;

    void Awake()
{
    // Aynı GameObject üzerinde varsa direkt al
    if (carJob == null)
        carJob = GetComponent<CarJobController>();

    // Sahnedeki herhangi bir yerdeyse bul
    if (carJob == null)
        carJob = FindFirstObjectByType<CarJobController>();

    // TireSpawner da boş kalırsa bul
    if (tireSpawner == null)
        tireSpawner = FindFirstObjectByType<StorageTireSpawner>();

    Debug.Log($"[JOB] Awake refs -> carJob={(carJob!=null)} tireSpawner={(tireSpawner!=null)}");
}

    void Start()
    {
        if (!autoStart) return;

        var order = new TireOrder
        {
            size = new TireSize(195, 55, 16),
            season = TireSeason.Summer,
            condition = TireCondition.New,
            quantity = 4
        };

        StartJob(order);
    }

    public void StartJob(TireOrder order)
    {
        completed = false;
        activeOrder = order;

        Debug.Log($"[JOB] START -> {activeOrder.Display}");

        // 1) Arabayı lifte spawnla (senin mevcut sistemin)
        if (carJob == null)
        {
            Debug.LogWarning("[JOB] carJob missing!");
            return;
        }

        var car = carJob.SpawnCarAtLift();
        if (car == null)
        {
            Debug.LogWarning("[JOB] Car spawn failed!");
            return;
        }

        // 2) Arabadan mountpointleri topla
        mountPoints = car.GetComponentsInChildren<CarWheelMountPoint>(true);
        if (mountPoints == null || mountPoints.Length == 0)
        {
            Debug.LogWarning("[JOB] No CarWheelMountPoint on car!");
            return;
        }

        // 3) Mountpoint event subscribe (teker tak/çıkar oldukça Validate çalışsın)
        foreach (var mp in mountPoints)
        {
            if (mp == null) continue;
            mp.OnChanged -= HandleMountChanged;
            mp.OnChanged += HandleMountChanged;
        }

        // 4) Doğru lastikleri depoda spawnla
        if (tireSpawner == null)
        {
            Debug.LogWarning("[JOB] tireSpawner missing!");
            return;
        }

        for (int i = 0; i < activeOrder.quantity; i++)
        {
            tireSpawner.SpawnRequestedTire(
                activeOrder.size.width,
                activeOrder.size.aspect,
                activeOrder.size.rim,
                activeOrder.season,
                activeOrder.condition
            );
        }

        Validate();
    }

    void HandleMountChanged(CarWheelMountPoint mp)
    {
        if (completed) return;
        Validate();
    }

    public bool Validate()
    {
        if (activeOrder == null || mountPoints == null)
        {
            Debug.Log("[JOB] No active job.");
            return false;
        }

        bool ok = true;

        foreach (var mp in mountPoints)
        {
            if (mp == null) continue;

            // teker takılı mı?
            if (!mp.HashWheel)
            {
                Debug.Log($"[JOB] FAIL: {mp.SlotName} wheel missing");
                ok = false;
                continue;
            }

            // lastik kimliği var mı?
            var tid = mp.GetMountedTireIdentity();
            if (tid == null)
            {
                Debug.Log($"[JOB] FAIL: {mp.SlotName} NO TireIdentity");
                ok = false;
                continue;
            }

            // ebat doğru mu?
            if (tid.size.width != activeOrder.size.width ||
                tid.size.aspect != activeOrder.size.aspect ||
                tid.size.rim != activeOrder.size.rim)
            {
                Debug.Log($"[JOB] FAIL: {mp.SlotName} wrong size -> {tid.size} expected {activeOrder.size}");
                ok = false;
            }

            // mevsim doğru mu?
            if (tid.season != activeOrder.season)
            {
                Debug.Log($"[JOB] FAIL: {mp.SlotName} wrong season -> {tid.season} expected {activeOrder.season}");
                ok = false;
            }

            // new/used doğru mu?
            if (tid.condition != activeOrder.condition)
            {
                Debug.Log($"[JOB] FAIL: {mp.SlotName} wrong condition -> {tid.condition} expected {activeOrder.condition}");
                ok = false;
            }

            // bijonlar sıkılı mı?
            if (!mp.AreLugNutsTight())
            {
                Debug.Log($"[JOB] FAIL: {mp.SlotName} lug nuts not tight");
                ok = false;
            }
        }

        if (ok && !completed)
        {
            completed = true;
            Debug.Log($"[JOB] COMPLETE ✅ -> {activeOrder.Display}");
            Debug.Log("[Customer] Eline sağlık ustam.");
        }
        else if (!ok)
        {
            Debug.Log("[JOB] NOT COMPLETE yet...");
        }

        return ok;
    }
}
