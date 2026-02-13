using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarJobController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] GameObject carPrefab; //Spawn edilecek araba
    [SerializeField] Transform liftParkingSpot; //Arabanın duracağı nokta

    GameObject currentCar; //sahnedeki araba için kullandığımı bir referans . (bu başka bir obje de olabilir sadece referans)
    public GameObject CurrentCar => currentCar; // DIşarıdan okumamıza yarıyor. 

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
    }

    public GameObject SpawnCarAtLift()
    {
        //Araba var ise spawn etme
        if (currentCar != null)  return currentCar;

        currentCar = Instantiate( //Bu tam olarak yeni bir kopya oluşturur.
            carPrefab, //bu kopyanın oluşacaği obje
            liftParkingSpot.position, //bu diğer objenin konumu
            liftParkingSpot.rotation // bu objnenin rotasyonu
        );

        //Araba LiftParkşngSpot'un child'I olacak
        currentCar.transform.SetParent(liftParkingSpot);

        //localleri sıfırlayalım ki tam noktaya otursun
        currentCar.transform.localPosition = Vector3.zero;
        currentCar.transform.localRotation = Quaternion.identity;
        Debug.Log("[CarJob] Car spawned -> " + currentCar.name);

        return currentCar;
    }

    public void RemoveCar()
    {
        if(currentCar == null) return;

        Destroy(currentCar);
        currentCar = null;
    }
}
