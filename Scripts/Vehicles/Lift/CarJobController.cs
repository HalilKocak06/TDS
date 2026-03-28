using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;


public class CarJobController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] List<CarSpawnData> cars = new List<CarSpawnData>(); //Spawn edilecek araba
    [SerializeField] Transform liftParkingSpot; //Arabanın duracağı nokta

    GameObject currentCar; //sahnedeki araba için kullandığımı bir referans . (bu başka bir obje de olabilir sadece referans)
    public GameObject CurrentCar => currentCar; // DIşarıdan okumamıza yarıyor. 

    [Header("Selection")]
    [SerializeField] int selectedCarIndex = 0;
    [SerializeField] bool debugKeyboardSelect = true;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F)) //F 'e bastığında aracı lifte atar .
        {
            SpawnCarAtLift();
        }
        if(Input.GetKeyDown(KeyCode.K)) //G'ye bastığında aracı kaldırır.
        {
            RemoveCar();
        }

        // İstersen debug için 1-2-3 ile araba seç
        if (debugKeyboardSelect)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetSelectedCarIndex(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetSelectedCarIndex(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetSelectedCarIndex(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetSelectedCarIndex(3);
            }
            if(Input.GetKeyDown(KeyCode.Alpha5))
            {
                SetSelectedCarIndex(4);
            }

            if(Input.GetKeyDown(KeyCode.Alpha6))
            {
                SetSelectedCarIndex(5);
            }
            if(Input.GetKeyDown(KeyCode.Alpha7))
            {
                SetSelectedCarIndex(6);
            }
            if(Input.GetKeyDown(KeyCode.Alpha8))
            {
                SetSelectedCarIndex(7);
            }
            if(Input.GetKeyDown(KeyCode.Alpha9))
            {
                SetSelectedCarIndex(8);
            }
        }
    }

    public void SetSelectedCarIndex(int index)
    {
        if (cars == null || cars.Count == 0)
        {
            Debug.LogWarning("[CarJob] Car list is empty.");
            return;
        }

        if (index < 0 || index >= cars.Count)
        {
            Debug.LogWarning("[CarJob] Invalid car index: " + index);
            return;
        }

        selectedCarIndex = index;
        Debug.Log("[CarJob] Selected car index -> " + selectedCarIndex + " | " + cars[selectedCarIndex].carId);
    }

    public GameObject SpawnCarAtLift()
    {
        //Araba var ise spawn etme
        if (currentCar != null)  return currentCar;

        CarSpawnData  data = GetSelectedCarPrefab();
        if(data == null || data.prefab == null)
        {
            Debug.LogError("[CARJOB] No valid car prefab selected");
            return null;
        }

        currentCar = Instantiate( //Bu tam olarak yeni bir kopya oluşturur.
            data.prefab, //bu kopyanın oluşacaği obje
            liftParkingSpot.position, //bu diğer objenin konumu
            liftParkingSpot.rotation // bu objnenin rotasyonu
        );

        //Araba LiftParkşngSpot'un child'I olacak
        currentCar.transform.SetParent(liftParkingSpot);

        //localleri sıfırlayalım ki tam noktaya otursun
        currentCar.transform.localPosition = data.localSpawnOffset;
        currentCar.transform.localRotation = Quaternion.Euler(data.localSpawnEulerOffset);
        Debug.Log($"[CarJob] Spawned -> {data.carId} | localOffset={data.localSpawnOffset}");

        return currentCar;
    }

    public void RemoveCar()
    {
        if(currentCar == null) return;

        Destroy(currentCar);
        currentCar = null;
    }

    public void DespawnCar()
    {
        if(currentCar == null) return;

        Destroy(currentCar);
        currentCar = null;

        Debug.Log("[CARJOB] Car despawned");
    }

    CarSpawnData GetSelectedCarPrefab()
    {
        if (cars == null || cars.Count == 0)
            return null;

        if (selectedCarIndex < 0 || selectedCarIndex >= cars.Count)
            return null;

        return cars[selectedCarIndex];
    }
}
