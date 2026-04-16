using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer Prefabs")]
    [SerializeField] List<GameObject> customerPrefabs = new List<GameObject>();
    
    [Header("Refs")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform talkPoint;
    [SerializeField] Transform exitPoint;
    [SerializeField] Transform outsidePoint1;

    [SerializeField] Transform outsidePoint2;

    [SerializeField] Transform outsidePoint3;

    [SerializeField] Transform waitingPoint;

    [SerializeField] Transform playerTransform;
    
    [SerializeField] ShopCoordinator coordinator;


    CustomerController customer;

    [Header("Spawn Rules")]
    [SerializeField] int maxAliveCustomers = 2;     // aynı anda kaç müşteri sahnede olsun
    [SerializeField] float spawnInterval = 8f;      // kaç saniyede bir spawn denesin
    [SerializeField] bool randomize = true;
    float nextSpawnTime;
    readonly List<CustomerController> aliveCustomers = new List<CustomerController>();

    //Talk Slot + dışarıda bekleme
    CustomerController talkOccupant;
    readonly List<CustomerController> outsideLine = new List<CustomerController>();

    void Start()
    {
        nextSpawnTime = Time.time + 4.0f;

        SpawnOneCustomer();
    }

    void Update()
    {
        CleanupNulls();

        if(Time.time >= nextSpawnTime)
        {
            SpawnOneCustomer();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    public void SpawnOneCustomer()
    {
        //1-Limitleme sahnede yeterince müşteri varsa basma
        if(aliveCustomers.Count >= maxAliveCustomers)
            return;

        //2 Prefab listesi boşsa basma
        if(customerPrefabs == null || customerPrefabs.Count == 0)
        {
            Debug.LogWarning("[Spawner] customerPrefabs list is empty");
            return;
        }    

        //3-Prefab seç
        GameObject prefabToSpawn = customerPrefabs[0];
        if(randomize && customerPrefabs.Count > 1)
        {
            int idx = Random.Range(0, customerPrefabs.Count);
            prefabToSpawn = customerPrefabs[idx];
        }


        //4-Instantiate 
        var go = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation); //NPC prefab oluşturduk seçilen üzerinden.
        customer = go.GetComponent<CustomerController>();
        if(customer == null) customer = go.AddComponent<CustomerController>();

        customer.Init(talkPoint, outsidePoint1, waitingPoint, exitPoint, coordinator);
        if(EconomyManager.I != null && EconomyManager.I.TryCreateRandomOrder(out var order))
        {
            customer.SetPendingOrder(order);
            Debug.Log($"[Spawner] Assigned pending order -> {order.Display}");
        }
        else
        {
            Debug.LogWarning("[Spawner] Failed to assign pending order");
        }
        
        customer.SetPlayer(playerTransform);
        customer.OnFreedTalkPoint -= HandleFreedTalkPoint;
        customer.OnFreedTalkPoint += HandleFreedTalkPoint;

        //Talk doluysa dışarıda beklet
        if(talkOccupant == null)
        {
            talkOccupant = customer;
            customer.GoToTalkPoint();
        }
        else
        {
            outsideLine.Add(customer);
            RebuildOutsideLinePositions();
        }

        //7-Takip listesine ekliyoruz 
        aliveCustomers.Add(customer);
        Debug.Log($"[Spawner] Spawned customer: {go.name} (alive={aliveCustomers.Count}) talkOccupant={(talkOccupant ? talkOccupant.name : "none")} outsideQ={outsideLine.Count}");
    }

    void HandleFreedTalkPoint(CustomerController who)
    {
        //talkOccupant bu müşteri ise boşalt
        if(talkOccupant == who)
            talkOccupant = null;

        //dışarıda bekleyen var ise talk'a alalım
        if(talkOccupant == null && outsideLine.Count > 0)
        {
            var next = outsideLine[0];
            outsideLine.RemoveAt(0);

            if(next != null)
            {
                talkOccupant = next;
                next.GoToTalkPoint();
                Debug.Log($"[Spawner] Talk freed -> next customer moved to talk: {next.name}");
            }

            RebuildOutsideLinePositions();
        }    
    }

    void CleanupNulls()
    {
        for(int i=outsideLine.Count -1; i>=0; i--)
        {
            if(outsideLine[i] == null)
                outsideLine.RemoveAt(i);
        }

        //talkOccupant destroy olduysa da slotu temizle
        if(talkOccupant == null && outsideLine.Count > 0)
        {
            //bu durum bazen frame farkılya olur slot boşsa sıradakini al
            var next = outsideLine[0];
            if(next != null)
            {
                talkOccupant = next;
                next.GoToTalkPoint();
            }

            RebuildOutsideLinePositions();
        }
    }

    void RebuildOutsideLinePositions()
    {
        for(int i=0; i< outsideLine.Count; i++)
        {
            var c = outsideLine[i];
            if(c== null) continue;

            if(i==0)
            {
                //outsidePoint1
                c.GoToOutsidePoint();
            }
            else if(i==1)
            {
                c.GoToSpecificPoint(outsidePoint2);
            }
            else
            {
                c.GoToSpecificPoint(outsidePoint3);
            }
        }
    }
}
