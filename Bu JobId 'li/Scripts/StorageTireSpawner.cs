using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class StorageTireSpawner : MonoBehaviour
{
    [Header("Spawn Input")]
    [SerializeField] KeyCode spawnKey = KeyCode.O;

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


    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(spawnKey)) return;

        SpawnNewTire();
    }

    public GameObject SpawnNewTire()
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

        // if (tires.Count == maxLimit)
        // {
        //     return;
        // }

        //1)Instantiate
        GameObject tire = Instantiate(
            newTirePrefab,
            spawnLocate.position,
            spawnLocate.rotation
        );

        // Identity oluşturmak
        //Burası önemli bir daha bak başka zaman (26 ocakta yazıldı)
        var id = tire.GetComponent<WheelPartIdentity>();
        if(id == null) id = tire.AddComponent<WheelPartIdentity>();
        id.kind = WheelPartKind.Tire;
        id.jobId = 0;

        

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
}
