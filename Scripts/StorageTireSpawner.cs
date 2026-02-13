using System.Collections.Generic;
using UnityEngine;

public class StorageTireSpawner : MonoBehaviour
{
    [Header("Spawn Input")]
    [SerializeField] KeyCode spawnKey = KeyCode.O;

    [Header("Debug Spawn Spec (for now)")]
    [SerializeField] int debugWidth = 205;
    [SerializeField] int debugAspect = 55;
    [SerializeField] int debugRim = 16;
    [SerializeField] TireSeason debugSeason = TireSeason.Summer;
    [SerializeField] TireCondition debugCondition = TireCondition.New;


    [Header("What to Spawn")]
    [SerializeField] GameObject newTirePrefab;

    [Header("Where to Spawn")]
    [SerializeField] Transform spawnLocate;

    [Header("Optional Setup")]
    [SerializeField] bool forceGenericLayer=true;
    [SerializeField] string genericLayerName = "Generic";
    [SerializeField] bool giveToPlayerHand = false;
    [SerializeField] List<GameObject> tires = new List<GameObject>();
    [SerializeField] int maxLimit = 1;

    [Header("Available Sizes (R16 examples)")]
    [SerializeField] List<TireSize> availableSizes = new List<TireSize>()
    {
    new TireSize(205,55,16),
    new TireSize(195,55,16),
    new TireSize(215,55,16),
    new TireSize(215,50,16),
    };

    static int _uidCounter = 1000;



    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(spawnKey)) return;
        

        var size = new TireSize(debugWidth, debugAspect, debugRim);
        SpawnNewTire(size, debugSeason, debugCondition);

    }

    public GameObject SpawnNewTire(TireSize size, TireSeason season, TireCondition condition)
    {
        if (newTirePrefab == null)
        {
            Debug.LogWarning("DepotTireSpawner : newTirePrefab atanmamiş!!");
            return null;
        }

        if (spawnLocate == null)
        {
            Debug.LogWarning("DepotTireSPawner: spawnLocate atanmamış!");
            return null;
        }

        //1)Instantiate
        GameObject tire = Instantiate(
            newTirePrefab,
            spawnLocate.position,
            spawnLocate.rotation
        );

        var partId = tire.GetComponent<WheelPartIdentity>();
        if(partId == null) partId = tire.AddComponent<WheelPartIdentity>();
        partId.kind = WheelPartKind.Tire;
        partId.jobId = 0;

        // Identity oluşturmak
        var tireId = tire.GetComponent<TireIdentity>();
        if(tireId == null) tireId = tire.AddComponent<TireIdentity>();

        int uid = ++_uidCounter;
        tireId.Init(size, season, condition, uid);

        Debug.Log($"[Spawner] Spawned: {tireId.stringDisplayName} (uid={uid}) -> {tire.name}");


        var spt = tire.GetComponentInChildren<SplitPhysicsToggle>(true);
        if (spt != null)
        {
            spt.SetLoose(true); // collider aç + rb dynamic
        }
        else if (tire.TryGetComponent<GenericCarryable>(out var gc))
        {
            gc.SetCarried(false); // collider açık + rb gravity açık
        }

        // 2)
        if(forceGenericLayer)
        {
            int layer = LayerMask.NameToLayer(genericLayerName);
            if(layer == -1)
            {
                Debug.LogWarning($"DepotTireSpawner: Layer '{genericLayerName} bulunamadı.");
            }
            else
            {
                SetLayerRecursively(tire,layer);
            }
        }

         // 3) (Opsiyonel) direkt ele ver
        if (giveToPlayerHand)
        {
            var player = FindFirstObjectByType<PlayerWheelCarrier>();
            if (player != null)
            {
                player.ForcePickUpExternalObject(tire);
            }
        }

         Debug.Log("DepotTireSpawner: New tire spawned -> " + tire.name);
         return tire;
    
    }

    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach(Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public GameObject SpawnRequestedTire(int width, int aspect, int rim, TireSeason season, TireCondition condition)
    {
        var wanted = new TireSize(width, aspect, rim);

        bool found =false;
        foreach ( var s in availableSizes)
        {
            if(s.width == wanted.width && s.aspect == wanted.aspect && s.rim == wanted.rim)
            {
                found = true;
                wanted = s;
                break;
            }
        }

        if (!found)
        {
        Debug.LogWarning($"[Spawner] Requested size NOT in available list: {wanted}");
        return null;
        }

        //Eğer lastik doğru ebatı bulunduysa spawn
        return SpawnNewTire(wanted,season, condition);
    }

    
}
