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
    [SerializeField] Transform player;

    CustomerController current;

    void Start()
    {
            //EĞER player transform'unu koymayı unutursam otomatik bulur / yani ? :D
         if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
            else Debug.LogError("[CustomerSpawner] Player bulunamadı! Player objesine 'Player' tag ver.");
        }


        SpawnOneCustomer();
    }

    public void SpawnOneCustomer()
    {
        if (current != null) return;

        var go = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation); //NPC prefab oluşturduk seçilen üzerinden.
        current = go.GetComponent<CustomerController>();
        if(current == null) current = go.AddComponent<CustomerController>();

        current.Init(talkPoint, exitPoint); // Burada gönderiyoruz konumları
        current.BeginEnterShop(); // Burada da giriş başlıyor.
        current.SetPlayer(player);
    }
}
