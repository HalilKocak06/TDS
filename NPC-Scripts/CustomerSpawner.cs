using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    
    [Header("Refs")]
    [SerializeField] GameObject customerPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform talkPoint;
    [SerializeField] Transform exitPoint;

    [SerializeField] Transform playerTransform;

    CustomerController current;

    void Start()
    {
        SpawnOneCustomer();
    }

    public void SpawnOneCustomer()
    {
        if (current != null) return;

        var go = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation); //NPC prefab oluşturduk seçilen üzerinden.
        current = go.GetComponent<CustomerController>();
        if(current == null) current = go.AddComponent<CustomerController>();

        current.Init(talkPoint, exitPoint);
        current.SetPlayer(playerTransform);
        current.BeginEnterShop();
    }
}
