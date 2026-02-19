using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCoordinator : MonoBehaviour
{
    [Header("Bays (Assign in Inspector)")]
    [SerializeField] List<ServiceBay> bays = new List<ServiceBay>();

    //Bay bekleyen müşteriler (Deal accepted ama bay yok)
    readonly Queue<CustomerController> waitingQueue = new Queue<CustomerController>();

    void Awake()
    {
        foreach(var b in bays)
        {
            if(b == null) continue;
            b.OnReleased -= HandleBayReleased;
            b.OnReleased += HandleBayReleased;

        }
    }

    /// <summary>
    /// Player "Tamam abi" dediğinde çağrılacak.
    /// Bay varsa atar ve iş başlatır. Bay yoksa kuyruğa laır
    /// </summary>
    
    public void DealAccepted(CustomerController customer, TireOrder order)
    {
        if(customer == null || order == null )
        {
            Debug.LogWarning("[Coordinator] DealAccepted missing args!");
            return;
        }

        var bay = FindFreeBay();
        if (bay != null)
        {
            AssignAndStart(customer, order, bay);
        }
        else
        {
            waitingQueue.Enqueue(customer);
            customer.SetWaitingForBay(true);
            Debug.Log("[Coordinator] no free bay -> customer queued.");
        }
    }

    ServiceBay FindFreeBay()
    {
        foreach(var b in bays)
        {
            if(b != null && !b.IsOccupied)
                    return b;
        }
        return null;  
    }

    void AssignAndStart(CustomerController customer, TireOrder order, ServiceBay bay)
    {
        //1)Bay'i kilitle (dolu)
        bay.Occupy();

        //2-Customer'ın bu bay ile ilişkisini kuruyoruz.
        customer.SetAssignedBay(bay);
        customer.SetJobManager(bay.tireJob);
        customer.SetWaitingForBay(false);

        //3-işi başlat
        Debug.Log($"[Coordinator] Assigned {customer.name} -> {bay.BayId}, starting job");
        bay.tireJob.StartJob(order);

        //4 Customer tarafında "job başladı" flag'i
        customer.NotifyJobStarted();
    }


    void HandleBayReleased(ServiceBay bay)
    {
        if(bay == null) return;

        if(waitingQueue.Count == 0)
                return;

        var nextCustomer = waitingQueue.Dequeue();

        //Kuyruktaki customer'ın order'ı customer tarafında duruyor(pendingOrder)
        var order = nextCustomer.GetPendingOrder();
        if(order == null)
        {
            Debug.LogWarning("[Coordinator] queued customer has  no pending order!");
            return;
        }        

        AssignAndStart(nextCustomer, order, bay);
        Debug.Log($"[Coordinator] Bay freed -> assigned queued customer to {bay.BayId}");
    }
}
