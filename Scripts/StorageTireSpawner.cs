using System.Collections.Generic;
using UnityEngine;

public class StorageTireSpawner : MonoBehaviour
{
    [Header("Spawn Input")]
    [SerializeField] KeyCode spawnKey = KeyCode.O;

    [Header("Debug Spawn Spec")]
    [SerializeField] int debugWidth = 225;
    [SerializeField] int debugAspect = 45;
    [SerializeField] int debugRim = 18;
    [SerializeField] TireSeason debugSeason = TireSeason.Summer;
    [SerializeField] TireCondition debugCondition = TireCondition.New;
    [SerializeField] TireBrand debugBrand = TireBrand.Micheal;

    [Header("Legacy Fallback Prefab (opsiyonel)")]
    [SerializeField] GameObject newTirePrefab;

    [Header("Brand + Season -> Prefab Listesi")]
    [SerializeField] List<TirePrefabData> tirePrefabs = new List<TirePrefabData>();

    [Header("AŞAMA 3 - Car + Prefab Fit Ayarları")]
    [SerializeField] List<TireFitData> tireFits = new List<TireFitData>();

    [Header("Where to Spawn")]
    [SerializeField] Transform spawnLocate;

    [Header("Optional Setup")]
    [SerializeField] bool forceGenericLayer = true;
    [SerializeField] string genericLayerName = "Generic";
    [SerializeField] bool giveToPlayerHand = false;

    [Header("Debug / Runtime Tracking")]
    [SerializeField] List<GameObject> tires = new List<GameObject>();
    [SerializeField] int maxLimit = 1;

    [SerializeField] TireCatalogS0 catalog;

    static int _uidCounter = 1000;

    void Update()
    {
        if (!Input.GetKeyDown(spawnKey))
            return;

        var size = new TireSize(debugWidth, debugAspect, debugRim);

        // Debug: O tuşuna basınca seçili brand + season için doğru prefab spawn edilir
        SpawnNewTire(size, debugSeason, debugCondition, debugBrand);
    }

    public GameObject SpawnNewTire(TireSize size, TireSeason season, TireCondition condition, TireBrand brand)
    {
        // 1) Brand + season'a göre doğru prefabı bul
        string prefabId;
        GameObject prefabToSpawn = ResolvePrefab(brand, season, out prefabId);

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[Spawner] {brand} + {season} için prefab bulunamadı!");
            return null;
        }

        // 2) Spawn noktası atanmış mı?
        if (spawnLocate == null)
        {
            Debug.LogWarning("[Spawner] spawnLocate atanmamış!");
            return null;
        }

        // 3) Instantiate
        GameObject tire = Instantiate(
            prefabToSpawn,
            spawnLocate.position,
            spawnLocate.rotation
        );

        // 4) WheelPartIdentity ekle / ayarla
        var partId = tire.GetComponent<WheelPartIdentity>();
        if (partId == null)
            partId = tire.AddComponent<WheelPartIdentity>();

        partId.kind = WheelPartKind.Tire;
        partId.jobId = 0;

        // 5) TireIdentity ekle / ayarla
        var tireId = tire.GetComponent<TireIdentity>();
        if (tireId == null)
            tireId = tire.AddComponent<TireIdentity>();

        int uid = ++_uidCounter;
        tireId.Init(size, season, condition, brand, uid);

        Debug.Log("[FIT] Method entered");
        // 6) AŞAMA 3:
        // Spawn olan lastiğin visual child'ına, seçilen araca göre fit uygula
        ApplyFitToSpawnedTire(tire, prefabId);

        Debug.Log($"[Spawner] Spawned: {tireId.stringDisplayName} (uid={uid}) -> {tire.name}");

        // 7) Fizik açılışı
        var spt = tire.GetComponentInChildren<SplitPhysicsToggle>(true);
        if (spt != null)
        {
            spt.SetLoose(true);
        }
        else if (tire.TryGetComponent<GenericCarryable>(out var gc))
        {
            gc.SetCarried(false);
        }

        // 8) Layer ayarı
        if (forceGenericLayer)
        {
            int layer = LayerMask.NameToLayer(genericLayerName);
            if (layer == -1)
            {
                Debug.LogWarning($"[Spawner] Layer '{genericLayerName}' bulunamadı.");
            }
            else
            {
                SetLayerRecursively(tire, layer);
            }
        }

        // 9) İstersen direkt oyuncunun eline ver
        if (giveToPlayerHand)
        {
            var player = FindFirstObjectByType<PlayerWheelCarrier>();
            if (player != null)
            {
                player.ForcePickUpExternalObject(tire);
            }
        }

        // 10) Debug listesine ekle
        tires.Add(tire);

        // maxLimit debug mantığı
        if (maxLimit > 0 && tires.Count > maxLimit)
        {
            tires.RemoveAll(x => x == null);

            while (tires.Count > maxLimit)
            {
                if (tires[0] != null)
                    Destroy(tires[0]);

                tires.RemoveAt(0);
            }
        }

        Debug.Log($"[Spawner] New tire spawned -> {tire.name} | brand={brand} | season={season} | prefabId={prefabId}");
        return tire;
    }

    GameObject ResolvePrefab(TireBrand brand, TireSeason season, out string prefabId)
    {
        if (tirePrefabs != null && tirePrefabs.Count > 0)
        {
            for (int i = 0; i < tirePrefabs.Count; i++)
            {
                var entry = tirePrefabs[i];
                if (entry == null || entry.prefab == null)
                    continue;

                if (entry.Matches(brand, season))
                {
                    prefabId = entry.prefabId;
                    return entry.prefab;
                }
            }
        }

        prefabId = "Unknown";

        // Eski fallback mantığı
        if (newTirePrefab != null)
        {
            Debug.LogWarning($"[Spawner] Exact prefab yok. Legacy fallback kullanılacak -> {brand} {season}");
            return newTirePrefab;
        }

        return null;
    }

    void ApplyFitToSpawnedTire(GameObject tire, string prefabId)
    {
        if (tire == null)
            return;

        // Aktif araç hangisi onu bul
        var carJob = FindFirstObjectByType<CarJobController>();
        if (carJob == null)
            return;
        Debug.Log("[FIT] CarJob found -> " + carJob.name);    

        string carId = carJob.CurrentCarId;
        if (string.IsNullOrEmpty(carId))
            return;
        Debug.Log("[FIT] CurrentCarId -> " + carId);    

        // Tire prefab root'unda TireVisualFitTarget olmalı
        var fitTarget = tire.GetComponent<TireVisualFitTarget>();
        if (fitTarget == null)
        {
            Debug.LogWarning($"[FIT] TireVisualFitTarget yok -> {tire.name}");
            return;
        }

        // Önce default haline dön
        fitTarget.ResetVisual();

        // Sonra eşleşen fit varsa uygula
        for (int i = 0; i < tireFits.Count; i++)
        {
            var fit = tireFits[i];

            if (fit.carId == carId && fit.prefabId == prefabId)
            {
                fitTarget.ApplyVisualFit(
                    fit.positionOffset,
                    fit.rotationOffset,
                    fit.scaleMultiplier
                );

                Debug.Log($"[FIT] Applied -> {carId} + {prefabId}");
                return;
            }
        }

        Debug.Log($"[FIT] No fit data -> {carId} + {prefabId}");
    }

    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public GameObject SpawnRequestedTire(int width, int aspect, int rim, TireSeason season, TireCondition condition, TireBrand brand)
    {
        var wanted = new TireSize(width, aspect, rim);

        // Catalog varsa kontrol et
        if (catalog != null && catalog.sizes != null && catalog.sizes.Count > 0)
        {
            bool ok = false;

            foreach (var e in catalog.sizes)
            {
                if (e.width == width && e.aspect == aspect && e.rim == rim)
                {
                    ok = true;
                    break;
                }
            }

            if (!ok)
                Debug.LogWarning($"[Spawner] Requested size NOT in catalog: {wanted} (spawning anyway)");
        }

        return SpawnNewTire(wanted, season, condition, brand);
    }

    public GameObject SpawnFromOrder(TireOrder order)
    {
        if (order == null)
        {
            Debug.LogWarning("[Spawner] SpawnFromOrder: order null");
            return null;
        }

        return SpawnNewTire(order.size, order.season, order.condition, order.brand);
    }
}