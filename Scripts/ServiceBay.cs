using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceBay : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] string bayId = "Bay-1";

    [Header("Points (optional but recommend)")]
    public Transform carSpawnPoint;
    public Transform talkPoint;
    public Transform exitPoint;

    [Header("Bay Systems (Must be inside this Bay)")]
    public CarJobController carJob;
    public TireJobManager tireJob;

    public bool IsOccupied {get; private set; }

    public event Action<ServiceBay> OnReleased;

    void Awake()
    {
        if(carJob == null)
                carJob = GetComponentInChildren<CarJobController>(true);

        if(tireJob == null)
                tireJob = GetComponentInChildren<TireJobManager>(true);

        //optional 
        if(carSpawnPoint == null)
                carSpawnPoint = transform;               

        if(tireJob != null && carJob != null)
        {
            tireJob.BindBay(carJob);
        }                 

        Debug.Log($"[Bay] Awake {bayId} -> carJob={(carJob != null)} tireJob={(tireJob != null)}");
    }

    public string BayId => bayId;

    public void Occupy()
    {
        IsOccupied = true;
        Debug.Log($"[BAY] {bayId} occupied");
    }

    public void Release()
    {
        IsOccupied = false;
        // Debug.Log($"[Bay] {bayId} released");
        OnReleased?.Invoke(this);
    }
}
