using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerJobFlow : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CarJobController carJob;
    [SerializeField] CustomerSpawner spawner;
    [SerializeField] Transform liftSpawnPoint;
    [SerializeField] GameObject carPrefab;

    GameObject spawnedCar;
    CustomerController currentCustomer;

    bool jobActive;
    bool jobDone;

    void OnEnable()
    {
        if(carJob != null)
            carJob.OnJobCompleted += HandleJobCompleted;
    }

    void OnDisable()
    {
        if(carJob != null)
            carJob.OnJobCompleted -= HandleJobCompleted;
    }

    public void StartJobFor(CustomerController customer)
    {
        if(jobActive) return;

        currentCustomer = customer;
        jobActive = true;
        jobDone = false;

        //Araba Spawn
        if(spawnedCar != null) Destroy(spawnedCar);
        spawnedCar = Instantiate(carPrefab, liftSpawnPoint.position,liftSpawnPoint.rotation);
        
        Debug.Log("[JOB] Araba lifte geldi . 4 lastik değiştir");

        carJob.BeginJob(spawnedCar);
    }

    void HandleJobCompleted()
    {
        if(!jobActive) return;
        jobDone = true;
        Debug.Log("[JOB] iş bitti !! NPC'ye git , bildir");
        if(currentCustomer != null) currentCustomer.MarkJobDone();
    }

    public void FinishAndPay()
    {
        if (!jobActive || !jobDone) return;

        Debug.Log("[PAY] NPC: Abi eline sağlık. Al paran: 120$"); // şimdilik console

        // NPC çıkış
        currentCustomer.LeaveShop();

        // araba da gitsin/despawn (ister hemen ister 2 sn sonra)
        if (spawnedCar != null) Destroy(spawnedCar, 1.5f);

        // müşteri despawn (spawner üzerinden de yapabilirsin)
        Destroy(currentCustomer.gameObject, 2.5f);

        jobActive = false;
        jobDone = false;
        currentCustomer = null;
    }

    public bool IsJobDone => jobDone;






}
